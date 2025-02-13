using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    /*
    public static UpgradeManager instance { get; private set; }
    public GamePlayManager mGamePlayManager = GamePlayManager.instance;
    public Text upgradeCostText;
    public Text mScrapNumber;
    public Button upgradeButton; // Will need to set up button
    private UpgradeableBuilding selectedBuilding; // The building currently selected for upgrade

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        UpdateUpgradeUI();
    }

    public void SelectBuildingForUpgrade(UpgradeableBuilding building)
    {
        selectedBuilding = building;
        UpdateUpgradeUI();
    }

    public void UpgradeSelectedBuilding()
    {
        if (selectedBuilding != null)
        {
            int upgradeCost = selectedBuilding.GetUpgradeCost();
            if (mGamePlayManager.SpendScrap(upgradeCost)) // Check if player has enough resources
            {
                selectedBuilding.UpgradeBuilding(); // Upgrade the building
                UpdateUpgradeUI(); // Update UI after upgrade
            }
            else
            {
                Debug.Log("Insufficient resources to upgrade!");
            }
        }

    }

    private void UpdateUpgradeUI()
    {
        mScrapNumber.text = mGamePlayManager.totalScrap.ToString();

        if (selectedBuilding != null)
        {
            upgradeCostText.text = "Upgrade Cost: " + selectedBuilding.GetUpgradeCost();
            upgradeButton.interactable = mGamePlayManager.SpendScrap(selectedBuilding.GetUpgradeCost());
        }
        else
        {
            //upgradeCostText.text = "Upgrade Cost: N/A";
            //upgradeButton.interactable = false;
        }
        

    }
    */
}
   