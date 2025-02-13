using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Weapon : Item, IUpgradeable<WeaponIncrementUpgrade>
{
    [Header("Damage Stats")]
    public int damage;
    public float fireRate = 1;
    public float bulletSpeed = 100;

    [Header("Reload Stats")]
    [SerializeField] KeyCode mRealodButton = KeyCode.R;
    [field: SerializeField] public float heatPerShot { get; private set; } = 0;
    [field: SerializeField] public float heatCapacity { get; private set; } = 100;
    [field: SerializeField] public float coolingSpeed { get; private set; } = 1;
    [field: SerializeField] public AudioClip coolingAudio { get; private set; }
    [field: SerializeField] public bool isCooling { get; private set; } = false;
    private float mCurrentHeat = 0;
    private static float mAutoCoolingIdleTime = 1;
    public float heatRatio => mCurrentHeat / heatCapacity;


    [Header("Recoil Values")]
    [Tooltip("Rotation value between min and max to apply to rotation as recoil")]
    public RecoilInfo recoilRotation;
    public float recoilRotationRecoveryFactor = 1f;

    [Tooltip("How far the weapon is pushed back by recoil")]
    public RecoilInfo recoilPosition;
    public float recoilPositionRecoveryFactor = 1f;

    [Header("Camera Shake Values")]
    [SerializeField] CameraShake mCameraShake = new CameraShake();

    [Header("Components")]
    [SerializeField] protected Transform mMuzzleTransform;
    [SerializeField] private AudioClip[] mFireSounds;
    [SerializeField, Range(0.1f, 1)] private float mFireSoundVolume = 1;

    //Keeping Track
    protected float mFireTimer = 0;
    protected float mLastShotTimeStamp = 0;

    protected static float mDamageVariance = 0.1f;

    public AimDownSights aimDownSights { get; private set; } //assigned in awake();

    protected override void Awake()
    {
        base.Awake();
        aimDownSights = GetComponent<AimDownSights>();
    }

    protected override void Update()
    {
        //Shooting
        if (mFireTimer > 0)
        {
            mFireTimer -= Time.deltaTime;
        }
        else
        {
            mFireTimer = 0;
        }
        
        if (LoadoutController.UseHeld)
        {
            if (mFireTimer <= 0)
            {
                Shoot();
            }
        }

        //Gun Cooling
        if (heatCapacity > 0)
        {
            if (LoadoutController.equippedWeapon == this)
            {
                if ((Input.GetKeyDown(mRealodButton) == true || mCurrentHeat >= heatCapacity) && isCooling == false)
                {
                    InitialiseCooling();
                }
            }

            if (isCooling == true || Time.time - mLastShotTimeStamp > mAutoCoolingIdleTime)
            {
                CoolWeapon();
            }
        }
    }

    protected Tuple<Vector3, Vector3> GetOriginDirection(float aXOffset = 0, float aYOffset = 0)
    {
        Transform tCameraTransform = CameraController.Instance.transform;
        Vector3 tCamDirection = tCameraTransform.forward;
        tCamDirection += tCameraTransform.right * aXOffset;
        tCamDirection += tCameraTransform.up * aYOffset;
        
        Vector3 tMuzzlePosition = mMuzzleTransform.position;
        Ray tShootRay = new Ray(tCameraTransform.position, tCamDirection);
        Vector3 tDirection;
        if (Physics.Raycast(tShootRay, out var hit, 1000f, LoadoutController.raycastMask))
        {
            tDirection = Vector3.Normalize(hit.point - tMuzzlePosition);
        }  
        else
        {
            tDirection = tCamDirection;
        }

        return new Tuple<Vector3, Vector3>(tMuzzlePosition, tDirection);
    }

    public void InitialiseCooling()
    {
        if (isCooling == true)
        {
            Log.Info(this, $"The weapon is already realoding. {100 - (mCurrentHeat / heatCapacity * 100)}% done.");
            return;
        }

        if (mCurrentHeat <= 0)
        {
            Log.Info(this, $"The weapon is already cold.");
            return;
        }

        if (coolingAudio != null)
        {
            LoadoutController.PlayAudioClip(coolingAudio, 1f, 0.1f);
        }

        isCooling = true;
    }

    public virtual void CoolWeapon() //Called frame by frame to gradually cool.
    {
        mCurrentHeat -= coolingSpeed * Time.deltaTime;

        if (mCurrentHeat < 0)
        {
            mCurrentHeat = 0;
            isCooling = false;

            if (LoadoutController.IsAudioClipPlaying() == true)
            {
                LoadoutController.StopAudioClip();
            }
        }
    }


    public virtual bool Shoot()
    {
        if (PlayerStates.isInMenu == true)
        {
            return false;
        }

        if (isCooling == true)
        {
            return false;
        }

        mFireTimer = 1f / fireRate;
        mCurrentHeat += heatPerShot;

        int tIndex = Random.Range(0, mFireSounds.Length);
        float tPitch = Random.Range(0.95f, 1.05f);
        float tVolume = Random.Range(0.95f * mFireSoundVolume, 1.05f * mFireSoundVolume);
        LoadoutController.PlayAudioClip(mFireSounds[tIndex], tPitch, tVolume);
        LoadoutController.ApplyRecoil(recoilPosition.GetRandomised(), recoilRotation.GetRandomised());

        mCameraShake.AddShakeEvent(mMuzzleTransform.position);
        mLastShotTimeStamp = Time.time;
        return true;
    }

    public override void OnEquip()
    {
        LoadoutController.animator.SetRecoilRecovery(recoilPositionRecoveryFactor, recoilRotationRecoveryFactor);
        Crosshair.instance.Animate();
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        if (LoadoutController.IsAudioClipPlaying() == true)
        {
            LoadoutController.StopAudioClip();
        }
    }

    //Incremental Upgrade
    public void OnUpgrade(WeaponIncrementUpgrade aOther)
    {
        damage += aOther.damage;
        fireRate += aOther.fireRate;
        bulletSpeed += aOther.bulletSpeed;

        heatCapacity += aOther.heatCapacity;
        coolingSpeed += aOther.coolingSpeed;
    }

    public KeyValuePair<string, string>[] GetNextUpgradeStats()
    {
        WeaponUpgrade aUpgradeComponent = GetComponent<WeaponUpgrade>();

        if (aUpgradeComponent == null)
        {
            Log.Info(this, "No upgradable component found!");
            return new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Level", "Max") };
        }

        if (aUpgradeComponent.IsMaxed() == false)
        {
            return GetCurrentStats();
        }

        WeaponIncrementUpgrade aOther = aUpgradeComponent.GetUpgradeData().upgrade;

        string tDamageStatValue = UpgradeDisplay<WeaponIncrementUpgrade>.UpgradeStatFormat(damage, aOther.damage);
        KeyValuePair<string, string> tDamageKeyValuePair = new KeyValuePair<string, string>("Damage", tDamageStatValue);

        string tFireRateStatValue = UpgradeDisplay<WeaponIncrementUpgrade>.UpgradeStatFormat(fireRate, aOther.fireRate);
        KeyValuePair<string, string> tFireRateKeyValuePair = new KeyValuePair<string, string>("Firerate", tFireRateStatValue);

        string tBulletSpeedStatValue = UpgradeDisplay<WeaponIncrementUpgrade>.UpgradeStatFormat(bulletSpeed, aOther.bulletSpeed);
        KeyValuePair<string, string> tBulletSpeedKeyValuePair = new KeyValuePair<string, string>("Bullet Speed", tBulletSpeedStatValue);

        string tHeatCapacityStatValue = UpgradeDisplay<WeaponIncrementUpgrade>.UpgradeStatFormat(heatCapacity, aOther.heatCapacity);
        KeyValuePair<string, string> tHeatCapacityKeyValuePair = new KeyValuePair<string, string>("Heat Capacity", tHeatCapacityStatValue);

        string tCoolingSpeedStatValue = UpgradeDisplay<WeaponIncrementUpgrade>.UpgradeStatFormat(coolingSpeed, aOther.coolingSpeed);
        KeyValuePair<string, string> tCoolingSpeedKeyValuePair = new KeyValuePair<string, string>("Cooling Speed", tCoolingSpeedStatValue);


        return new KeyValuePair<string, string>[] { tDamageKeyValuePair, tFireRateKeyValuePair, tBulletSpeedKeyValuePair, tCoolingSpeedKeyValuePair, tHeatCapacityKeyValuePair };
    }

    public KeyValuePair<string, string>[] GetCurrentStats(bool includeCost = false)
    {
        KeyValuePair<string, string> tDamageKeyValuePair = new KeyValuePair<string, string>("Damage", damage.ToString());
        KeyValuePair<string, string> tFireRateKeyValuePair = new KeyValuePair<string, string>("Firerate", fireRate.ToString());
        KeyValuePair<string, string> tBulletSpeedKeyValuePair = new KeyValuePair<string, string>("Bullet Speed", bulletSpeed.ToString());
        KeyValuePair<string, string> tHeatCapacityKeyValuePair = new KeyValuePair<string, string>("Heat Capacity", heatCapacity.ToString());
        KeyValuePair<string, string> tCoolingSpeedKeyValuePair = new KeyValuePair<string, string>("Cooling Speed", coolingSpeed.ToString());

        return new KeyValuePair<string, string>[] { tDamageKeyValuePair, tFireRateKeyValuePair, tBulletSpeedKeyValuePair, tCoolingSpeedKeyValuePair, tHeatCapacityKeyValuePair };
    }
}

[Serializable]
public struct RecoilInfo
{
    public Vector3 min;
    public Vector3 max;

    public Vector3 GetRandomised()
    {
        return new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            Random.Range(min.z, max.z)
        );
    }
}

[Serializable]
public struct WeaponIncrementUpgrade
{
    [field: SerializeField] public int damage { get; private set; }
    [field: SerializeField] public float fireRate { get; private set; }
    [field: SerializeField] public float bulletSpeed { get; private set; }

    [field: SerializeField] public float heatCapacity { get; private set; }
    [field: SerializeField] public float coolingSpeed { get; private set; }

}