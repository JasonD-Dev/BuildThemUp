using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Scrap : MonoBehaviour
{
    [SerializeField] int mScrapAmount = 10;
    private Transform mPlayerTransform; //Assigned in Start();

    //Stats Variables
    [Header("Stats")]
    [Range(0f, 12f)]
    public float mDistaceBeforeAttraction;
    [Range(1f, 4f)]
    public float mDistaceBeforeCollect;

    [Tooltip("Lifespan in seconds after which scrap is destroyed.")]
    [SerializeField] private float mLifeSpan = 10;

    //Bob and Rotating Variables
    [Header("Bob and Rotation")]
    [SerializeField] private Transform mBobObject;
    private Vector3 mBobObjectStartingPosition; //Assigned in Start();
    [SerializeField, Range(0.1f, 5f)] private float mBobSpeed;
    [SerializeField, Range(0.1f, 1f)] private float mBobHeight;
    [SerializeField, Range(1f, 400f)] private float mRotationSpeed;

    [Header("Sounds")]
    [SerializeField] AudioClip mCollectionSound;
    [SerializeField, Range(0f, 1f)] float mCollectionSoundVolume = 1;

    //Nav Agent
    private NavMeshAgent mNavAgent;

    //Interval Update
    private float mSpawnTimeStamp;
    private int mUpdateFrequency = 10;
    private int mFramesSinceLastUpdate = 0;

    void Awake()
    {
        mNavAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        mSpawnTimeStamp = Time.time;
        mBobObjectStartingPosition = mBobObject.localPosition;
        mPlayerTransform = GamePlayManager.instance.playerTransform;
    }

    void IntervalUpdate()
    {
        if (GamePlayManager.instance.IsInstantAddScrapToPlayer())
        {
            CollectScrap();
            Destroy(gameObject);
            return;
        }

        float tDistanceToPlayer = Vector3.Distance(mPlayerTransform.position, transform.position);

        if (tDistanceToPlayer <= mDistaceBeforeCollect)
        {
            CollectScrap();
            return;
        }

        if (tDistanceToPlayer <= mDistaceBeforeAttraction)
        {
            AttractTo(mPlayerTransform.position);
            return;
        }

        if (Time.time - mSpawnTimeStamp > mLifeSpan) 
        {
            Destroy(gameObject);
            return;
        }

        StopAttraction();
    }

    void FixedUpdate()
    {
        mFramesSinceLastUpdate++;
        if (mFramesSinceLastUpdate >= mUpdateFrequency)
        {
            IntervalUpdate();
            mFramesSinceLastUpdate = 0;
        }

        transform.Rotate(Vector3.up, mRotationSpeed * Time.deltaTime);

        float tYBobValue = mBobHeight * Mathf.Sin(Time.time * mBobSpeed);
        mBobObject.localPosition = mBobObjectStartingPosition + new Vector3(0, tYBobValue, 0);
    }

    public void CollectScrap()
    {
        GamePlayManager.instance.AddScrap(mScrapAmount);
        PlayerController.instance.PlayOneShot(mCollectionSound, mCollectionSoundVolume);
        Destroy(gameObject);
    }

    public void AttractTo(Vector3 aPlayerPosition)
    {
        mNavAgent.SetDestination(aPlayerPosition);
        mNavAgent.isStopped = false;
    }

    public void StopAttraction()
    {
        mNavAgent.isStopped = true;
    }
}
