using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class UmaLLM : MonoBehaviour
{
    [SerializeField] private string apiUrl = "http://localhost:11434/api/chat";
    [SerializeField] private string modelName = "qwen2.5:7b";

    [SerializeField] private UmaLLMConfig umaConfig;

    [Tooltip("Mensagens user+assistant a manter (não conta o system prompt).")]
    [SerializeField] private int windowSize = 10;

    [Tooltip("Mesmo com schema validation, alguns modelos chamam tools fora da lista. Rede de segurança.")]
    [SerializeField] private bool validateAgainstDeclaredTools = true;
    private TMP_Text agentReplyText;    
    private readonly List<ChatMessage> history = new();
    private readonly Dictionary<string, Action<JObject>> handlers = new();
    public int id = -1;

    // ── Registo de tools (chamado pelos componentes que sabem agir) ──────────

    public void RegisterTool(string name, Action<JObject> handler)
    {
        handlers[name] = handler;
    }

    // ── Ciclo de vida ────────────────────────────────────────────────────────

    private void Awake()
    {
        var go = GameObject.FindWithTag("AgentAnswer");
        if (go != null) agentReplyText = go.GetComponent<TMP_Text>();
    }

    public void Talk(string message)
    {
        if (agentReplyText != null) agentReplyText.text = "<a pensar...>";
        StartCoroutine(SendToLLM(message));
    }

    // ── Pipeline principal ───────────────────────────────────────────────────

    private IEnumerator SendToLLM(string userMessage)
    {
        if (umaConfig == null)
        {
            if (agentReplyText != null) agentReplyText.text = "AgentConfig não atribuído.";
            yield break;
        }

        history.Add(new ChatMessage { role = "user", content = userMessage });
        TrimHistory();

        // Constrói payload manualmente com JObject — JsonUtility não cobre o
        // schema de tools (objectos com chaves variáveis no `parameters`).
        var requestJson = BuildRequestJson(userMessage);

        using var http = new UnityWebRequest(apiUrl, "POST");
        http.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestJson));
        http.downloadHandler = new DownloadHandlerBuffer();
        http.SetRequestHeader("Content-Type", "application/json");

        yield return http.SendWebRequest();

        if (http.result != UnityWebRequest.Result.Success)
        {
            if (agentReplyText != null) agentReplyText.text = umaConfig.DefaultErrorAnswer;
            Debug.LogWarning($"LLM error: {http.error}\n{http.downloadHandler?.text}");
            history.RemoveAt(history.Count - 1);
            yield break;
        }

        JObject resp;
        try { resp = JObject.Parse(http.downloadHandler.text); }
        catch (Exception e) { Debug.LogError($"JSON parse: {e.Message}"); yield break; }

        var message = resp["message"] as JObject;
        if (message == null) yield break;

        // 1) Texto falado pelo NPC (T2 do feedback)
        string spoken = message["content"]?.ToString();
        if (string.IsNullOrWhiteSpace(spoken)) spoken = "...";
        if (agentReplyText != null) agentReplyText.text = spoken;
        history.Add(new ChatMessage { role = "assistant", content = spoken });
        TrimHistory();

        // 2) Acções a executar (T3 do feedback fica nos handlers)
        var toolCalls = message["tool_calls"] as JArray;
        if (toolCalls == null) yield break;

        foreach (var call in toolCalls)
        {
            var fn = call["function"] as JObject;
            if (fn == null) continue;

            string toolName = fn["name"]?.ToString()?.Trim();
            if (string.IsNullOrEmpty(toolName)) continue;

            if (validateAgainstDeclaredTools && !umaConfig.IsDeclared(toolName))
            {
                Debug.LogWarning($"Tool rejeitada (não declarada no AgentConfig): '{toolName}'");
                continue;
            }

            if (!handlers.TryGetValue(toolName, out var handler))
            {
                Debug.LogWarning($"Tool sem handler registado: '{toolName}'");
                continue;
            }

            var arguments = (fn["arguments"] as JObject) ?? new JObject();
            Debug.Log($"Tool call: {toolName} args={arguments.ToString(Formatting.None)}");

            try { handler(arguments); }
            catch (Exception e) { Debug.LogError($"Handler '{toolName}' falhou: {e.Message}"); }
        }
    }

    // ── Construção do payload ────────────────────────────────────────────────

    private string BuildRequestJson(string userMessage)
    {
        var messages = new JArray
        {
            new JObject { ["role"] = "system",  ["content"] = BuildSystemPrompt() },
        };
        foreach (var m in history)
            messages.Add(new JObject { ["role"] = m.role, ["content"] = m.content });

        var tools = new JArray();
        foreach (var t in umaConfig.Tools)
            tools.Add(t.ToToolJson());

        var req = new JObject
        {
            ["model"]    = modelName,
            ["stream"]   = false,
            ["messages"] = messages,
            ["tools"]    = tools,
        };
        return req.ToString(Formatting.None);
    }

    private string BuildSystemPrompt()
    {
        var sb = new StringBuilder();
        sb.AppendLine(umaConfig.ContextPrompt);
        sb.AppendLine();
        sb.AppendLine(umaConfig.AnswerGuideline);
        return sb.ToString();
    }

    private void TrimHistory()
    {
        while (history.Count > windowSize)
            history.RemoveAt(0);
    }
}
