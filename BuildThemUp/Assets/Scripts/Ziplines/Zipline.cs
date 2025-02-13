using UnityEngine;

public class Zipline : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] LineRenderer mLineRenderer;
    [SerializeField] Transform mAnchorOne;
    [SerializeField] Transform mAnchorTwo;

    [Header("Parameters")]
    [SerializeField] float mZiplineSpeed = 2;
    [SerializeField] float mDetectionDistanceToPole = 5;

    [Header("Visuals")]
    [SerializeField, Range(0f, -10f)] float mIdleRopeSag = -2;
    [SerializeField, Range(0f, -5f)] float mAdditivePlayerRopeSag = -2;
    [SerializeField, Range(0f, -5f)] float mDistanceToRope = -2;

    [Header("Sound")]
    [SerializeField] AudioClip mZiplineSound;
    [SerializeField, Range(0f, 1f)] float mZiplineSoundVolume = 1;
    AudioSource mZiplineAudioSource;

    [Header("Testing")]
    [SerializeField] bool mAlternativeZiplining = false;

    Transform mPlayerTransform; //Assigned in Start();
 
    Transform mStartingAnchor; //Null is player isn't close enough.
    Transform mEndingAnchor; //Null is player isn't close enough.
    public bool isPlayerClose { get; private set; } = false;
    bool mIsOnZipLine = false;

    float mZiplineDistance; //Assigned in Start();
    float mZiplineDuration; //Assigned in Start();
    float mTimeSinceAttachment = 0f;
    float mLineVerticesDistance = 2;
    float mDistanceThreshold = 0.05f; //How far does the player have to be from the end before it counts as finished.

    private byte mPromptId;

    void Start()
    {
        mPlayerTransform = GamePlayManager.instance.playerTransform;

        mZiplineDistance = Vector3.Distance(mAnchorOne.position, mAnchorTwo.position);
        if (mZiplineDistance <= mDetectionDistanceToPole)
        {
            Log.Error(this, $"Ziplines cannot be placed too close! The ziplines must atleast be a detection distance ({mDetectionDistanceToPole}) apart! Distance was {mZiplineDistance}. Destroying...");
            Destroy(this);
        }

        if (mZiplineSpeed <= 0)
        {
            Log.Info(this, "Zipline speed was zero or below. Setting it to 1.");
            mZiplineSpeed = 1;
        }

        mZiplineDuration = mZiplineDistance / mZiplineSpeed;

        mLineRenderer.positionCount = (int)Mathf.Ceil(mZiplineDistance / mLineVerticesDistance) + 1;
        ResetRope();

        //Sound
        if (mZiplineSound != null)
        {
            mZiplineAudioSource = gameObject.AddComponent<AudioSource>();
            mZiplineAudioSource.playOnAwake = false;
            mZiplineAudioSource.volume = mZiplineSoundVolume;
            mZiplineAudioSource.clip = mZiplineSound;
            mZiplineAudioSource.loop = true;
        }
    }

    private void Update()
    {
        if (mIsOnZipLine == true)
        {
            mTimeSinceAttachment += Time.deltaTime;
            float t = mTimeSinceAttachment / mZiplineDuration;

            if (t >= 1 - mDistanceThreshold)
            {
                DetachFromZipline();
            }
            else
            {
                Vector3 tStartingPosition = mStartingAnchor.position;
                Vector3 tEndingPosition = mEndingAnchor.position;

                if (mAlternativeZiplining == true)
                {
                    //The alternative, it has rope sagging with weight but has ease off with speed at the end. 
                    SetPlayerPosition(t, tStartingPosition, tEndingPosition, mIdleRopeSag + mAdditivePlayerRopeSag, t);
                    SetRope(tStartingPosition, tEndingPosition, mIdleRopeSag + mAdditivePlayerRopeSag, t);
                }
                else
                {
                    SetPlayerPosition(t, tStartingPosition, tEndingPosition, mIdleRopeSag);
                    SetRope(tStartingPosition, tEndingPosition, mIdleRopeSag);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (mIsOnZipLine == false)
        {
            DetermineStartAndEndPoles();
        }
    }

    void AttachToZipLine()
    {
        // if attached to zipline, prompt will be visible, so hide it
        PromptManager.instance.RemovePrompt(mPromptId);
        // show detach prompt
        mPromptId = PromptManager.instance.ShowPrompt("[Space] Detach Zipline", DetachFromZipline, UIBinds.detachFromZiplineKey);
        
        mTimeSinceAttachment = 0;

        SetPlayerPosition(mDistanceThreshold, mStartingAnchor.position, mEndingAnchor.position, mIdleRopeSag);

        //PlayerStates.isOnZipline = true; //A static reference for other classes to use.
        PlayerController.instance.movementController.SetState(false);
        isPlayerClose = false; // set this to false so that prompt still appears when we detach (forces change of state)
        mIsOnZipLine = true;

        if (mZiplineAudioSource != null)
        {
            mZiplineAudioSource.Play();
        }
    }

    void DetachFromZipline()
    {
        // if detached from zipline, hide detach prompt
        PromptManager.instance.RemovePrompt(mPromptId);
        
        mTimeSinceAttachment = float.MaxValue;

        //PlayerStates.isOnZipline = false;
        PlayerController.instance.movementController.SetState(true);
        mIsOnZipLine = false;

        ResetRope();

        if (mZiplineAudioSource != null)
        {
            mZiplineAudioSource.Stop();
        }
    }

    void DetermineStartAndEndPoles()
    {
        float tDistanceToAnchorOne = Vector3.Distance(mAnchorOne.position, mPlayerTransform.position);
        float tDistanceToAnchorTwo = Vector3.Distance(mAnchorTwo.position, mPlayerTransform.position);

        Transform tClosestAnchor = null;
        Transform tFurthestAnchor = null;
        if (tDistanceToAnchorOne <= mDetectionDistanceToPole)
        {
            tClosestAnchor = mAnchorOne;
            tFurthestAnchor = mAnchorTwo;
        }
        else if (tDistanceToAnchorTwo <= mDetectionDistanceToPole)
        {
            tClosestAnchor = mAnchorTwo;
            tFurthestAnchor = mAnchorOne;
        }

        bool tWasPlayerClose = isPlayerClose;
        isPlayerClose = (tClosestAnchor != null);
        
        if (!tWasPlayerClose && isPlayerClose)
            // entered attach radius, show prompt
            mPromptId = PromptManager.instance.ShowPrompt("[E] Zipline", AttachToZipLine, UIBinds.attachToZiplineKey);
        else if (tWasPlayerClose && !isPlayerClose)
            // left attach radius, so hide prompt
            PromptManager.instance.RemovePrompt(mPromptId);
        
        mStartingAnchor = tClosestAnchor;
        mEndingAnchor = tFurthestAnchor;
    }

    void SetPlayerPosition(float aT, Vector3 aStartPosition, Vector3 aEndPosition, float aHeight, float aControlPointBias = 0.5f)
    {
        Vector3 tPlayerPosition = CalculateBezierPoint(aT, aStartPosition, aEndPosition, aHeight, aControlPointBias);
        tPlayerPosition.y += mDistanceToRope;

        PlayerController.instance.movementController.SetPosition(tPlayerPosition, true);
        //mPlayerTransform.position = tPlayerPosition;
    }

    void SetRope(Vector3 aStartPosition, Vector3 aEndPosition, float aHeight, float aControlPointBias = 0.5f)
    {
        for (int i = 0; i < mLineRenderer.positionCount; i++)
        {
            float t = i / (float)(mLineRenderer.positionCount - 1);
            Vector3 tPosition = CalculateBezierPoint(t, aStartPosition, aEndPosition, aHeight, aControlPointBias);
            mLineRenderer.SetPosition(i, tPosition);
        }
    }

    void ResetRope()
    {
        SetRope(mAnchorOne.position, mAnchorTwo.position, mIdleRopeSag, 0.5f);
    }

    Vector3 CalculateBezierPoint(float aT, Vector3 aStartPosition, Vector3 aEndPosition, float aHeight, float aControlPointBias = 0.5f)
    {
        aT = Mathf.Clamp01(aT);
        aControlPointBias = Mathf.Clamp01(aControlPointBias);
        Vector3 tControlPoint = Vector3.Lerp(aStartPosition, aEndPosition, aControlPointBias) + (Vector3.up * aHeight);

        // Quadratic Bezier formula
        Vector3 tPoint = (Mathf.Pow(1 - aT, 2) * aStartPosition) 
                        + (2 * (1 - aT) * aT * tControlPoint) 
                        + (Mathf.Pow(aT, 2) * aEndPosition);

        return tPoint;
    }
}
