using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI enemiesKilledText;
    [SerializeField] TextMeshProUGUI scrapCollectedText;
    [SerializeField] TextMeshProUGUI scrapSpentText;
    [SerializeField] TextMeshProUGUI wavesSurvivedText;
    [SerializeField] TextMeshProUGUI satellitesRepairedText;

    public static GameStats currentStats;

    public void Setup()
    {
        gameObject.SetActive(true);
        enemiesKilledText.text = "Sys> " + currentStats.enemiesKilled + " enemies killed.";
        scrapCollectedText.text = "Sys> " + currentStats.scrapCollected + " scrap collected.";
        scrapSpentText.text = "Sys> " + currentStats.scrapSpent + " scrap spent.";
        wavesSurvivedText.text = "Sys> " + currentStats.wavesSurvived + " waves survived.";
        satellitesRepairedText.text = "Sys> " + currentStats.satellitesCompleted + " satellites repaired.";
    }
    private void Awake()
    {
        Setup();
    }
}
