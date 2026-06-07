
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GhostInteraction : MonoBehaviour
{
    private float cooldown = 2;
    private bool canReply = true;
    public int idx;
    [SerializeField] private Material matVerde;
    [SerializeField] private Material matVermelho;
    public string ghostName;
    
    public void SetName(string ghostName)
    {
        this.ghostName = ghostName;   
        GameObject textObject = new GameObject("HoveringNameText");
        textObject.transform.SetParent(transform);
        textObject.transform.localPosition = new Vector3(0, 2, 0);
        textObject.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        textObject.transform.localScale= new(0.1f, 0.2f, 0.1f);
        TextMeshPro textComponent = textObject.AddComponent<TextMeshPro>();
        textComponent.text = $"Guess {ghostName}";
        textComponent.alignment = TextAlignmentOptions.Center;
    }

    public bool Cooldown()
    {
        if(!canReply) return true;
        StartCoroutine(CooldownRoutine());
        return false;
    }

    IEnumerator CooldownRoutine()
    {
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        renderer.material = matVermelho;
        canReply = false;
        yield return new WaitForSeconds(cooldown);
        renderer.material = matVerde;
        canReply = true;
    }
}