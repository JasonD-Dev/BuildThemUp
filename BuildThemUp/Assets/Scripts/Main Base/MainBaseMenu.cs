using UnityEngine;

public class MainBaseMenu : MonoBehaviour
{
    public static MainBaseMenu instance { get; private set; }

    [SerializeField] Transform mPlayerTransform;
    [SerializeField] float mPlayerDetectionRange = 10f;
    
    [SerializeField] Canvas mSelectionMenuUI;
    [SerializeField] Canvas mBuyAndUpgradeMenuUI;
    [SerializeField] Canvas mUpgradeBaseMenuUI;

    bool mIsMenuActive = false;
    bool mIsSelectionMenuActive = false;
    bool mIsBuyAndUpgradeMenuActive = false;
    bool mIsUpgradeMenuActive = false;
    bool mIsPlayerWithinRange = false;

    float mMinMenuOpenTime = 0.1f;
    float mLastMenuOpenTimeStamp = 0;

    private byte mPromptId;

    //Keybinds
    KeyCode mOpenUIKey => UIBinds.openMainBaseUI;
    KeyCode mCloseUIKey => UIBinds.closeMainBaseUI;
    KeyCode mBackKey => UIBinds.back;

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
    }

    private void Start()
    {
        CloseMenu(); //Incase its open.
    }

    private void Update()
    {
        if (mIsPlayerWithinRange == true)
        {
            if (Input.GetKeyDown(mCloseUIKey) == true && Time.time - mLastMenuOpenTimeStamp >= mMinMenuOpenTime && mIsMenuActive == true)
            {
                CloseMenu();
            }
            else if (Input.GetKeyDown(mOpenUIKey) == true && mIsSelectionMenuActive == false)
            {
                DisplaySelectionMenu(true);
            }
        }
        else
        {
            if (mIsMenuActive == true)
            {
                CloseMenu();
            }
        }
    }

    private void FixedUpdate()
    {
        bool tPlayerWasInRange = mIsPlayerWithinRange;
        mIsPlayerWithinRange = Vector3.Distance(mPlayerTransform.position, transform.position) < mPlayerDetectionRange;
        
        if (!tPlayerWasInRange && mIsPlayerWithinRange && !mIsMenuActive)
            mPromptId = PromptManager.instance.ShowPrompt($"[{mOpenUIKey}] Buy and Upgrade", () => DisplaySelectionMenu(true), mOpenUIKey);
        else if (tPlayerWasInRange && !mIsPlayerWithinRange)
            PromptManager.instance.RemovePrompt(mPromptId);
        
        if (mIsMenuActive == true)
            Cursor.lockState = CursorLockMode.Confined;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }

    //TODO: Use Enum to more cleanly control switch between menus.
    public void CloseMenu()
    {
        mIsMenuActive = false;
        mIsSelectionMenuActive = false;
        mIsBuyAndUpgradeMenuActive = false;
        mIsUpgradeMenuActive = false;
        mIsPlayerWithinRange = false;

        mSelectionMenuUI.gameObject.SetActive(false);
        mBuyAndUpgradeMenuUI.gameObject.SetActive(false);
        mUpgradeBaseMenuUI.gameObject.SetActive(false);

        PlayerStates.isInMenu = false;
    }

    public void DisplaySelectionMenu(bool aBool)
    {
        mSelectionMenuUI.gameObject.SetActive(aBool);
        mIsSelectionMenuActive = aBool;

        if (aBool == true)
        {
            PromptManager.instance.RemovePrompt(mPromptId);
            
            DisplayBuyAndUpgradeMenu(false);
            DisplayUpgradeBaseMenu(false);
            PlayerStates.isInMenu = true;
        }

        mLastMenuOpenTimeStamp = Time.time;
        mIsMenuActive = IsMenuActive();
    }

    public void DisplayBuyAndUpgradeMenu(bool aBool)
    {
        mBuyAndUpgradeMenuUI.gameObject.SetActive(aBool);
        mIsBuyAndUpgradeMenuActive = aBool;

        if (aBool == true)
        {
            DisplaySelectionMenu(false);
            DisplayUpgradeBaseMenu(false);
            PlayerStates.isInMenu = true;
        }

        mLastMenuOpenTimeStamp = Time.time;
        mIsMenuActive = IsMenuActive();
    }

    public void DisplayUpgradeBaseMenu(bool aBool)
    {
        mUpgradeBaseMenuUI.gameObject.SetActive(aBool);
        mIsUpgradeMenuActive = aBool;

        if (aBool == true)
        {
            DisplaySelectionMenu(false);
            DisplayBuyAndUpgradeMenu(false);
            PlayerStates.isInMenu = true;
        }

        mLastMenuOpenTimeStamp = Time.time;
        mIsMenuActive = IsMenuActive();
    }

    private bool IsMenuActive() //If any of the menus are active.
    {
        return (mIsSelectionMenuActive || mIsBuyAndUpgradeMenuActive || mIsUpgradeMenuActive);
    }
}
