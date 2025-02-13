using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager instance { get; private set; }

    public GameStats currentGameStats;
    public int totalScrapCount { get; private set; } = 100;
    public int totalScrapSpent { get; private set; } = 0;
    public int enemiesKilled { get; private set; } = 0;

    public float gravity { get; private set; } = 9.81f;

    //Win-lose variables.
    [Header("Scenes")]
    [SerializeField] string mGameWinSceneName = "Game_Win";
    [SerializeField] string mGameOverSceneName = "Game_Over";

    //Main base Varaibles
    [Header("Player and Main Base")]
    [field: SerializeField] public Transform mainBaseTransform { get; private set; }
    [field: SerializeField] public Transform playerTransform { get; private set; }
    public Vector3 mainBasePostion  => mainBaseTransform.position;
    public Vector3 playerPosition => playerTransform.position;

    [Header("Satellites")]
    public SatelliteController[] satellites;
    public float winTime = 1; // time to win in seconds from first satellite activation
    public int numActiveSatellites = 0;
    [SerializeField] TextMeshProUGUI mTimerText;
    [SerializeField] TextMeshProUGUI mObjectiveTimerText;

    //Scrap Varaibles
    [Header("Scrap")]
    [SerializeField] TextMeshProUGUI mScrapText;
    [SerializeField] Transform mScrapFeed;
    [SerializeField] ScrapFeedItem mScrapFeedPrefab;
    [field: SerializeField] public int totalScrap { get; private set; } = 100;
    [field: SerializeField] public int maxScrap { get; private set; } = 6000;
    [SerializeField] private bool mInstantScrapToPlayer;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        UpdateScrapText();
    }

    private void FixedUpdate()
    {
        float tTotalCommTime = 0;
        int tActiveSats = 0;
        
        foreach (var i in satellites)
        {
            if (i.activated)
            {
                tActiveSats++;
                tTotalCommTime += i.communicationTime;
            }
        }
        
        // set timer text visibility depending on state change
        if (numActiveSatellites <= 0 && tActiveSats > 0)
            mTimerText.gameObject.SetActive(true);
        else if (numActiveSatellites > 0 && tActiveSats <= 0)
            mTimerText.gameObject.SetActive(false);

        numActiveSatellites = tActiveSats;
        
        if (numActiveSatellites > 0)
        {
            var tEta = winTime / numActiveSatellites - tTotalCommTime / numActiveSatellites;
            
            // set timer text at top of screen
            var tMinutes = (int)(tEta / 60);
            var tSeconds = (int)(tEta % 60);
            mTimerText.SetText($"{tMinutes}:{tSeconds:D2}");
            //mObjectiveTimerText.text = $"{tMinutes}:{tSeconds:D2}";

            var tRemaining = winTime - tTotalCommTime;
            
            // we won!!
            if (tRemaining <= 0)
                SetGameWin();
        }
    }

    public bool AddScrap(int aAmount)
    {
        if (totalScrap + aAmount > maxScrap)
        {
            AddScrapFeed("FULL");
            return false;
        }

        totalScrap += aAmount;
        AddScrapFeed(aAmount);
        TotalScrapCount(aAmount);
        UpdateScrapText();
        return true;
    }

    public void TotalScrapCount(int aAmount)
    {
        totalScrapCount += aAmount;
    }
    public void TotalScrapSpent(int aAmount)
    {
        totalScrapSpent += aAmount;
    }
    
    private string[] mNoScrapMessages =
    {
        "DECLINED",
        "UR BROKE LOL",
        "ACCESS DENIED",
        "TOO POOR",
        "NO SCRAP?"
    };

    public bool SpendScrap(int aAmount)
    {
        if (HasSufficientScrap(aAmount) == false)
        {
            AddScrapFeed(mNoScrapMessages[Random.Range(0, mNoScrapMessages.Length)]);
            return false;
        }

        totalScrap -= aAmount;
        AddScrapFeed(-aAmount);
        TotalScrapSpent(aAmount);
        UpdateScrapText();
        return true;
    }

    public void AddScrapFeed(int aAmount)
    {
        var tScrapFeedItem = Instantiate(mScrapFeedPrefab, mScrapFeed);
        tScrapFeedItem.Setup(aAmount);
    }
    
    public void AddScrapFeed(string aMessage)
    {
        var tScrapFeedItem = Instantiate(mScrapFeedPrefab, mScrapFeed);
        tScrapFeedItem.Setup(aMessage);
    }

    public bool HasSufficientScrap(int aAmount)
    {
        return (totalScrap - aAmount >= 0);
    }

    private void UpdateScrapText()
    {
        if (mScrapText != null)
        {
            var tText = totalScrap.ToString();
            mScrapText.text = tText;

            var tLocalPos = mScrapFeed.localPosition;
            tLocalPos.x = mScrapText.GetPreferredValues(tText).x + 10; // 10 is a margin
            mScrapFeed.localPosition = tLocalPos;
        }
    }

    public void SetGameWin()
    {
        // Populate GameStats
        currentGameStats = new GameStats(
            enemiesKilled, // Enemies killed stat
            totalScrapCount, // Total scrap collected stat
            totalScrapSpent, // Total scrap spent stat
            EnemiesManager.instance.waveNumber, // Waves survived stat
            numActiveSatellites // Satellites repaired stat
        );
        ScoreManager.currentStats = currentGameStats;
        Cursor.lockState = CursorLockMode.Confined;
        SceneManager.LoadScene(mGameWinSceneName);
    }

    public void SetGameOver()
    {
        // Populate GameStats
        currentGameStats = new GameStats(
            enemiesKilled, // Enemies killed stat
            totalScrapCount, // Total scrap collected stat
            totalScrapSpent, // Total scrap spent stat
            EnemiesManager.instance.waveNumber, // Waves survived stat
            numActiveSatellites // Satellites repaired stat
        );
        ScoreManager.currentStats = currentGameStats;
        Cursor.lockState = CursorLockMode.Confined;
        SceneManager.LoadScene(mGameOverSceneName);
    }

    public void IncrementKillCount(int aAmount = 1) //Incase you for what even reason what to add more than 1.
    {
        enemiesKilled += aAmount;
    }

    public bool IsInstantAddScrapToPlayer()
    {
        return mInstantScrapToPlayer;
    }
}
