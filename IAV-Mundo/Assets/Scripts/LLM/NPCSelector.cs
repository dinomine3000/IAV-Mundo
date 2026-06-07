using UnityEngine;
using UnityEngine.InputSystem;

public class NPCSelector : MonoBehaviour
{
    [SerializeField] private GhostGameManager gameManager;

    public GhostLLM GetSelectedNPC() => gameManager.currentGhost;
}
