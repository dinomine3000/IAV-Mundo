using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextZoneManager : MonoBehaviour
{
    [Header("Text Zone")]
    public Button falarBtn;
    public TMP_InputField mensageField;

    public void ActivateChat(bool active)
    {
        falarBtn.interactable = active;
        mensageField.interactable = active;
    }
}
