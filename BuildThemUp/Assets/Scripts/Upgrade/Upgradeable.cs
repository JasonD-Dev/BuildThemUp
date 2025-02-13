using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct UpgradeData<T>
{
    [field: SerializeField] public Sprite icon { get; private set; }
    [field: SerializeField] public int cost { get; private set; }
    [field: SerializeField] internal T upgrade { get; private set; }
}

public class Upgradeable<T> : MonoBehaviour
{
    public IUpgradeable<T> current { get; private set; }
    private int currentLevel = 1;

    [field: SerializeField] public Sprite defaultIcon { get; private set; }
    [field: SerializeField] public List<UpgradeData<T>> upgrades { get; private set; }

    [Header("Sound")]
    [SerializeField] AudioClip mUpgradeSound;
    [SerializeField, Range(0f, 1f)] float mUpgradeSoundVolume = 1;

    private void Awake()
    {
        current = GetComponent<IUpgradeable<T>>();

        if (current == null)
        {
            Log.Warning(this, "There is no upgradeable component on this game object!");
        }
    }

    public bool Upgrade()
    {
        if (current == null)
        {
            Log.Error(this, "There is no upgradable component on this game object!");
            return false;
        }

        if (IsMaxed() == true)
        {
            Log.Info(this, "Already at max upgrade level!");
            return false;
        }

        UpgradeData<T> tUpgradeData = GetUpgradeData();
        if (tUpgradeData.upgrade == null)
        {
            Log.Error(this, $"Upgrade data for upgrade level index {currentLevel} is null!");
            return false;
        }

        int tPlayerScrap = GamePlayManager.instance.totalScrap;
        if (tPlayerScrap < tUpgradeData.cost)
        {
            Log.Info(this, $"Player does not have enough scrap to upgrade! Current scrap amount: {tPlayerScrap}");
            return false;
        }

        if (tUpgradeData.cost == 0) 
        {
            Log.Info(this, $"The upgrade to upgrade level index {currentLevel} is set to be free!.");
        }

        bool tPurchaseStatus = GamePlayManager.instance.SpendScrap(tUpgradeData.cost);

        if (tPurchaseStatus == false)
        {
            Log.Info(this, $"Transaction failed.");
            return false;
        }

        current.OnUpgrade(tUpgradeData.upgrade);
        currentLevel++;

        if (mUpgradeSound != null)
        {
            PlayerController.instance.PlayOneShot(mUpgradeSound, mUpgradeSoundVolume);
        }

        return true;
    }

    public void Copy(Upgradeable<T> aOther)
    {
        currentLevel = aOther.currentLevel;
        upgrades = aOther.upgrades;
        //shouldnt need to copy the current upgradeable variable.
    }

    public void IncrementLevel() // public incase it needs to be increments explicitly outside.
    {
        currentLevel++;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public UpgradeData<T> GetUpgradeData()
    {
        return upgrades[Mathf.Clamp(currentLevel - 1, 0, upgrades.Count - 1)];
    }

    public bool IsMaxed()
    {
        return (currentLevel - 1 >= upgrades.Count);
    }
}

