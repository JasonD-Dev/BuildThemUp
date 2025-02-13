
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyRadar))]
[RequireComponent(typeof(MainBaseHealth))]
[RequireComponent(typeof(MainBaseUpgrade))]
public class MainBase : MonoBehaviour, IUpgradeable<MainBaseIncrementUpgrade>
{
    public static MainBase instance { get; private set; }

    [field: SerializeField] public float connectionRange { get; private set; } = 30;
    [SerializeField, Range(0.5f, 3)] float mHealOnUpgradeRatio = 2f;

    private MainBaseHealth mHealth;
    private EnemyRadar mRadar;
    private MainBaseUpgrade mUpgradeComponent;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Log.Error(this, $"There were multiple instances of {typeof(MainBase)} found. Destroying this one!");
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        mHealth = GetComponent<MainBaseHealth>();
        mUpgradeComponent = GetComponent<MainBaseUpgrade>();
        mRadar = GetComponent<EnemyRadar>();
    }

    //Increment upgrade, so the value is added rather than assigned.
    public void OnUpgrade(MainBaseIncrementUpgrade aOther)
    {
        connectionRange += aOther.connectionRange;

        //Heal on upgrade
        mHealth.IncreaseMaxHealth(aOther.maxHealth);
        mHealth.Heal(aOther.maxHealth * mHealOnUpgradeRatio);

        //TEMP
        Health tPlayerHealth = GamePlayManager.instance.playerTransform.GetComponent<PlayerHealth>();
        tPlayerHealth.Heal(tPlayerHealth.maxHealth * 0.5f);

        //Upgrade Enemy Radar
        mRadar.SetDelay(mRadar.radarDelay + aOther.radarDelay);
        mRadar.SetRange(mRadar.radarRange + aOther.radarRange);
    }

    public KeyValuePair<string, string>[] GetNextUpgradeStats()
    {
        if (mUpgradeComponent.IsMaxed() == true) 
        {
            return GetCurrentStats();
        }

        MainBaseIncrementUpgrade aOther = mUpgradeComponent.GetUpgradeData().upgrade;

        KeyValuePair<string, string> tHealthKeyValuePair = new KeyValuePair<string, string>("Max Health", UpgradeDisplay<MainBaseIncrementUpgrade>.UpgradeStatFormat(mHealth.maxHealth, aOther.maxHealth));
        KeyValuePair<string, string> tHealKeyValuePair = new KeyValuePair<string, string>("Heal", (aOther.maxHealth * mHealOnUpgradeRatio).ToString());

        KeyValuePair<string, string> tConnectionKeyValuePair = new KeyValuePair<string, string>("Connection Range", UpgradeDisplay<MainBaseIncrementUpgrade>.UpgradeStatFormat(connectionRange, aOther.connectionRange));

        KeyValuePair<string, string> tRadarDelayKeyValuePair = new KeyValuePair<string, string>("Radar Delay", UpgradeDisplay<MainBaseIncrementUpgrade>.UpgradeStatFormat(mRadar.radarDelay, aOther.radarDelay));
        KeyValuePair<string, string> tRadarRangeKeyValuePair = new KeyValuePair<string, string>("Radar Range", UpgradeDisplay<MainBaseIncrementUpgrade>.UpgradeStatFormat(mRadar.radarRange, aOther.radarRange));
        

        return new KeyValuePair<string, string>[] { tHealthKeyValuePair, tHealKeyValuePair, tConnectionKeyValuePair, tRadarRangeKeyValuePair, tRadarDelayKeyValuePair };
    }

    public KeyValuePair<string, string>[] GetCurrentStats(bool includeCost = false)
    {
        KeyValuePair<string, string> tHealthKeyValuePair = new KeyValuePair<string, string>("Max Health", mHealth.maxHealth.ToString());

        KeyValuePair<string, string> tConnectionKeyValuePair = new KeyValuePair<string, string>("Connection Range", connectionRange.ToString());

        KeyValuePair<string, string> tRadarDelayKeyValuePair = new KeyValuePair<string, string>("Radar Delay", mRadar.radarDelay.ToString());
        KeyValuePair<string, string> tRadarRangeKeyValuePair = new KeyValuePair<string, string>("Radar Range", mRadar.radarRange.ToString());

        return new KeyValuePair<string, string>[] { tHealthKeyValuePair, tConnectionKeyValuePair, tRadarRangeKeyValuePair, tRadarDelayKeyValuePair };
    }
}
