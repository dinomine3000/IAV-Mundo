using System.Collections.Generic;
using UnityEngine;

public class GhostGameManager : MonoBehaviour
{
    [Tooltip("Define the ghosts here. Game objects with the GhostLLM ")]
    [SerializeField] private List<GhostLLM> ghosts;
    public GhostLLM currentGhost {private set; get;}= null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int i = 0;
        foreach(GhostLLM go in ghosts)
            go.GetComponent<GhostLLM>().id = i++;
        SelectRandomGhost();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SelectRandomGhost()
    {
        if(ghosts.Count == 0) return;
        currentGhost = ghosts[Random.Range(0, ghosts.Count)];
    }

    public void Restart()
    {
        SelectRandomGhost();
    }

    public bool ValidateChoice(int chosenIdx){return currentGhost == null ? true : currentGhost.GetComponent<GhostLLM>().id == chosenIdx;}
}
