using System;
using PrimeTween;
using UnityEngine;

public class LoadoutAnimator : MonoBehaviour
{
    public Transform weaponParentTransform;
    
    [Range(-10f, 10f)]
    public float lookSwayScale = -5f;
    [Range(-0.5f, 0.5f)]
    public float moveRecoilPosXScale = -0.025f;
    [Range(-0.5f, 0.5f)]
    public float moveRecoilPosYScale = -0.1f;
    [Range(-0.5f, 0.5f)]
    public float moveRecoilPosZScale = -0.02f;
    [Range(-50, 50)]
    public float moveRecoilRotXScale = 30;
    [Range(-30, 30)]
    public float moveRecoilRotYScale = 0;
    [Range(-30, 30)]
    public float moveRecoilRotZScale = -5f;

    [Header("Rotation Clamp Params")]
    [SerializeField, Range(0, 90)] float mMaxRotationX;
    [SerializeField, Range(0, -90)] float mMinRotationX;

    [Header("Position Clamp Params")]
    [SerializeField, Range(0, 1f)] float mMaxPositionY;
    [SerializeField, Range(0, -1f)] float mMinPositionY;

    [SerializeField] private Animation mAnimation;
    public bool equipping => mAnimation.isPlaying;

    private float mPositionRecoilRecoveryFactor;
    private float mRotationRecoilRecoveryFactor;
    private Vector3 mPositionRecoil;
    private Vector3 mRotationRecoil;
    
    private Transform mTransform;
    public void Awake() => mTransform = transform;

    public void Update()
    {
        // Apply both rotational and positional recoil
        mRotationRecoil = Vector3.Lerp(mRotationRecoil, Vector3.zero, 10f * mRotationRecoilRecoveryFactor * Time.deltaTime);
        mPositionRecoil = Vector3.Lerp(mPositionRecoil, Vector3.zero, 10f * mPositionRecoilRecoveryFactor * Time.deltaTime);

        mTransform.localEulerAngles = mRotationRecoil;
        mTransform.localPosition = mPositionRecoil;
    }

    public void ApplyRecoil(Vector3 aPositionForce, Vector3 aRotationForce)
    {
        // do recoil animation stuff
        mPositionRecoil += aPositionForce;
        mRotationRecoil += aRotationForce;

        mRotationRecoil.x = Mathf.Clamp(mRotationRecoil.x, mMinRotationX, mMaxRotationX);
        mPositionRecoil.y = Mathf.Clamp(mPositionRecoil.y, mMinPositionY, mMaxPositionY);
    }

    public void SetRecoilRecovery(float aPositionFactor, float aRotationFactor)
    {
        mPositionRecoilRecoveryFactor = aPositionFactor;
        mRotationRecoilRecoveryFactor = aRotationFactor;
    }

    public void OnEquip()
    {
        mAnimation.Stop();
        mAnimation.Play(PlayMode.StopAll);
    }
    
    public void OnLook(Vector2 aLookAmount)
    {
        var tScaledAmount = aLookAmount * (Time.deltaTime * lookSwayScale);
        ApplyRecoil(Vector3.zero, new Vector3(tScaledAmount.y, -tScaledAmount.x, 0));
    }
    
    public void OnMove(Vector3 aVelocity, bool aIsGrounded)
    {
        Vector3 tLocalVelocity = mTransform.InverseTransformDirection(aVelocity);
        Vector3 tTimeScaled = tLocalVelocity * Time.deltaTime;

        Vector3 tScaledAmount = new Vector3(
            tTimeScaled.x * moveRecoilPosXScale,
            aIsGrounded ? 0 : tTimeScaled.y * moveRecoilPosYScale, // we don't want weapons moving going up/down slopes
            tTimeScaled.z * moveRecoilPosZScale
        );

        Vector3 tRotationAmount = new Vector3(
            aIsGrounded ? 0 : tTimeScaled.y * moveRecoilRotXScale, // we don't want weapons moving going up/down slopes
            tTimeScaled.z * moveRecoilRotYScale,
            tTimeScaled.x * moveRecoilRotZScale
        );
        
        ApplyRecoil(tScaledAmount, tRotationAmount);
    }
}