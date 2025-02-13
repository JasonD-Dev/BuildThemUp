using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    //Components
    private NavMeshAgent mNavigationAgent;
    [SerializeField] private Transform mPlayerTransform;
    [SerializeField] protected Transform mMainBaseTransform { get; private set; }
    Transform[] mMainBaseTargets; 
    Collider mPlayerCollider;
    Collider mMainBaseCollider;
    Health mPlayerHealth;
    Health mMainBaseHealth;

    [Header("Enemy Stats")]
    [SerializeField] float mAttackRange = .005f;
    [SerializeField] float mAttackDamage = 2f;
    [SerializeField] float mAttackCooldown = 0.2f;
    [SerializeField] private EnemyTargetingPreference mTargetingPrefernce = EnemyTargetingPreference.Both;
    [SerializeField, Range(1, 50)] private int mTargetingPreferenceAmount = 25; //units

    [Header("Audio")]
    [SerializeField, Range (1, 50)] float mMaxAudioDistance = 30;
    [SerializeField] AudioClip[] mWalkingSounds;
    [SerializeField, Range(0.01f, 1f)] float mWalkingSoundVolume = 1;
    [SerializeField, Range(0, 3f)] float mWalkingSoundDelay = 0;
    AudioSource mAudioSource; //Assigned in start();

    // Animator
    EnemyAnimatorController mEnemyAnimatorController;

    //Keeping track
    private float mLastAttackTimeStamp = 0;
    private bool mHasReachedCheckPoint = false;
    private Transform mCheckPoint;
    private Transform mCurrentTarget;
    private float mLastWalkingSoundTimeStamp = 0; //The last time the enemy started walking.

    //LongInterval Update
    private int mLongUpdateInterval = 40;
    private int mSinceLastLongUpdate = 0;

    //Short Interval Update
    private int mShortUpdateInterval = 20;
    private int mSinceLastShortUpdate = 0;

    //public info
    public Vector3 aimPosition => mPosition + colliderCenterOffset;
    public Vector3 velocity => mNavigationAgent.velocity;

    private Transform mTransform;
    private Vector3 mPosition;

    public Vector3 colliderCenterOffset { get; private set; }

    [NonSerialized] public DynamicHitboxController dynamicHitbox;

    private enum EnemyTargetingPreference
    {
        Both,
        Player,
        Base
    }

    protected virtual void Start()
    {
        mTransform = transform;
        mNavigationAgent = GetComponent<NavMeshAgent>();
        
        mPlayerTransform = GamePlayManager.instance.playerTransform;
        mMainBaseTransform = GamePlayManager.instance.mainBaseTransform;
        mMainBaseTargets = EnemiesManager.instance.mainBaseTargets;

        mPlayerCollider = mPlayerTransform.GetComponent<Collider>();
        mMainBaseCollider = mMainBaseTransform.GetComponent<Collider>();
        
        // for now, box collider. update this when we switch to per-body-part colliders
        dynamicHitbox = GetComponent<DynamicHitboxController>();
        colliderCenterOffset = dynamicHitbox.GetHitboxCenter();

        mPlayerHealth = mPlayerTransform.GetComponent<Health>();
        mMainBaseHealth = mMainBaseTransform.GetComponent<Health>();

        if (mMainBaseCollider == null)
        {
            Log.Error(this, "Could not get Collider from Mainbase");
        }

        if (mPlayerCollider == null)
        {
            Log.Error(this, "Could not get Collider from Player.");
        }

        if (mPlayerHealth == null)
        {
            Log.Error(this, "Could not get Health from Player.");
        }

        if (mMainBaseHealth == null)
        {
            Log.Error(this, "Could not get Health from Mainbase.");
        }

        mNavigationAgent.stoppingDistance = mAttackRange * 0.9f;

        mEnemyAnimatorController = GetComponent<EnemyAnimatorController>();
        if (mEnemyAnimatorController == null)
        {
            Log.Error(this, "Enemy Animator is not initiated.");
        }

        mAudioSource = gameObject.AddComponent<AudioSource>();
        mAudioSource.maxDistance = mMaxAudioDistance;
        mAudioSource.playOnAwake = false;
        mAudioSource.spatialBlend = 1;
    }

    private void LongIntervalUpdate()
    {
        if (mCheckPoint != null && mHasReachedCheckPoint == false)
        {
            if (Vector3.Distance(mTransform.position, mCheckPoint.position) <= mAttackRange * 1.2f)
            {
                SetReachedCheckpoint();
            }
        }

        mCurrentTarget = GetClosestTarget();
        SetNavDestination(mCurrentTarget);
    }

    private void ShortIntervalUpdate()
    {
        bool tResetAnimator = false;

        if (mCurrentTarget == mPlayerTransform)
        {
            tResetAnimator = !tAttackIfCloseToCollider(mPlayerCollider, mPlayerHealth);
        }
        else if (mMainBaseTargets.Contains(mCurrentTarget) == true)
        {
            tResetAnimator = !tAttackIfCloseToCollider(mMainBaseCollider, mMainBaseHealth);    
        }
        else
        {   
            //Only reach this code if the current target is a checkpoint.
            tResetAnimator = true;
        }

        if (tResetAnimator == true) 
        {
            if (mEnemyAnimatorController.isActiveAndEnabled)
            {
                mEnemyAnimatorController.ResetAnimator();
            }
        }

        //Local function
        bool tAttackIfCloseToCollider(Collider aCollider, Health aHealth)
        {
            if (Vector3.Distance(transform.position, aCollider.ClosestPoint(transform.position)) <= mAttackRange)
            {
                if (Time.time - mLastAttackTimeStamp >= mAttackCooldown)
                {
                    Attack(aHealth);
                    Log.Info(this, "Attacking...");
                    return true;
                }
                
                else //Testing, Keep resetting despite Attacking
                {
                    return true;

                }
            }
            return false;
        }
    }

    protected virtual void FixedUpdate()
    {
        //Long Interval Update
        mSinceLastLongUpdate++;
        if (mSinceLastLongUpdate > mLongUpdateInterval)
        {
            LongIntervalUpdate();
            mSinceLastLongUpdate = 0;
        }

        //Short Interval Update
        mSinceLastShortUpdate++;
        if (mSinceLastShortUpdate > mShortUpdateInterval)
        {
            ShortIntervalUpdate();
            mSinceLastShortUpdate = 0;
        }

        mPosition = transform.position;

        //Play Walking audio
        if (mWalkingSounds.Length != 0 && mNavigationAgent.velocity.magnitude > 1f)
        {
            if (Time.time - mLastWalkingSoundTimeStamp >= mWalkingSoundDelay)
            {
                PlayWalkingSound();
            }

            void PlayWalkingSound()
            {
                mAudioSource.clip = mWalkingSounds[UnityEngine.Random.Range(0, mWalkingSounds.Length)];
                mAudioSource.volume = mWalkingSoundVolume;
                mAudioSource.Play();

                mLastWalkingSoundTimeStamp = Time.time;
            }
        }
    }

    protected virtual Transform GetClosestTarget()
    {
        float tDistanceToPlayer = Vector3.Distance(transform.position, mPlayerTransform.position);

        if (mCheckPoint != null && mHasReachedCheckPoint == false)
        {
            float tDistanceToCheckPoint = Vector3.Distance(mCheckPoint.position, mTransform.position);
            if (tDistanceToCheckPoint <= tDistanceToPlayer)
            {
                return mCheckPoint;
            }

            //Ignore checkpoint if contact is ever made with player. Code only reaches this is player is closer.
            SetReachedCheckpoint(); 
        }

        //Only run the code below this point once we know the we are not targeting the checkpoint.
        float tDistanceToMainBase = Vector3.Distance(transform.position, mMainBaseTransform.position);

        //We artifically increase the distance values of the unprefered target.
        if (mTargetingPrefernce == EnemyTargetingPreference.Base)
        {
            tDistanceToPlayer += mTargetingPreferenceAmount;
        }
        else if (mTargetingPrefernce == EnemyTargetingPreference.Player)
        {
            tDistanceToMainBase += mTargetingPreferenceAmount;
        };

        Transform tClosestTransform = (tDistanceToPlayer < tDistanceToMainBase) ? mPlayerTransform : mMainBaseTransform;
        if (tClosestTransform == mMainBaseTransform)
        {
            Transform tClosestTarget = mMainBaseTargets[0];

            float tClosestTargetDistance = tDistanceToMainBase;
            foreach (Transform tMainBaseTarget in mMainBaseTargets)
            {
                float tCurretDistance = Vector3.Distance(transform.position, tMainBaseTarget.position);
                if (tCurretDistance < tClosestTargetDistance)
                {
                    tClosestTargetDistance = tCurretDistance;
                    tClosestTarget = tMainBaseTarget;
                }
            }

            tClosestTransform = tClosestTarget;
        }

        return tClosestTransform;
    }

    private void SetNavDestination(Transform aTransform)
    {
        if (mNavigationAgent.destination != aTransform.position)
        {
            mNavigationAgent.destination = aTransform.position;
        }
    }

    protected virtual void Attack(Health tHealth)
    {
        if (mEnemyAnimatorController != null)
        {
            mEnemyAnimatorController.PlayAttackAnimation();
        }
        tHealth.TakeDamage(mAttackDamage, mTransform);
        mLastAttackTimeStamp = Time.time;
    }

    public void SetCheckPoint(Transform aTransform)
    {
        mCheckPoint = aTransform;
        mHasReachedCheckPoint = false;
    }

    void SetReachedCheckpoint()
    {
        mCheckPoint = null;
        mHasReachedCheckPoint = true;
    }

    public void OnDestroy()
    {
        GamePlayManager.instance.IncrementKillCount();
        EnemiesManager.instance.RemoveEnemy(this);
    }
}