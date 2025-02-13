using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class Turret : MonoBehaviour, IUpgradeable<Turret>
{
    //Turret stats
    [Header("Turrets Stats")]
    [field: SerializeField] public float damage { get; private set; }
    [field: SerializeField] public float bulletSpeed { get; private set; }
    [field: SerializeField] public float intervalBetweenShots { get; private set; }
    [field: SerializeField] public float shootingRange { get; private set; }
    [field: SerializeField] public Vector2 barrelElevationRange { get; private set; } // The lowest the barrel can point (x) and the highest the barrel can point (y).
    [field: SerializeField] public float horizontalRotationSpeed { get; private set; }
    [field: SerializeField] public float verticalRotationSpeed { get; private set; }
    [field: SerializeField] public int scrapCost { get; private set; }

    [Space(15)]
    [SerializeField] private float mAimingThreshold; // How far the turret can aimming be from target before it starts shooting.

    //Components
    [Header("Modules")]
    [SerializeField] Transform mTurretPivot;
    [SerializeField] Transform mElevationPivot;
    [SerializeField] Transform mMuzzle;
    [SerializeField] Transform mRecon; // Used to check if turret can see the enemies. Place at the hieght of the Muzzle, in the center of the turret.
    [SerializeField] Bullet mBullet;

    [Space(15)]
    [SerializeField] LayerMask mTargetsLayerMask;

    //Sound
    [Header("Sounds")]
    [SerializeField] AudioClip mFiringSound;
    private AudioSource mFiringAudioSource; //Assigned in Start()

    //Effects
    [Header("Effects")]
    [SerializeField] VisualEffect mMuzzleEffect;

    //Camera Shake
    [Header("Camera Shake")]
    [SerializeField] CameraShake mCameraShake;

    //Keeping track
    [Header("Debug")]
    [SerializeField] Enemy currentTarget;
    [SerializeField] float mLastShotTimeStamp = 0;
    [SerializeField] bool mIsAimingNearTarget = false;
    [SerializeField] bool mIsReconOnTarget = false;

    //Interval Update
    private int mUpdateInterval = 25;
    private int mFramesSinceLastUpdate = 0;

    private void Awake()
    {
        //Get Firing Audio Source
        mFiringAudioSource = mMuzzle.GetComponent<AudioSource>();
        if (mFiringAudioSource == null && mFiringSound != null)
        {
            mFiringAudioSource = mMuzzle.AddComponent<AudioSource>();
            mFiringAudioSource.playOnAwake = false;
            mFiringAudioSource.clip = mFiringSound;
            mFiringAudioSource.spatialBlend = 1f;
        }
        else
        {
            Log.Warning(this, "No firing audio provided.");
        }
    }

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Turret");

        //Check if elevaltion min is greater than max
        if (barrelElevationRange.x > barrelElevationRange.y)
        {
            Log.Info(this, $"[{name}, Turret] The barrel depression is greater than elevation! Swapping...");
            barrelElevationRange = new Vector2(barrelElevationRange.y, barrelElevationRange.x);
        }
    }

    private void IntervalUpdateTurret()
    {
        if(currentTarget != null)
        {
            mIsAimingNearTarget = IsAimingNearTarget(currentTarget);
        }

        Enemy tClosestEnemy = GetClosestTargetFromPoint(transform.position, EnemiesManager.instance.allEnemies);
        if (tClosestEnemy != null)
        {
            SetTarget(tClosestEnemy);
        }
    }

    private void FixedUpdate()
    {
        //Interval Update clock
        mFramesSinceLastUpdate++;
        if (mFramesSinceLastUpdate >= mUpdateInterval)
        {
            IntervalUpdateTurret();
            mFramesSinceLastUpdate = 0;
        }

        //Fixed Update Code below
        //Debug.DrawRay(mMuzzle.position, mMuzzle.forward * mShootingRange, Color.green, Time.deltaTime);
        if (currentTarget != null)
        {
            mIsReconOnTarget = IsReconOnTarget(currentTarget);
            if (mIsReconOnTarget == true)
            {
                AimForTarget(currentTarget);

                if (mIsAimingNearTarget == true && Time.time - mLastShotTimeStamp >= intervalBetweenShots)
                {
                    Shoot();
                }
            }
        }
    }

    private Enemy GetClosestTargetFromPoint(Vector3 aPoint, List<Enemy> aTargets)
    {
        if (aTargets.Count == 0)
        {
            currentTarget = null;
            return null;
        }

        Enemy tClosestTarget = null;
        float tClosestTargetDistance = shootingRange;

        foreach (Enemy tTarget in aTargets) 
        {
            float tCurrentTargetDistance = Vector3.Distance(tTarget.aimPosition, aPoint);
            if (tCurrentTargetDistance >= tClosestTargetDistance)
                continue;
            
            if (IsReconOnTarget(tTarget) == true)
            {
                tClosestTarget = tTarget;
                tClosestTargetDistance = tCurrentTargetDistance;
            }
        }
        
        return tClosestTarget;
    }

    private void SetTarget(Enemy aTarget)
    {
        if (aTarget == null)
        {
            currentTarget = null;
            return;
        }
        
        Health tTargetHealthComponent = aTarget.GetComponent<Health>();
        if (tTargetHealthComponent == null)
        {
            Log.Warning(this, $"The given target " + aTarget.name + " does not have a Health component. Aborting...");
            return;
        }

        if (tTargetHealthComponent.isFriendly == true)
        {
            Log.Warning(this, $"The given target " + aTarget.name + " to be assingned as the current target is friendly. Aborting...");
            return;
        }

        if (Vector3.Distance(aTarget.aimPosition, transform.position) > shootingRange)
        {
            Log.Warning(this, $"The target is too far! The target: {aTarget.name}");
            return;
        }

        if (!aTarget.GetComponent<DynamicHitboxController>())
        {
            Log.Warning(this, $"The target does not have a dynamic hitbox! The target: {aTarget.name}");
            return;
        }

        currentTarget = aTarget;
    }

    private Vector3 CalculateTargetAimPosition(Enemy aTarget)
    {
        Vector3 tTargetPosition = aTarget.aimPosition;
        
        Vector3 tMuzzlePosition = mMuzzle.position;

        float tDurationToTarget = CalculateBulletFlightDuration(tTargetPosition);
        Vector3 tProvisionalAimPosition = CalculateTargetFuturePosition(tDurationToTarget);
        //Debug.DrawLine(tTargetPosition, tProvisionalAimPosition, Color.magenta, Time.deltaTime);

        float tDurationToProvisonalAimPosition = CalculateBulletFlightDuration(tProvisionalAimPosition);
        Vector3 tAimPosition = CalculateTargetFuturePosition(tDurationToProvisonalAimPosition);
        tAimPosition.y += mBullet.CalculateBulletDrop(tDurationToProvisonalAimPosition);
        //Debug.DrawLine(tAimPosition, tTargetPosition, Color.yellow, Time.deltaTime);

        Vector3 CalculateTargetFuturePosition(float aDuration)
        {
            return tTargetPosition + (aTarget.velocity * aDuration);
        }

        float CalculateBulletFlightDuration(Vector3 aPosition)
        {
            return Vector3.Distance(tMuzzlePosition, aPosition) / bulletSpeed;
        }

        return tAimPosition;
    }

    private void AimForTarget(Enemy aTarget)
    {
        float tDeltaTime = Time.deltaTime;

        Vector3 tTargetAimPosition = CalculateTargetAimPosition(aTarget);
        Debug.DrawLine(mMuzzle.position, tTargetAimPosition, Color.red);

        //Turret Aiming
        Vector3 tTurretPivotTargetDirection = tTargetAimPosition - mTurretPivot.position;
        tTurretPivotTargetDirection.y = 0f;
        Quaternion tTurretPivotTargetRotation = Quaternion.LookRotation(tTurretPivotTargetDirection);
        mTurretPivot.localRotation = Quaternion.RotateTowards(mTurretPivot.localRotation, tTurretPivotTargetRotation, horizontalRotationSpeed * tDeltaTime);

        //Barrel Aiming
        Vector3 tBarrelTargetDirection = tTargetAimPosition - mElevationPivot.position;
        Quaternion tBarrelPivotTargetRotation = Quaternion.LookRotation(tBarrelTargetDirection);
        Quaternion tBarrelPivotLerpRotation = Quaternion.Slerp(mElevationPivot.rotation, tBarrelPivotTargetRotation, verticalRotationSpeed * tDeltaTime);

        //Local Euler Angles from global rotation, X
        float tBarrelLerpPitch = (Quaternion.Inverse(mElevationPivot.parent.rotation) * tBarrelPivotLerpRotation).eulerAngles.x;

        //Ensuring angle is between -180 and 180 before clamping between max and min elevation angles;
        tBarrelLerpPitch %= 360;
        if (tBarrelLerpPitch > 180)
        {
            tBarrelLerpPitch -= 360;
        }
        tBarrelLerpPitch = Mathf.Clamp(tBarrelLerpPitch, barrelElevationRange.x, barrelElevationRange.y);

        mElevationPivot.localEulerAngles = new Vector3(tBarrelLerpPitch, 0, 0);
    }

    private bool IsReconOnTarget(Enemy aTarget)
    {
        Vector3 tReconPosition = mRecon.position;

        RaycastHit tRayCastHit = new RaycastHit();
        //Debug.DrawLine(tReconPosition, aTarget.aimPosition, Color.yellow, 1f);
        if (Physics.Raycast(tReconPosition, aTarget.aimPosition - tReconPosition, out tRayCastHit, shootingRange, mTargetsLayerMask))
        {
            //Debug.DrawLine(tReconPosition, tRayCastHit.point, Color.blue, Time.deltaTime);

            //All checks for health components and such should be done in SetTarget()
            var tHitbox = tRayCastHit.transform.GetComponent<DynamicHitbox>();
            if (tHitbox != null && tHitbox.parent == aTarget.dynamicHitbox)
                return true;
        }
        
        return false;
    }

    private bool IsAimingNearTarget(Enemy aTarget)
    {  
        Vector3 tTargetAimPosition = CalculateTargetAimPosition(aTarget);
        Vector3 tMuzzlePosition = mMuzzle.position;
        float tDistanceToTargetAimPosition = DistancePointToLine(tTargetAimPosition, tMuzzlePosition, tMuzzlePosition + (mMuzzle.forward * shootingRange));

        return (tDistanceToTargetAimPosition <= mAimingThreshold);

        float DistancePointToLine(Vector3 aPoint, Vector3 aLineStart, Vector3 aLineEnd)
        {
            Vector3 tLineToPoint = aPoint - aLineStart;
            Vector3 tLineDirection = aLineEnd - aLineStart;
            float tLineLengthSquared = tLineDirection.sqrMagnitude;

            float t = Mathf.Clamp01(Vector3.Dot(tLineToPoint, tLineDirection) / tLineLengthSquared);
            Vector3 closestPoint = aLineStart + t * tLineDirection;

            return Vector3.Distance(aPoint, closestPoint);
        }

    }

    private void Shoot()
    {
        mLastShotTimeStamp = Time.time;

        //Creating a bullet
        ProjectileManager.Instance.GetFromPool(mBullet, true, mMuzzle.position, mMuzzle.forward, bulletSpeed, damage, false);

        //Playing firing sound
        if (mFiringAudioSource != null)
        {
            mFiringAudioSource.volume = Random.Range(0.8f, 1f);
            mFiringAudioSource.Play();
        }

        //Play effects
        mMuzzleEffect.Play();
        mCameraShake.AddShakeEvent(mMuzzle.position);
    }

    public float GetLastShotTimeStamp()
    {
        return mLastShotTimeStamp;
    }

    public bool IsReadToShoot()
    {
        return (mIsAimingNearTarget && mIsReconOnTarget);
    }

    //Does an incremental upgrade
    public void OnUpgrade(Turret aOther)
    {
        Transform tTransform = transform;
        Turret tUpgradedTurret = TurretsManager.instance.SpawnTurret(aOther.name.ToLowerInvariant(), tTransform.position, tTransform.rotation);

        //Move the current TurretUpgrade to the upgraded turret.
        TurretUpgrade tCurrentUpgradeComponent = GetComponent<TurretUpgrade>();
        TurretUpgrade tUpgradedTurretUpgradeComponent = tUpgradedTurret.GetComponent<TurretUpgrade>();
        if (tUpgradedTurretUpgradeComponent == null)
        {
            tUpgradedTurretUpgradeComponent = tUpgradedTurret.AddComponent<TurretUpgrade>();
        }
        else
        {
            Log.Info(this, $"There was already another {typeof(TurretUpgrade)} on the upgraded turret which will be reassigned with this gameobject's.");
        }
        // +1 because the level variable is not incremented for this component as its copied before the increment, so we do it here.
        tUpgradedTurretUpgradeComponent.Copy(tCurrentUpgradeComponent);
        tUpgradedTurretUpgradeComponent.IncrementLevel();

        TurretsManager.instance.DeleteTurret(this);
    }

    public KeyValuePair<string, string>[] GetNextUpgradeStats()
    {
        TurretUpgrade aUpgradeComponent = GetComponent<TurretUpgrade>();

        if (aUpgradeComponent == null)
        {
            Log.Error(this, "No upgradable component found!");
            return null;
        }

        if (aUpgradeComponent.IsMaxed() == true)
        {
            return GetCurrentStats();
        }

        Turret aOther = aUpgradeComponent.GetUpgradeData().upgrade;

        string tDamageStatValue = UpgradeDisplay<Turret>.UpgradeStatFormat(damage, aOther.damage, false);
        KeyValuePair<string, string> tDamageKeyValuePair = new KeyValuePair<string, string>("Damage", tDamageStatValue);

        string tFireRateStateValue = UpgradeDisplay<Turret>.UpgradeStatFormat(intervalBetweenShots, aOther.intervalBetweenShots, false);
        KeyValuePair<string, string> tFireRateKeyValuePair = new KeyValuePair<string, string>("Firerate", tFireRateStateValue);

        string tBulletSpeed = UpgradeDisplay<Turret>.UpgradeStatFormat(bulletSpeed, aOther.bulletSpeed, false);
        KeyValuePair<string, string> tBulletSpeedKeyValuePair = new KeyValuePair<string, string>("Bullet Speed", tBulletSpeed);

        string tShootingRange = UpgradeDisplay<Turret>.UpgradeStatFormat(shootingRange, aOther.shootingRange, false);
        KeyValuePair<string, string> tShootingRangeKeyValuePair = new KeyValuePair<string, string>("Range", tShootingRange);

        string tHorizontalSpeed = UpgradeDisplay<Turret>.UpgradeStatFormat(horizontalRotationSpeed, aOther.horizontalRotationSpeed, false);
        KeyValuePair<string, string> tHorizontalSpeedKeyValuePair = new KeyValuePair<string, string>("Turret Speed", tHorizontalSpeed);

        return new KeyValuePair<string, string>[] { tDamageKeyValuePair, tFireRateKeyValuePair, tBulletSpeedKeyValuePair, tShootingRangeKeyValuePair, tHorizontalSpeedKeyValuePair};
    }

    public KeyValuePair<string, string>[] GetCurrentStats(bool includeCost = false)
    {
        // is this less performant than a KVP array? probably
        // will the impact be measurable and even in the top 100 cpu consuming methods? if it is then i'll eat a shoe
        Dictionary<string, string> statsList = new Dictionary<string, string>();
        
        statsList.Add("Damage", damage.ToString());
        statsList.Add("Firerate", intervalBetweenShots.ToString());
        statsList.Add("Bullet Speed", bulletSpeed.ToString());
        statsList.Add("Range", shootingRange.ToString());
        statsList.Add("Turret Speed", horizontalRotationSpeed.ToString());

        if (includeCost)
            statsList.Add("Cost", scrapCost.ToString());

        return statsList.ToArray();
    }
}
