using UnityEngine;

public class ShootingStarSpawner : MonoBehaviour
{
    [SerializeField] ShootingStarBehaviour mShootingStarPrefab;
    [SerializeField, Range(0f, 50f)] float mMinInterval = 5.0f;
    [SerializeField, Range(0f, 50f)] float mMaxInterval = 15.0f;

    private float mNextStarTime;

    void Start()
    {
        ScheduleNextStar();
    }

    void FixedUpdate()
    {
        if (Time.time >= mNextStarTime)
        {
            SpawnShootingStar();
            ScheduleNextStar();
        }
    }

    void ScheduleNextStar()
    {
        mNextStarTime = Time.time + Random.Range(mMinInterval, mMaxInterval);
    }

    void SpawnShootingStar()
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(-200, 200),  // Random X position
            Random.Range(100, 150),   // Random Y position (higher in the sky)
            Random.Range(-200, 200)   // Random Z position
        );

        Instantiate(mShootingStarPrefab, randomPosition, Quaternion.identity);
    }
}
