using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponStatusList : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] LoadoutController mLoadoutController;
    [SerializeField] WeaponUpgradeDisplay mStatsDisplay;

    [Header("Prefabs Components")]
    [SerializeField] RectTransform mButtonPrefab;
    [SerializeField] TextMeshProUGUI mTextPrfab;

    [Header("Shape")]
    [SerializeField] Vector2 mListElementSize;
    [SerializeField] int mYAxisMargin;

    private WeaponsListElementData[] allWeaponListElements = new WeaponsListElementData[0];

    private void Awake()
    {
        if (mLoadoutController == null)
        {
            Log.Error(this, "The loadout controller not set in the inspector!");
            Destroy(gameObject);
            return;
        }

        if (mStatsDisplay == null)
        {
            Log.Error(this, "The weapopns upgrade stats display not set in the inspector!");
            Destroy(gameObject);
            return;
        }

        if (mButtonPrefab == null)
        {
            Log.Error(this, "The button prefab not set in the inspector!");
            Destroy(gameObject);
            return;
        }

        if (mTextPrfab == null)
        {
            Log.Error(this, "The text prefab not set in the inspector!");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Invoke("Intialise", 1);
    }

    void Intialise()
    {
        Weapon[] allWeapons; allWeapons = WeaponsPrefabManager.Instance.weapons;
        allWeaponListElements = new WeaponsListElementData[allWeapons.Length];

        for (int i = 0; i < allWeapons.Length; i++)
        {
            Weapon tCurrentWeapon = allWeapons[i];
            if (tCurrentWeapon == null)
            {
                Log.Warning(this, $"Weapon at index {i} ({i + 1}) in the weapons list is empty.");
                continue;
            }

            //Default sizes so to create the same Vector2 var multiple times.
            Vector2 tDefaultVectorSize1 = new Vector2(0, 1);
            Vector2 tDefaultVectorSize2 = new Vector2(0, 0.5f);
            Vector2 tDefaultVectorSize3 = new Vector2(1, 0.5f);

            //Background
            RectTransform mButtonObject = Instantiate(mButtonPrefab, transform);
            mButtonObject.anchorMin = tDefaultVectorSize1;
            mButtonObject.anchorMax = tDefaultVectorSize1;
            mButtonObject.pivot = tDefaultVectorSize1;
            mButtonObject.sizeDelta = mListElementSize;
            mButtonObject.anchoredPosition = new Vector2(0, -(i * (mButtonObject.rect.size.y + mYAxisMargin)));
            mButtonObject.name = tCurrentWeapon.displayName + " Element";

            //Name text
            TextMeshProUGUI tNameText;
            RectTransform tNameTransform;

            tNameText = Instantiate(mTextPrfab, mButtonObject);
            tNameText.alignment = TextAlignmentOptions.Left;
            tNameText.text = tCurrentWeapon.displayName;

            tNameTransform = tNameText.rectTransform;
            tNameTransform.sizeDelta = new Vector2(0, mListElementSize.y);
            tNameTransform.pivot = tDefaultVectorSize2;
            tNameTransform.anchorMin = tDefaultVectorSize2;
            tNameTransform.anchorMax = tDefaultVectorSize2;
            tNameTransform.anchoredPosition = new Vector2(30, 0);

            //Status text
            TextMeshProUGUI tStatusText;
            RectTransform tStatusTransform;

            tStatusText = Instantiate(mTextPrfab, mButtonObject);
            tStatusText.alignment = TextAlignmentOptions.Right;

            tStatusTransform = tStatusText.rectTransform;
            tStatusTransform.sizeDelta = new Vector2(0, mListElementSize.y);
            tStatusTransform.pivot = tDefaultVectorSize2;
            tStatusTransform.anchorMin = tDefaultVectorSize3;
            tStatusTransform.anchorMax = tDefaultVectorSize3;
            tStatusTransform.anchoredPosition = new Vector2(-30, 0);

            WeaponsListElementData aListElementData = new WeaponsListElementData(tStatusText, tCurrentWeapon, mLoadoutController);
            aListElementData.UpdateStatus();
            allWeaponListElements[i] = aListElementData;

            //Button
            Button mButton = mButtonObject.GetComponent<Button>();
            Weapon tInventoryWeapon = mLoadoutController.GetWeapon(tCurrentWeapon);
            WeaponUpgrade tInventoryWeaponUpgrade;
            if (tInventoryWeapon == null)
            {
                tInventoryWeaponUpgrade = null;
                Log.Error(this, "Null weapon.");
            }
            else
            {
                tInventoryWeaponUpgrade = tInventoryWeapon.GetComponent<WeaponUpgrade>();

                if (tInventoryWeaponUpgrade == null)
                {
                    Log.Warning(this, "Null weapon upgrade.");
                }
            }
            mButton.onClick.AddListener(() => mStatsDisplay.RefreshDisplay(tInventoryWeaponUpgrade));
        }
    }

    void OnEnable()
    {
        UpdateAllWeaponsStatus();
    }

    public void UpdateAllWeaponsStatus()
    {
        foreach(WeaponsListElementData aListElement in allWeaponListElements)
        {
            aListElement.UpdateStatus();
        }
    }
}

public struct WeaponsListElementData
{
    public TextMeshProUGUI status { get; private set; }
    public Weapon item { get; private set; }
    public LoadoutController loadoutController{ get; private set; }

    public WeaponsListElementData(TextMeshProUGUI aStatus, Weapon aItem, LoadoutController aLoadoutController)
    {
        status = aStatus;
        item = aItem;
        loadoutController = aLoadoutController;
    }

    public void UpdateStatus()
    {
        Item tItem = loadoutController.GetWeapon(item);

        if (tItem == null)
        {
            status.text = "Not Owned";
            return;
        }

        Upgradeable<WeaponIncrementUpgrade> mUpgradeComponent = tItem.GetComponent<Upgradeable<WeaponIncrementUpgrade>>();
        if (mUpgradeComponent == null)
        {
            status.text = "Not Upgradeable";
            return;
        }

        if (mUpgradeComponent.IsMaxed() == true)
        {
            status.text = "Max";
            return;
        }

        status.text = "Level " + mUpgradeComponent.GetCurrentLevel().ToString();
    }
}
