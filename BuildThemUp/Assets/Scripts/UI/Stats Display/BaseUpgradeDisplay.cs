
using UnityEngine;

public class BaseUpgradeDisplay : UpgradeDisplay<MainBaseIncrementUpgrade>
{
    [Header("Main Base Upgrade")]
    [SerializeField] MainBaseUpgrade mMainBaseUpgrade;

    public override void Start()
    {
        base.Start();
        RefreshDisplay(mMainBaseUpgrade);
    }

    void OnEnable()
    {
        if (mMainBaseUpgrade != null)
        {
            RefreshDisplay(mMainBaseUpgrade);
        }
        else
        {
            Log.Warning(this, "Upgrade component was null. This is normal if this happens on game initalisation.");
        }
    }
}
