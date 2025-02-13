using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeDisplay<T> : MonoBehaviour
{
    [Header("Shape")]
    [SerializeField] Vector2 mListElementSize;
    [SerializeField] int mYAxisMargin;

    [Header("Prefabs Components")]
    [SerializeField] RectTransform mBackGroundPrefab;
    [SerializeField] TextMeshProUGUI mTextPrfab;

    [Header("Icon")]
    [SerializeField] Image mIconImage;
    [SerializeField] RectTransform mIconImageBackground;
    float mVerticalOffset; //Assigned in start();
    float mCurrentVerticalOffset = 0;

    [Header("Upgrade Button")]
    [SerializeField] TypeWriterEffect mUpgradeButtonText;
    [SerializeField] Button mUpgradeButton;

    KeyCode mUpgradeKey => UIBinds.upgradeKey;

    public Upgradeable<T> upgradable { get; private set; }
    private KeyValuePair<string, string>[] mStatsList = new KeyValuePair<string, string>[0];
    private GameObject[] mAllListElementGameObjects = new GameObject[0];

    private void Awake()
    {
        if (mUpgradeButton == null || mUpgradeButtonText == null)
        {
            Log.Error(this, "Not all upgrade button components are set in the inspector!");
            Destroy(this);
            return;
        }

        if (mIconImage == null || mIconImageBackground == null)
        {
            Log.Error(this, "Not all icon sprite components are not assigned in the inspector!");
            Destroy(this); 
            return;
        }

        if (mBackGroundPrefab == null)
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

    public virtual void Start()
    {
        mVerticalOffset = mIconImageBackground.rect.height + mYAxisMargin;
        RefreshDisplay(null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(mUpgradeKey))
        {
            if (mUpgradeButton.gameObject.activeSelf == true && mUpgradeButton.interactable == true)
            {
                PurchaseCurrentUpgrade();
            }
        }
    }

    // for showing base stats
    public void RefreshDisplayBaseStats(Upgradeable<T> aUpgradeable, IUpgradeable<T> aCurrent)
    {
        upgradable = aUpgradeable;
        mStatsList = aCurrent.GetCurrentStats(true);
        mIconImage.sprite = aUpgradeable.defaultIcon;
        mCurrentVerticalOffset = mVerticalOffset;
        
        // set upgrade button to false to indicate we aren't actually upgrading
        mUpgradeButton.gameObject.SetActive(false);

        mIconImage.gameObject.SetActive(true);
        mIconImageBackground.gameObject.SetActive(true);

        CreateNewList();
    }

    public void RefreshDisplay(Upgradeable<T> aUpgradeable)
    {
        upgradable = aUpgradeable;
        Sprite tSprite = null;

        if (upgradable == null)
        {
            mCurrentVerticalOffset = 0;
            mStatsList = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Not Upgradeable", " ") };
        }
        else
        {
            mStatsList = aUpgradeable.current.GetNextUpgradeStats();

            if (upgradable.IsMaxed() == true)
            {
                tSprite = upgradable.defaultIcon;
            }
            else
            {
                tSprite = upgradable.GetUpgradeData().icon;
                if (tSprite == null)
                {
                    tSprite = upgradable.defaultIcon;
                }
            }
            
            if (tSprite != null)
            {
                mIconImage.sprite = tSprite; 
            }

            mCurrentVerticalOffset = (tSprite == null) ? 0 : mVerticalOffset;
        }

        mUpgradeButton.gameObject.SetActive(aUpgradeable);

        mIconImage.gameObject.SetActive(tSprite);
        mIconImageBackground.gameObject.SetActive(tSprite);

        CreateNewList();
    }

    private void CreateNewList()
    {
        DeleteList();

        if (mStatsList == null || mStatsList.Length == 0)
        {
            Log.Info(this, "The stats list wasempty while creating a new list");
            mUpgradeButton.gameObject.SetActive(false);
            mUpgradeButton.interactable = false;
            return;
        }

        mAllListElementGameObjects = new GameObject[mStatsList.Length];

        for (int i = 0; i < mStatsList.Length; i++)
        {
            KeyValuePair<string, string> tCurrentStat = mStatsList[i];

            //Default sizes so its not creating the same Vector2 var multiple times.
            Vector2 tDefaultVectorSize1 = new Vector2(0, 1);
            Vector2 tDefaultVectorSize2 = new Vector2(0, 0.5f);
            Vector2 tDefaultVectorSize3 = new Vector2(1, 0.5f);

            //Background
            RectTransform mBackGround = Instantiate(mBackGroundPrefab, transform);
            mBackGround.anchorMin = tDefaultVectorSize1;
            mBackGround.anchorMax = tDefaultVectorSize1;
            mBackGround.pivot = tDefaultVectorSize1;
            mBackGround.sizeDelta = mListElementSize;
            mBackGround.anchoredPosition = new Vector2(mIconImageBackground.anchoredPosition.x, - (i * (mListElementSize.y + mYAxisMargin) + mCurrentVerticalOffset));
            mBackGround.name = tCurrentStat.Key + " Element";
            mAllListElementGameObjects[i] = mBackGround.gameObject;

            //Name text
            TextMeshProUGUI tNameText;
            RectTransform tNameTransform;

            tNameText = Instantiate(mTextPrfab, mBackGround);
            tNameText.alignment = TextAlignmentOptions.Left;
            tNameText.text = tCurrentStat.Key;

            tNameTransform = tNameText.rectTransform;
            tNameTransform.sizeDelta = new Vector2(0, mListElementSize.y);
            tNameTransform.pivot = tDefaultVectorSize2;
            tNameTransform.anchorMin = tDefaultVectorSize2;
            tNameTransform.anchorMax = tDefaultVectorSize2;
            tNameTransform.anchoredPosition = new Vector2(30, 0);

            //Status text
            TextMeshProUGUI tStatusText;
            RectTransform tStatusTransform;

            tStatusText = Instantiate(mTextPrfab, mBackGround);
            tStatusText.alignment = TextAlignmentOptions.Right;
            tStatusText.text = tCurrentStat.Value;

            tStatusTransform = tStatusText.rectTransform;
            tStatusTransform.sizeDelta = new Vector2(0, mListElementSize.y);
            tStatusTransform.pivot = tDefaultVectorSize2;
            tStatusTransform.anchorMin = tDefaultVectorSize3;
            tStatusTransform.anchorMax = tDefaultVectorSize3;
            tStatusTransform.anchoredPosition = new Vector2(-30, 0);
        }

        RectTransform tUpgradeButtonRect = mUpgradeButton.GetComponent<RectTransform>();
        tUpgradeButtonRect.anchoredPosition = new Vector2(tUpgradeButtonRect.anchoredPosition.x, - (mStatsList.Length * (mListElementSize.y + mYAxisMargin) + mCurrentVerticalOffset));

        SetupUpgradeButtonInteractibility();
    }

    private void SetupUpgradeButtonInteractibility()
    {
        bool tInteratibility = true;
        if (upgradable == null)
        {
            tInteratibility = false;
            mUpgradeButtonText.SetText("Null");
        }
        else if (upgradable.IsMaxed() == true)
        {
            tInteratibility = false;
            mUpgradeButtonText.SetText("Max");
        }
        else if (GamePlayManager.instance.HasSufficientScrap(upgradable.GetUpgradeData().cost) == false)
        {
            tInteratibility = false;
            mUpgradeButtonText.SetText($"Need [{upgradable.GetUpgradeData().cost - GamePlayManager.instance.totalScrap}] more.");
        }
        else
        {
            mUpgradeButtonText.SetText("[" + mUpgradeKey.ToString() + "] " + upgradable.GetUpgradeData().cost.ToString() + " Scrap");
        }

        mUpgradeButton.interactable = tInteratibility;
    }

    private void DeleteList()
    {
        foreach(GameObject aListElement in mAllListElementGameObjects)
        {
            Destroy(aListElement);
        }
    }

    public static string UpgradeStatFormat(float aInitial, float aUpgrade, bool isAdditive = true)
    {
        if (isAdditive == true)
        {
            if (aUpgrade == 0)
            {
                return aInitial.ToString();
            }

            return $"{aInitial} > {aInitial + aUpgrade}";
        }

        if (aUpgrade == aInitial)
        {
            return aInitial.ToString();
        }

        return $"{aInitial} > {aUpgrade}";
    }

    public void PurchaseCurrentUpgrade()
    {
        if (upgradable == null)
        {
            Log.Info(this, "The upgradeable is null. Cannot upgrade.");
            return;
        }

        upgradable.Upgrade();

        RefreshDisplay(upgradable); //refresh
    }
}