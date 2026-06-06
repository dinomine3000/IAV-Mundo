using System.Collections.Generic;
using UnityEngine;

//copiado do AgentConfigV2

[CreateAssetMenu(fileName = "GhostConfig", menuName = "LLM/GhostConfig")]
public class GhostConfig : ScriptableObject
{
    [Header("Personalidade (system prompt)")]
    [TextArea(3, 10)] [SerializeField] private string contextPrompt =
        "Tu és um guarda velho num portão de pedra. Falas em português, de forma seca. " +
        "Estás cansado de estar parado — se o jogador parecer perdido, ofereces-te para " +
        "patrulhar ou para ir ver algo, mas sem enumerar a tua lista de tarefas como um menu.";

    [Header("Como responder (tom, comprimento)")]
    [TextArea(3, 10)] [SerializeField] private string answerGuideline =
        "Responde em 1-2 frases, em português europeu, sempre em personagem.";

    [Header("Tools — declaradas via API com schema")]
    [SerializeField] private List<ToolEntryV2> tools = new()
    {
        new ToolEntryV2 {
            name = "Flicker",
            description = "Pisca as luzes",
            parameters = new List<ToolParam> {
                new ToolParam {
                    name = "duration",
                    description = "Duração do flicker em segundos",
                    required = false
                },
            },
        }
    };

    [Header("Fallback")]
    [SerializeField] private string defaultErrorAnswer = "Desculpa, não consegui responder.";

    public string ContextPrompt    => contextPrompt;
    public string AnswerGuideline  => answerGuideline;
    public IReadOnlyList<ToolEntryV2> Tools => tools;
    public string DefaultErrorAnswer => defaultErrorAnswer;

    public bool IsDeclared(string toolName)
    {
        foreach (var t in tools)
            if (t.name == toolName) return true;
        return false;
    }
}
