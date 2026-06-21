using TMPro;
using UnityEngine;

// Liga o input do jogador (TMP_InputField + Botão "Falar") ao NPC seleccionado
// pelo NPCSelector. Anexar ao Canvas; ligar OnClick do botão a este OnSend.
public class UmaTalk : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private UmaLLM agent;

    public void OnSend()
    {
        Debug.Log("Sending");
        if (inputField == null) return;

        if (agent == null || string.IsNullOrWhiteSpace(inputField.text)) return;

        string message = inputField.text;
        inputField.text = "";
        agent.Talk(message);
    }
}
