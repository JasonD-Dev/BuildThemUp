using System;
using UnityEngine;
using UnityEngine.AI;

public class TurretSpawner : Weapon
{
    [Header("Preview Display")]
    public Material previewValidMaterial;
    public Material previewErrorMaterial;
    [SerializeField] Prompt mPreviewErrorText;
    private string mError;
    private string mErrorDefault => "[" + mSwapTurretTypeButton.ToString() + "] Switch Turret";

    [Header("Turret Types")]
    [SerializeField] KeyCode mSwapTurretTypeButton = KeyCode.Q;
    [SerializeField] string turretTypeName1 = "LightTurret Lvl 1";
    [SerializeField] string turretTypeName2 = "HeavyTurret Lvl 1";
    private string tCurrentTurretTypeName; //assigned in InitialisePreview();

    private Transform mSpawnPreview;
    private bool mIsPreviewActive;
    private bool mIsError;

    [Header("Purchasing")]
    [SerializeField, Range(0, 20)] int mSlopeLimit = 15;
    [SerializeField] float mMaxDistanceFromGun = 5;
    [SerializeField] float mMaxDistanceFromOtherTurrets = 5;
    private float mMaxDistanceFromBase => MainBase.instance.connectionRange;
    

    [Header("Upgrading")]
    [SerializeField] TurretDisplay mStatsDisplay;
    [Tooltip("The angle the player can look within towards a turret for upgrading.")]
    [SerializeField] float mUpgradingThresholdAngle = 20;
    
    private int mPurchaseCost;

    protected override void Awake()
    {
        mError = mErrorDefault;
        InitialisePreview(turretTypeName1);
    }

    protected override void Update()
    {
        base.Update();
        
        if (Input.GetKeyDown(mSwapTurretTypeButton) == true)
        {
            if (tCurrentTurretTypeName == turretTypeName2)
            {
                Log.Info(this, $"Trying to switch to {turretTypeName1}");
                InitialisePreview(turretTypeName1);
            }
            else
            {
                Log.Info(this, $"Trying to switch to {turretTypeName2}");
                InitialisePreview(turretTypeName2);
            }
        }
    }

    private void InitialisePreview(string aTurretPrfabName)
    {
        if (mSpawnPreview != null)
        {
            Destroy(mSpawnPreview.gameObject);
        }
        
        Turret tTurretPrefab = TurretsManager.instance.GetTurretByName(aTurretPrfabName);
        mPurchaseCost = tTurretPrefab.scrapCost;
        mSpawnPreview = Instantiate(tTurretPrefab.gameObject, transform).transform;

        // remove the turret component
        Destroy(mSpawnPreview.GetComponent<TurretAnimator>());
        Destroy(mSpawnPreview.GetComponent<TurretUpgrade>()); 
        Destroy(mSpawnPreview.GetComponent<Turret>());
        
        // set all layers to ignore raycast
        mSpawnPreview.gameObject.layer = 2;
        var tChildren = mSpawnPreview.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var i in tChildren)
            i.gameObject.layer = 2;
        
        // remove collider
        Destroy(mSpawnPreview.GetComponent<Collider>());
        Destroy(mSpawnPreview.GetComponent<NavMeshObstacle>());

        SetPreviewMaterial(previewValidMaterial);

        mIsPreviewActive = false;
        mSpawnPreview.gameObject.SetActive(false);
        tCurrentTurretTypeName = aTurretPrfabName;
    }

    private void SetPreviewMaterial(Material aMaterial)
    {
        // set all mesh renderers to the preview material
        var tMeshRenderers = mSpawnPreview.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var i in tMeshRenderers)
        {
            i.sharedMaterial = aMaterial;
        }  
    }

    void SetPreviewActive(bool aBool)
    {
        mSpawnPreview.gameObject.SetActive(aBool);
        mIsPreviewActive = aBool;
    }

    public override void OnUnequip()
    {
        mIsPreviewActive = false;
        mSpawnPreview.gameObject.SetActive(false);
    }

    protected override void FixedUpdate()
    {
        base.Update();
        UpdateErrorUI();

        if (PlayerStates.isInMenu == true)
        {
            DisplayUpgradeUI(null);
            SetPreviewActive(false);
            return;
        }
        
        Transform tCameraTransform = CameraController.Instance.transform;
        Vector3 tCameraPosition = tCameraTransform.position;
        Vector3 tCameraDirection = tCameraTransform.forward;
        
        // this block checks if there is a valid turret that the player is looking at that can be upgraded
        TurretUpgrade tTurretUpgrade = null;
        float tClosestTurretDistance = mMaxDistanceFromGun;
        foreach (Turret tTurret in TurretsManager.instance.allTurrets)
        {
            Vector3 tCurrentTurretPosition = tTurret.transform.position;
            float tCurrentDistanceFromGun = Vector3.Distance(tCameraPosition, tCurrentTurretPosition);
            if (tCurrentDistanceFromGun >= tClosestTurretDistance)
            {
                //Log.Info(this, tTurret.name + " was too far, " + tCurrentDistanceFromGun);
                continue;
            }

            TurretUpgrade tCurrentTurretUpgrade = tTurret.GetComponent<TurretUpgrade>();
            if (tCurrentTurretUpgrade == null)
            {
                //Log.Info(this, tTurret.name + " is not upgradablee!");
                continue;
            }

            Vector3 tVerticalCameraDirection = tCameraDirection;
            Vector3 tVerticalCurrentTurretDirection = (tCurrentTurretPosition - tCameraPosition).normalized;
            tVerticalCurrentTurretDirection.y = 0;
            tVerticalCameraDirection.y = 0;

            float tCameraToCurrentTurretAngle = Vector3.Angle(tVerticalCameraDirection, tVerticalCurrentTurretDirection);
            if (tCameraToCurrentTurretAngle <= mUpgradingThresholdAngle)
            {
                tTurretUpgrade = tCurrentTurretUpgrade;
                tClosestTurretDistance = tCurrentDistanceFromGun;
                continue;
            }
            //Log.Info(this, tTurret.name + " did not meet the criteria");
        }

        if (tTurretUpgrade != null)
        {
            DisplayUpgradeUI(tTurretUpgrade);
            SetPreviewActive(false);
        }
        else
        {
            if (DisplayPlacementPreview(tCameraPosition, tCameraDirection))
                DisplayTurretPreviewUI();
            else
                DisplayUpgradeUI(null);
        }
    }

    public override bool Shoot()
    {
        mFireTimer = 1f / fireRate;
        mLastShotTimeStamp = Time.time;

        if (mIsPreviewActive == false || mIsError == true)
        {
            return false;
        }

        bool tIsTransactionCompleted = GamePlayManager.instance.SpendScrap(mPurchaseCost);
        if (tIsTransactionCompleted == false)
        {
            Log.Warning(this, "The player does not have enough scrap to purchase this turret.");
            return false;
        }

        Turret tTurret = TurretsManager.instance.SpawnTurret(tCurrentTurretTypeName, mSpawnPreview.position, mSpawnPreview.rotation);
        return tTurret != null;
    }

    private bool DisplayPlacementPreview(Vector3 aCameraPosition, Vector3 aCameraDirection)
    {
        Ray tRay = new Ray(aCameraPosition, aCameraDirection);
        bool tRayWasHit = Physics.Raycast(tRay, out var tHit, mMaxDistanceFromGun, LoadoutController.raycastMask);

        SetPreviewActive(tRayWasHit);
        if (tRayWasHit == false)
            return false;

        mIsError = false;

        // Do the cheap error checks first.
        float tHitDistanceFromBase = Vector3.Distance(GamePlayManager.instance.mainBasePostion, tHit.point);
        float tHitSlope = Vector3.Angle(Vector3.up, tHit.normal);

        if (!mIsError)
        {
            foreach (Turret tTurret in TurretsManager.instance.allTurrets)
            {
                float tCurrentDistance = Vector3.Distance(tTurret.transform.position, tHit.point);
                if (tCurrentDistance < mMaxDistanceFromOtherTurrets)
                {
                    mError = "Turret too close";
                    mIsError = true;
                    break;
                }
            }
        }
        if (tHitSlope > mSlopeLimit)
        {
            mIsError = true;
            mError = "Turret can't go here";
        }
        else if (tHitDistanceFromBase > mMaxDistanceFromBase)
        {
            mIsError = true;
            mError = "Too far from Main base";
        }
        else if (!GamePlayManager.instance.HasSufficientScrap(mPurchaseCost))
        {
            mIsError = true;
            mError = "Not enough scrap";
        }

        mSpawnPreview.up = tHit.normal;
        mSpawnPreview.position = tHit.point;

        if (mIsError == true)
            SetPreviewMaterial(previewErrorMaterial);
        else
        {
            mError = mErrorDefault;
            SetPreviewMaterial(previewValidMaterial);
        }

        return true;
    }

    private void DisplayUpgradeUI(TurretUpgrade tTurretUpgrade)
    {
        if (mStatsDisplay.upgradable != tTurretUpgrade)
        {
            mStatsDisplay.RefreshDisplay(tTurretUpgrade);
        }
        mStatsDisplay.gameObject.SetActive(tTurretUpgrade != null);
    }
    
    private void DisplayTurretPreviewUI()
    {
        var tTurret = TurretsManager.instance.GetTurretByName(tCurrentTurretTypeName);
        var tUpgrade = tTurret.GetComponent<TurretUpgrade>();

        if (mStatsDisplay.upgradable != tUpgrade)
            mStatsDisplay.RefreshDisplayBaseStats(tUpgrade, tTurret);
        
        mStatsDisplay.gameObject.SetActive(tTurret != null);
    }

    private void UpdateErrorUI()
    {
        bool tIsTextSet = mPreviewErrorText.SetText(mError);

        if (tIsTextSet)
        {
            mPreviewErrorText.Deactivate();
            mPreviewErrorText.Activate();
        }
    }
}