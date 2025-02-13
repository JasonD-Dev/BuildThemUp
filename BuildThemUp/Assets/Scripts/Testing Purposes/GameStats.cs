using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameStats
{
    public int enemiesKilled;
    public int scrapCollected;
    public int scrapSpent;
    public int wavesSurvived;
    public int satellitesCompleted;

    public GameStats(int killed, int collected, int spent, int survived, int completed)
    {
        enemiesKilled = killed;
        scrapCollected = collected;
        scrapSpent = spent;
        wavesSurvived = survived;
        satellitesCompleted = completed;
    }
}