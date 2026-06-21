using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public UmaAgent umaAgent;
    public StatsMenu statsMenu;
    public UmaHealth umaHealth;
    public TextZoneManager tzm;
    public Renderer umaRenderer;
    public List<Texture2D> images;
    public int turns = 20;
    private int currentTurn;
    private void Start()
    {
        currentTurn = turns;
        umaRenderer.material.mainTexture = images[7];
    }

    private void Update()
    {
        
    }

    public void RestartSession()
    {
        umaHealth.Reset();

        umaAgent.OnEpisodeBegin();

        tzm.ActivateChat(true);

        statsMenu.score.text = "----------------\r\n\r\n\r\n\r\n\r\n\r\n\r\n(Finish Session)";
        statsMenu.AtStart();
        statsMenu.UpdateStats(umaHealth);

        ChangeImage(7);
        currentTurn = turns;

    }

    public void ChangeImage(int index)
    {
        currentTurn--;
        umaRenderer.material.mainTexture = images[index];

        if (currentTurn == 0)
        {
            statsMenu.ShowPerformance(umaHealth);
            tzm.ActivateChat(false);
            statsMenu.restartBtn.gameObject.SetActive(true);
        }
    }
}
