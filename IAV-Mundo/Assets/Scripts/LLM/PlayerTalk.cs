using TMPro;
using UnityEngine;

// Liga o input do jogador (TMP_InputField + Botão "Falar") ao NPC seleccionado
// pelo NPCSelector. Anexar ao Canvas; ligar OnClick do botão a este OnSend.
public class PlayerTalk : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private NPCSelector selector;

    public void OnSend()
    {
        Debug.Log("Sending");
        if (inputField == null || selector == null) return;

        GhostLLM npc = selector.GetSelectedNPC();
        if (npc == null || string.IsNullOrWhiteSpace(inputField.text)) return;

        string message = inputField.text;
        inputField.text = "";
        npc.Talk(message);
    }
}
