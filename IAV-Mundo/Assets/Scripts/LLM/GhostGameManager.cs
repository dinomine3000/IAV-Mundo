using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GhostGameManager : MonoBehaviour
{
    [Tooltip("Define the ghosts here. Game objects with the GhostLLM ")]
    [SerializeField] private List<GhostLLM> ghosts;
    [SerializeField] private GameObject buttonTemplate;
    [SerializeField] private Transform buttonCenter;
    [SerializeField] private LayerMask buttonLayerMask;
    [SerializeField] private TextMeshProUGUI answer;

    public GhostLLM currentGhost {private set; get;}= null;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int numberOfGhosts = 0;
        foreach(GhostLLM go in ghosts)
            go.GetComponent<GhostLLM>().id = numberOfGhosts++;
        SelectRandomGhost();
        if (numberOfGhosts == 1)
        {
            Vector3 spawnPosition = transform.position;
            CreateGhostChoice(spawnPosition, 0, ghosts[0].name);
            return;
        }
        float width = 7f;
        float startX = transform.position.x - (width / 2f);
        float spacing = width / (numberOfGhosts - 1);

        for (int i = 0; i < numberOfGhosts; i++)
        {
            float currentX = startX + (i * spacing);
            Vector3 spawnPosition = new Vector3(currentX, transform.position.y, transform.position.z);
            CreateGhostChoice(spawnPosition, i, ghosts[i].name);
        }
    }

    private void CreateGhostChoice(Vector3 position, int idx, string name)
    {
        GhostInteraction ghost = Instantiate(buttonTemplate, position, Quaternion.identity, transform).GetComponent<GhostInteraction>();
        ghost.idx = idx;
        ghost.SetName(name);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            GhostInteraction interaction = RaycastChoice();
            if(interaction == null || interaction.Cooldown()) return;
            int idx = interaction.idx;
            if(idx == -1) return;
            bool win = ValidateChoice(idx);
            if (win)
                Debug.Log("Correct");
            else
                Debug.Log("Wrong");
            if(win) Restart();
        }
    }
    
    private GhostInteraction RaycastChoice()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, buttonLayerMask))
        {
            return hit.collider.gameObject.GetComponent<GhostInteraction>() ?? null;
        }

        return null;
    }

    private void SelectRandomGhost()
    {
        if(ghosts.Count == 0) return;
        currentGhost = ghosts[Random.Range(0, ghosts.Count)];
        Debug.Log($"Selected {currentGhost.ghostName}");
    }

    public void Restart()
    {
        SelectRandomGhost();
        answer.text = "";
    }

    public bool ValidateChoice(int chosenIdx){return currentGhost == null ? true : currentGhost.GetComponent<GhostLLM>().id == chosenIdx;}
}

