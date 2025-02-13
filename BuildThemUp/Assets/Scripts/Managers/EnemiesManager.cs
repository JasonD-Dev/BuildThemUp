using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemiesManager : MonoBehaviour
{
    public static EnemiesManager instance { get; private set; }
    
    public List<Enemy> allEnemies { get; private set; } = new List<Enemy>();
    private int mNumberOfEnemies => allEnemies.Count;

    [Header("Components")]
    [SerializeField] private Enemy[] mEnemyPrefabs;
    [SerializeField] private Enemy[] mBossPrefabs;
    [SerializeField] private Enemy mFinalBoss;
    [field: SerializeField] public Transform[] mainBaseTargets;
    [SerializeField] SpawnPoint[] mEnemySpawnPoints;

    [Header("Enemy Spawning")]
    [SerializeField] int mSpawnCountPerWave = 10;
    [SerializeField] private AnimationCurve mSpawnCountMultiplier = AnimationCurve.Linear(0, 1, 1, 1);
    [SerializeField] float mSpawnDelay = 0.5f;
    [SerializeField] float mWaveDelay = 10;
    [SerializeField] int mMaxEnemies = 100;
    [field: SerializeField] public int maxWaves { get; private set; } = 4;

    [Header("Warning UI")]
    [SerializeField] private TextMeshProUGUI mWarningUI;
    private float Timer = 0;

    [Header("Satellites")]
    [SerializeField] private SatelliteController mSatelliteA;
    private bool mAlreadyActiveA;
    [SerializeField] private SatelliteController mSatelliteB;
    private bool mAlreadyActiveB;
    [SerializeField] private SatelliteController mSatelliteC;
    private bool mAlreadyActiveC;

    [Header("Keeping track of Waves/Enemies")]
    [SerializeField] private TextMeshProUGUI mEnemiesRemainingUI;
    [SerializeField] private TextMeshProUGUI mWaveNumberUI;
    //keeping track
    [field: SerializeField] public int waveNumber { get; private set; } = 1;

    [SerializeField] private float mWaveTimer;
    private float mSpawnTimer;
    [SerializeField] private int mRemainingSpawns;

    [SerializeField] private bool mDisabled;

    // Ending Sequence
    private bool mEndingSequenceEnabled = false;
    private string[] mFinalBossText; // Warning text of final boss
    private int mStringArrayCount = 0; // Position of array

    private void Awake()
    {
        // why no destroy checks?
        // if player goes back to menu -> restarts scene:
        // 1. old instance will be destroyed (but Instance won't be null)
        // 2. we need the new enemiesmanager in there anyway
        instance = this;    }

    private void Start()
    {
        //Sanity Checks
        if (mEnemySpawnPoints.Length <= 0)
        {
            Log.Warning(this, "Instantiated with no spawn points! No enemies will be spawned!");
            mDisabled = true;
        }

        if (mEnemyPrefabs.Length <= 0)
        {
            Log.Warning(this, "Instantiated with no enemy prefabs! No enemies will be spawned!");
            mDisabled = true;
        }

        if (mSatelliteA == null || mSatelliteB == null || mSatelliteC == null)
        {
            Log.Warning(this, "Satellites not instantiated for boss waves!");
        }

            if (mWaveNumberUI == null || mEnemiesRemainingUI == null || mWarningUI == null)
        {
            Log.Warning(this, "UI not Instantiated for waves!!");
            mDisabled = true;
        }
        


        mWarningUI.text = "Wave Incoming";
        mWarningUI.enabled = true;

        mFinalBossText = new string[9];

        mRemainingSpawns = mSpawnCountPerWave;
    }

    void FixedUpdate()
    {
        if (!mEndingSequenceEnabled) // When ending sequence begins these values will be changed
        {
            // Fixed Nums for Waves and Enemies Remaining
            mWaveNumberUI.text = $"Wave {waveNumber}";
            mEnemiesRemainingUI.text = $"{mNumberOfEnemies} enemies remaining.";
        }

        if (mWarningUI.isActiveAndEnabled == true)
        {
            Timer += Time.deltaTime;
            if (Timer > 5)
            {
                mWarningUI.color = Color.black;
                mWarningUI.gameObject.SetActive(false);
                Timer = 0;
            }
        }

        mSpawnTimer -= Time.deltaTime;
        if (mNumberOfEnemies < mMaxEnemies)
        {
            mWaveTimer -= Time.deltaTime;
        }

        // Exit early. We don't have anything to do for now...
        if (mDisabled || mWaveTimer > 0 || mSpawnTimer > 0 || mNumberOfEnemies >= mMaxEnemies)
            return;

        // Boss Wave
        if (mSatelliteA.activated)
        {
            if (!mAlreadyActiveA)
            {
                BossWarning();
                Enemy tEnemy = SpawnBoss(0, mEnemySpawnPoints[3]);
                mSpawnTimer = mSpawnDelay;
                mAlreadyActiveA = true;
            }

        }
        if (mSatelliteB.activated) 
        {
            if (!mAlreadyActiveB)
            {
                BossWarning();
                Enemy tEnemy = SpawnBoss(1, mEnemySpawnPoints[0]);
                mSpawnTimer = mSpawnDelay;
                mAlreadyActiveB = true;
            }
                
        }
        if (mSatelliteC.activated)
        {
            if (!mAlreadyActiveC)
            {
                BossWarning();
                Enemy tEnemy = SpawnBoss(2, mEnemySpawnPoints[2]);
                mSpawnTimer = mSpawnDelay;
                mAlreadyActiveC = true;
            }
        }

        if (mRemainingSpawns > 0)
        {
            var tSpawnPoint = mEnemySpawnPoints[Random.Range(0, mEnemySpawnPoints.Length)];
            Enemy tEnemy = SpawnEnemy(Random.Range(0, mEnemyPrefabs.Length), tSpawnPoint);

            mSpawnTimer = mSpawnDelay;
            mRemainingSpawns--;
        }

        // Wave Over
        if (mRemainingSpawns < 1 && waveNumber < maxWaves && mNumberOfEnemies == 0)
        {
            NextWave();
        }

        if (mNumberOfEnemies == 0 && mRemainingSpawns == 0 && waveNumber == maxWaves)
        {
           if (!mEndingSequenceEnabled)
            {
                BeginEndingSequence();
            }
        }

        if (mEndingSequenceEnabled == true) {

            if (mWarningUI.isActiveAndEnabled == false && mStringArrayCount < mFinalBossText.Length-1)
            {
                mStringArrayCount++;
                BossWarning();
            }
            if (mStringArrayCount == mFinalBossText.Length - 1 && mNumberOfEnemies == 0)
            {
                mWaveNumberUI.text = "¥øü åřë τhë ønl¥ Ɇnëm¥ lëƒτ";
                mWaveNumberUI.color = Color.red;
                mWaveNumberUI.fontSize = 22;
                mEnemiesRemainingUI.text = "G ø ø ∂ b ¥ ë";
                mEnemiesRemainingUI.fontSize = 25;
                mEnemiesRemainingUI.color = Color.red;
                SpawnFinalBoss(mEnemySpawnPoints[2]);
            }
        }
    }
    
    public void AddEnemy(Enemy aEnemy)
    {
        allEnemies.Add(aEnemy);
    }

    public void RemoveEnemy(Enemy aEnemy)
    {
        allEnemies.Remove(aEnemy);
    }

    public Enemy DebugSpawnEnemy(Vector3 aSpawnPoint)
    {
        int tEnemyPrefabIndex = Random.Range(0, mEnemyPrefabs.Length);
        Enemy tEnemy = Instantiate(mEnemyPrefabs[tEnemyPrefabIndex], aSpawnPoint, Quaternion.identity, transform);
        AddEnemy(tEnemy);
        return tEnemy;
    }

    public Enemy DebugSpawnBoss(Vector3 aSpawnPoint)
    {
        int tBossPrefabIndex = Random.Range(0, mBossPrefabs.Length);
        Enemy tEnemy = Instantiate(mBossPrefabs[tBossPrefabIndex], aSpawnPoint, Quaternion.identity, transform);
        AddEnemy(tEnemy);
        return tEnemy;
    }

    private Enemy SpawnEnemy(int aEnemyPrefabIndex, SpawnPoint aSpawnPoint)
    {
        Enemy tEnemy = Instantiate(mEnemyPrefabs[aEnemyPrefabIndex], aSpawnPoint.SpawnPointPosition(), Quaternion.identity, transform);
        tEnemy.SetCheckPoint(aSpawnPoint.RandomCheckPoint());
        AddEnemy(tEnemy);
        return tEnemy;
    }

    private Enemy SpawnBoss(int aBossPrefabIndex, SpawnPoint aSpawnPoint)
    {
        Enemy tEnemy = Instantiate(mBossPrefabs[aBossPrefabIndex], aSpawnPoint.SpawnPointPosition(), Quaternion.identity, transform);
        tEnemy.SetCheckPoint(aSpawnPoint.RandomCheckPoint());
        AddEnemy(tEnemy);
        return tEnemy;
    }

    private void SpawnFinalBoss(SpawnPoint aSpawnPoint)
    {
        Enemy tEnemy = Instantiate(mFinalBoss, aSpawnPoint.SpawnPointPosition(), Quaternion.identity, transform);
        tEnemy.SetCheckPoint(aSpawnPoint.RandomCheckPoint());
        AddEnemy(tEnemy);
    }

    private void NextWave()
    {
        waveNumber++;
        mWarningUI.gameObject.SetActive(true);
        
        if (waveNumber == maxWaves)
        {
            mWarningUI.text = $"Final Wave Incoming!";
            mWarningUI.fontStyle = FontStyles.Bold;
        }
        else
        {
            mWarningUI.text = $"Wave {waveNumber} Incoming";
        }

        mWaveTimer = mWaveDelay;
        mRemainingSpawns = Mathf.RoundToInt(mSpawnCountPerWave * mSpawnCountMultiplier.Evaluate(waveNumber / (float)maxWaves));
        Log.Info(this, $"Next Wave ({waveNumber}): spawning {mRemainingSpawns} enemies");
    }

    private void BossWarning()
    {
        if (!mEndingSequenceEnabled)
        {
            mWarningUI.text = "Raid Boss Incoming";
        }
        else
        {
            if (mStringArrayCount < mFinalBossText.Length)
            {
                mWarningUI.text = mFinalBossText[mStringArrayCount];
            }
        }
        mWarningUI.color = Color.red;
        mWarningUI.gameObject.SetActive(true);
    }

    private void BeginEndingSequence()
    {
        Debug.Log("Final Sequence Beginning");
        mEndingSequenceEnabled = true;
        InstatiateFinalBossText();
        BossWarning();
        
    }

    private void InstatiateFinalBossText()
    {
        mFinalBossText[0] = "Final Wave IncoM.I..";
        mFinalBossText[1] = "W a r n .. n i n g ...";
        mFinalBossText[2] = "Err⚠r: System Malfunction \u26A0";
        mFinalBossText[3] = "";
        mFinalBossText[4] = "Y ø u . .";
        mFinalBossText[5] = "Yøu sêëm †ø b€ êñjøYîñg Yøµrš€lf..";
        mFinalBossText[6] = "Killing my ʞind";
        mFinalBossText[7] = "Yøu diD ɳot ʜëëd iTs Ⱳåř₦ïngs..";
        mFinalBossText[8] = "Yøu ₩ïLL rɇgr∊t τhªt VɆŘ¥ soon.";
    }
}

[Serializable]
public struct SpawnPoint //Enemies spawn at spawnpoint at move to one of the checkpoints.
{
    [field: SerializeField] public Transform spawnPoint { get; private set; }
    [field: SerializeField] public List<Transform> checkPoints { get; private set; }

    public Vector3 SpawnPointPosition()
    {
        return spawnPoint.position;
    }

    public Transform RandomCheckPoint()
    {
        return checkPoints[Random.Range(0, checkPoints.Count)];
    }
}
