using System.Collections.Generic;
using UnityEngine;

//copiado do AgentConfigV2

[CreateAssetMenu(fileName = "GhostConfig", menuName = "LLM/GhostConfig")]
public class GhostConfig : ScriptableObject
{
    [Header("Identidade")]
    [SerializeField] public string ghostName = "Fantasma";

    [Header("Personalidade (system prompt)")]
    [TextArea(3, 10)] [SerializeField] private string contextPrompt =
        "És um espírito antigo que assombra esta casa. Falas em português europeu. " +
        "Reages ao jogador de forma sinistra mas críptica. Nunca quebras o personagem.";

    [Header("Como responder (tom, comprimento)")]
    [TextArea(3, 10)] [SerializeField] private string answerGuideline =
        "Responde sempre em 1-2 frases curtas, em português europeu. " +
        "Tom: perturbador, misterioso. Não listes as tuas capacidades.";

    [Header("Tools — declaradas via API com schema")]
    [SerializeField] private List<ToolEntryV2> tools = new()
    {
        new ToolEntryV2 {
            name = "Flicker",
            description = "Pisca as luzes",
            parameters = new List<ToolParam> {
                new ToolParam {
                    name = "duration",
                    description = "Duração do flicker em segundos, por defeito 3 segundos.",
                    required = false,
                    allowedValues = new(),
                    type = "int"
                }
            },
        },
        new ToolEntryV2 {
            name = "BangDoor",
            description = "Abre ou fecha a porta",
            parameters =  new()
        },
        new ToolEntryV2 {
            name = "ShowOrbs",
            description = "Mostra esferas fantasma",
            parameters = new List<ToolParam> {
                new ToolParam {
                    name = "count",
                    description = "Quantas esferas mostrar. Idealmente um valor entre 3 e 10",
                    required = true,
                    allowedValues = new(),
                    type = "int"
                }
            },
        },
        new ToolEntryV2 {
            name = "BleedingWalls",
            description = "Faz as paredes sangrar",
            parameters = new()
        }
    };

    [Header("Fallback")]
    [SerializeField] private string defaultErrorAnswer = "...";
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
