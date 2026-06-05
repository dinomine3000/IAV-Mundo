using UnityEngine;
using UnityEngine.InputSystem;

// Permite seleccionar um NPC com clique. NPCs têm de ter a tag "SmartAgent".
// Anexar à Camera principal.
public class NPCSelector : MonoBehaviour
{
    private Camera cam;
    private GameObject selectedNPC;

    private void Awake() { cam = Camera.main; }

    private void Update()
    {
       if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        // Use Mouse.current.position.ReadValue() instead of Input.mousePosition
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out var hit) && hit.collider.CompareTag("SmartAgent"))
        {
            selectedNPC = hit.collider.gameObject;
            Debug.Log($"NPC seleccionado: {selectedNPC.name}");
        }
    }

    public GameObject GetSelectedNPC() => selectedNPC;
}
