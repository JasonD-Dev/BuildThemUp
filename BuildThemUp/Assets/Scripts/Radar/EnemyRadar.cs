using System.Collections.Generic;
using UnityEngine;

public class EnemyRadar : MonoBehaviour
{
    [SerializeField] Canvas mRadarCanvas;

    [Header("Radar Settings")]
    [SerializeField, Range(0f, 10)] float mRadarDelay = 2;
    [SerializeField, Range(0f, 100)] float mRadarRange = 100;

    [Header("Recon Settings")]
    [SerializeField] ImageFillAnimation mReconIndicator;
    [SerializeField, Range(0f, 1)] float mReconIndicatorDisplayTime = 0.4f;

    public float radarDelay => mRadarDelay;
    public float radarRange => mRadarRange;

    List<Enemy> mAllEnemies => EnemiesManager.instance.allEnemies;

    //Keeping track.
    float mLastReconTimeStamp = 0;

    private void Start()
    {
        if (mRadarCanvas.gameObject.activeSelf == false)
        {
            mRadarCanvas.gameObject.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        if (Time.time - mLastReconTimeStamp > mRadarDelay)
        {
            DisplayReconIndicators(GetReconPositions());
            mLastReconTimeStamp = Time.time;
        }
    }

    List<Vector3> GetReconPositions()
    {
        List<Vector3> tPositions = new List<Vector3>();
        for (int i = 0; i < mAllEnemies.Count; i++)
        {
            Vector3 tCurrentTargetPosition = mAllEnemies[i].aimPosition;
            if (Vector3.Distance(transform.position, tCurrentTargetPosition) <= mRadarRange)
            {
                tPositions.Add(tCurrentTargetPosition);
            }
        }

        Log.Info(this, $"Found {tPositions.Count} enemies.");
        return tPositions;
    }

    void DisplayReconIndicators(List<Vector3> aPositions)
    {
        for (int i = 0; i < aPositions.Count; i++)
        {
            ImageFillAnimation tIndicator = Instantiate(mReconIndicator, aPositions[i], Quaternion.identity, mRadarCanvas.transform);
            Destroy(tIndicator.gameObject, tIndicator.fillTime + mReconIndicatorDisplayTime);
        }

        Log.Info(this, $"Displaying {aPositions.Count} radar indicators.");
    }

    public void SetRange(float aRange)
    {
        if (aRange < 0)
        {
            Log.Error(this, "New Range cannot be below 0.");
            return;
        }

        mRadarRange = aRange;
    }

    public void SetDelay(float aDelay)
    {
        if (aDelay < 0)
        {
            Log.Error(this, "New delay cannot be below 0.");
            return;
        }

        mRadarDelay = aDelay;
    }
}
