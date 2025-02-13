using System;
using UnityEngine;

[RequireComponent(typeof(Turret))]
public class TurretAnimator : MonoBehaviour
{
    //Components
    [Header("Components")]
    [SerializeField] Turret mTurretToAnimate;
    // #Note to future self. change names below to have "Group" at the end when upgrades are implemented.
    [SerializeField] Transform mRecoilBarrel;
    [SerializeField] Transform mRecoilBody;
    [SerializeField] Transform mRotationBarrel;

    //Recoil stats
    [Header("Recoil Stats")]
    [SerializeField] private float mRecoilIntensity;
    [Tooltip("Duration proportional to the time between shots.")]
    [SerializeField, Range(0.1f, 0.9f)] private float mRecoilProportionalDuration;
    [SerializeField, Range(0f, 1f)] private float mBodyRecoilMultiplier;
    [Tooltip("What proportion of the total recoil should be taken up by the retracting motion.")]
    [SerializeField, Range(0.1f, 0.9f)] private float mRecoilRetractProportion;

    //Turret barrel rotation stats
    [Header("Turret Barrel Rotation Stats")]
    [SerializeField] private float mShootingBarrelRotationSpeed;
    [SerializeField] private float mIdleBarrelRotationSpeed;

    //Keeping track of recoil, Assigned in Initialise();
    private float recoilDuration;
    private Vector3 mRecoilBarrelStartPosition;
    private Vector3 mRecoilBarrelEndPosition;
    private Vector3 mRecoilBodyStartPosition;
    private Vector3 mRecoilBodyEndPosition;

    //Keeping track of rotation
    private float mCurrentBarrelRotationSpeed = 0; //Assigned in Start();

    private void Start()
    {
        //Pre calculating recoil amounts instead of doing it every call.
        recoilDuration = mTurretToAnimate.intervalBetweenShots * mRecoilProportionalDuration;
        float tRecoilAmount = (recoilDuration * mRecoilRetractProportion) * mRecoilIntensity;

        mRecoilBarrelStartPosition = mRecoilBarrel.localPosition;
        mRecoilBarrelEndPosition = mRecoilBarrelStartPosition - (mRecoilBarrel.forward * tRecoilAmount);

        mRecoilBodyStartPosition = mRecoilBody.localPosition;
        mRecoilBodyEndPosition = mRecoilBodyStartPosition - (mRecoilBarrel.forward * (tRecoilAmount * mBodyRecoilMultiplier));

        mCurrentBarrelRotationSpeed = mIdleBarrelRotationSpeed;
    }

    private void FixedUpdate()
    {
        float tLastShotTimeStamp = mTurretToAnimate.GetLastShotTimeStamp();
        //Doing Recoil
        if (Time.time - tLastShotTimeStamp <= recoilDuration * 1.1f)
        {
            DoRecoil(tLastShotTimeStamp);
        }

        //Barrel Rotation
        if (mRotationBarrel != null)
        {
            bool isReadyToShoot = mTurretToAnimate.IsReadToShoot();
            RotateBarrel(isReadyToShoot);
        }
    }

    public void DoRecoil(float aLastShotTimeStamp)
    {
        float tNormalisedTimeSinceLastShot = (Time.time - aLastShotTimeStamp) / recoilDuration;
        if (tNormalisedTimeSinceLastShot <= mRecoilRetractProportion)
        {
            float tCurrentLerpTime = tNormalisedTimeSinceLastShot / mRecoilRetractProportion; //Still retracting.
            mRecoilBarrel.localPosition = Vector3.Lerp(mRecoilBarrelStartPosition, mRecoilBarrelEndPosition, tCurrentLerpTime);
            mRecoilBody.localPosition = Vector3.Lerp(mRecoilBodyStartPosition, mRecoilBodyEndPosition, tCurrentLerpTime);
        }
        else
        {
            float tCurrentLerpTime = tNormalisedTimeSinceLastShot / (1 - mRecoilRetractProportion); //Moving back to starting position
            mRecoilBarrel.localPosition = Vector3.Lerp(mRecoilBarrelEndPosition, mRecoilBarrelStartPosition, tCurrentLerpTime);
            mRecoilBody.localPosition = Vector3.Lerp(mRecoilBodyEndPosition, mRecoilBodyStartPosition, tCurrentLerpTime);
        }
    }

    public void RotateBarrel(bool aIsReadyToShoot)
    {
        //Do Spining
        float tRotationSpeedStep = (mShootingBarrelRotationSpeed - mIdleBarrelRotationSpeed) * Time.deltaTime;
        if (aIsReadyToShoot == true && mCurrentBarrelRotationSpeed < mShootingBarrelRotationSpeed)
        {
            mCurrentBarrelRotationSpeed += tRotationSpeedStep;
        }
        else if (aIsReadyToShoot == false && mCurrentBarrelRotationSpeed > mIdleBarrelRotationSpeed)
        {
            mCurrentBarrelRotationSpeed -= tRotationSpeedStep;
        }

        mRotationBarrel.Rotate(Vector3.forward * mCurrentBarrelRotationSpeed * Time.deltaTime);
    }
}
