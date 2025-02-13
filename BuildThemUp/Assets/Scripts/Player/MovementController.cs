using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    // not serialized as playercontroller auto-sets this value
    [NonSerialized] public PlayerController playerController;
    
    [SerializeField] private Vector3 mPosition;
    [SerializeField] private Vector3 mRotation;
    [SerializeField] private CharacterController mCharController;
    [SerializeField] private Transform mViewTransform;

    // Player inputs
    private Vector3 mMoveAmount;

    private float mCameraAngleX;

    [Header("Movement Variables")]
    // TODO: We should move this somewhere else, like a world property
    [SerializeField] private float mGravityMult = 1f;
    // Movement speed variables
    [SerializeField] private float mGroundSharpness = 100f;
    [SerializeField] private float mAirAcceleration = 20f;
    [SerializeField] public float mMaxSpeed = 5f;
    
    // jump variables
    private bool mPendingJump = false;
    private float mLastJumpTime = 0;
    private float mJumpGroundingBuffer = 0.2f;
    [SerializeField] private float mJumpForce = 2.5f;
    
    // grounding variables
    private float mGroundCheckDistance = 0.05f;
    [SerializeField] private LayerMask mGroundLayers = -1;
    
    // Player's current movement velocity
    private Vector3 mVelocity;
    // Normal of ground the player is touching - used for when ascending slopes
    private Vector3 mGroundNormal;

    private bool mWasGrounded;
    private float mGravity;
    private Transform mTransform;
    private bool mEnabled = true;
    private bool mNoClipping = false;

    // Update is called once per frame
    void Start()
    {
        mGravity = mGravityMult * GamePlayManager.instance.gravity;
        mTransform = transform;
    }
    
    void Update()
    {
        if (mEnabled)
            HandleMovement();
    }

    public void SetState(bool aEnabled)
    {
        mEnabled = aEnabled;
    }

    public void ToggleNoClipping()
    {
        mNoClipping = !mNoClipping;
    }

    public void SetPosition(Vector3 aPosition, bool aUpdateVelocity = false, float aVelocityDivisor = 2f)
    {
        if (aUpdateVelocity)
            mVelocity = (aPosition - mTransform.position) / (Time.deltaTime * aVelocityDivisor);
        
        mTransform.position = aPosition;
    }

    void HandleMovement()
    {
        if (mNoClipping)
        {
            // store mMoveAmount variable for manipulation
            Vector3 tMoveAmount = mMoveAmount;

            // the hardcoded inputs (it's debug so...)
            if (Input.GetKey(KeyCode.Space))
                tMoveAmount.y = 1;
            else if (Input.GetKey(KeyCode.LeftControl))
                tMoveAmount.y = -1;
            else
            {
                // make camera angle actually influence movement
                var tCameraAngle = CameraController.Instance.transform.localEulerAngles.x;
                if (tCameraAngle > 180)
                    tCameraAngle -= 360;
                tMoveAmount.y = -tMoveAmount.z * tCameraAngle / 80f;
            }
            
            // the most absolute barest of bones movement code
            Vector3 tNoClipVelocity = transform.TransformVector(tMoveAmount.normalized) * mMaxSpeed;
            mVelocity = Vector3.Lerp(mVelocity, tNoClipVelocity, mGroundSharpness * Time.deltaTime);
            mTransform.position += mVelocity * Time.deltaTime;
            return;
        }
        
        bool tIsGrounded = CheckGrounded();

        Vector3 tWorldMoveInput = transform.TransformVector(mMoveAmount.normalized);

        if (tIsGrounded)
        {
            Vector3 tTargetVelocity = tWorldMoveInput * mMaxSpeed;
            tTargetVelocity = GetDirectionReorientedOnSlope(tTargetVelocity.normalized, mGroundNormal) * tTargetVelocity.magnitude;
            mVelocity = Vector3.Lerp(mVelocity, tTargetVelocity, mGroundSharpness * Time.deltaTime);
            
            if (mPendingJump)
            {
                // do jump stuff
                mVelocity = new Vector3(mVelocity.x, mJumpForce, mVelocity.z);
                mLastJumpTime = Time.time;
                mGroundNormal = Vector3.up;
                mPendingJump = false;
            }
        }
        else
        {
            if (mPendingJump)
            {
                mPendingJump = false;
            } 
            
            Vector3 tNewVelocity = mVelocity + tWorldMoveInput * (mAirAcceleration * Time.deltaTime);
            // check first that we aren't allowing the player to accelerate over maximum velocity
            // second check allows the player to decelerate even if already over max velocity
            if (tNewVelocity.magnitude < mMaxSpeed || tNewVelocity.magnitude < mVelocity.magnitude)
            {
                mVelocity = tNewVelocity;
            }

            mVelocity += Vector3.down * mGravity * Time.deltaTime;
            mVelocity.y = Mathf.Clamp(mVelocity.y, -3 * mMaxSpeed, 3 * mMaxSpeed); //Dunno why, just is. 
        }
        
        Vector3 tCapsuleBottom = GetCapsuleBottomHemisphere();
        Vector3 tCapsuleTop = GetCapsuleTopHemisphere();
        
        mCharController.Move(mVelocity * Time.deltaTime);
        playerController.loadoutController.OnMove(mVelocity, tIsGrounded);

        // slide down slopes
        if (Physics.CapsuleCast(tCapsuleBottom, tCapsuleTop, mCharController.radius, mVelocity.normalized,
                out var hit, mVelocity.magnitude * Time.deltaTime, -1, QueryTriggerInteraction.Ignore))
        {
            mVelocity = Vector3.ProjectOnPlane(mVelocity, hit.normal);
        }
        
        mWasGrounded = tIsGrounded;
    }

    bool CheckGrounded()
    {
        if (Time.time < mLastJumpTime + mJumpGroundingBuffer)
            return false;

        RaycastHit tHit;
        bool tFoundGround = Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(),
            mCharController.radius, Vector3.down, out tHit, mGroundCheckDistance, mGroundLayers);

        if (!tFoundGround)
            return false;

        mGroundNormal = tHit.normal;
        if (Vector3.Dot(mGroundNormal, transform.up) > 0 && IsNormalUnderSlopeLimit(mGroundNormal))
        {
            if (tHit.distance > 0.001f)
                mCharController.Move(Vector3.down * tHit.distance);

            return true;
        }

        return false;
    }

    
    #region Input Events
    public void OnLook(InputAction.CallbackContext context)
    {
        if (PlayerStates.isInMenu == true)
        {
            return;
        }

        // read the value for the "look" action each event call
        var tLookAmount = context.ReadValue<Vector2>();

        // TODO: Remove magic hardcoded value and replace with configurable mouse sens
        tLookAmount /= 15f;
        
        // look around
        transform.Rotate(new Vector3(0, tLookAmount.x, 0), Space.Self);
        
        // look up and down. make angle negative or else we get inverted controls
        mCameraAngleX = Mathf.Clamp(mCameraAngleX + tLookAmount.y, -80f, 80f);
        CameraController.Instance.SetAngleX(-mCameraAngleX);
        
        mViewTransform.localEulerAngles = new Vector3(-mCameraAngleX, 0, 0);
        
        playerController.loadoutController.OnLook(tLookAmount);
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        // read movement value
        var tMove = context.ReadValue<Vector2>();

        mMoveAmount = new Vector3(tMove.x, 0, tMove.y);
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        mPendingJump = true;
    }
    #endregion
    
    #region Movement Helper Functions
    private Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        var directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }
    
    private bool IsNormalUnderSlopeLimit(Vector3 normal)
    {
        return Vector3.Angle(transform.up, normal) <= mCharController.slopeLimit;
    }
    
    private Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + transform.up * mCharController.radius;
    }
    
    private Vector3 GetCapsuleTopHemisphere()
    {
        return transform.position + transform.up * (mCharController.height - mCharController.radius);
    }
    #endregion
}