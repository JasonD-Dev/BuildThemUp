using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LoadoutController : MonoBehaviour
{
    public LoadoutAnimator animator;
    
    [SerializeField] private AudioSource mItemAudioSource;

    public bool UseHeld => mUseHeld && !animator.equipping;
    public Weapon equippedWeapon => mCurrentWeapon;

    [SerializeField] private Image mShtogunImage;
    [SerializeField] private Image mSniperImage;
    [SerializeField] private Image mAssaultRifleImage;
    [SerializeField] private Image mGrenadeLauncherImage;
    [SerializeField] private Image mTurretSpawnerImage;
    private float mEquippedOpacity = 1f;
    private float mUnequippedOpacity = 0.4f;

    private Inventory<Weapon> mLoadout;
    private Weapon mCurrentWeapon;
    private int mCurrentIndex;
    private bool mUseHeld;

    private float mADSLookMultiplier = 0.125f;
    private float mADSMoveMultiplier = 0.125f;
    private float mADSRecoilMultiplier = 0.75f;

    public int raycastMask { get; private set; }

    private void Awake()
    {
        mLoadout = new Inventory<Weapon>(5);
    }

    private void Start()
    {
        AddWeapon("NewSniper");
        AddWeapon("ShotgunNew");
        AddWeapon("TestHeavyAR");
        AddWeapon("TestGrenadeLauncher");
        AddWeapon("TurretSpawner");

        mCurrentIndex = 0;
        Equip(mLoadout.GetItem(mCurrentIndex));

        if (!animator)
            Log.Error(this, "LoadoutController loaded without LoadoutAnimator assigned!");

        // bitwise complement of layermask inverts it
        raycastMask = ~LayerMask.GetMask("FirstPersonView");
    }

    private void AddWeapon(string name)
    {
        // for testing only
        Weapon tWeapon = WeaponsPrefabManager.Instance.GetWeaponsPrefab(name);
        Weapon tWeaponInstance = Instantiate(tWeapon, animator.weaponParentTransform);
        if (tWeaponInstance == null)
        {
            Log.Warning(this, $"{name} is not a weapon!");
            return;
        }
        
        tWeaponInstance.Initialise(this);
        
        // set layer in children too
        var tChildren = tWeaponInstance.GetComponentsInChildren<Transform>(includeInactive: true);
        
        // invert the inverted raycast mask
        var tLayer = LayerMask.NameToLayer("FirstPersonView");
        tWeaponInstance.gameObject.layer = tLayer;
        foreach (var i in tChildren)
            i.gameObject.layer = tLayer;
        
        mCurrentIndex = mLoadout.AddItem(tWeaponInstance);
        Equip(tWeaponInstance);
    }

    public Weapon GetWeapon(int aIndex)
    {
        return mLoadout.GetItem(aIndex).GetComponent<Weapon>();
    }

    public Weapon GetWeapon(Weapon aWeapon) //Returns the weapon from the inventory if it matches.
    {
        return mLoadout.GetItem(aWeapon);
    }

    public void PlayAudioClip(AudioClip aClip, float aPitch, float aVolume = 1)
    {
        mItemAudioSource.pitch = aPitch;
        mItemAudioSource.PlayOneShot(aClip, aVolume);
    }

    public bool IsAudioClipPlaying()
    {
        return mItemAudioSource.isPlaying;
    }

    public void StopAudioClip()
    {
        mItemAudioSource.Stop();
    }

    public void Equip(Weapon aWeapon)
    {
        if (mCurrentWeapon != null)
        {
            mCurrentWeapon.gameObject.SetActive(false);
            mCurrentWeapon.OnUnequip();
        }
        
        mCurrentWeapon = aWeapon;
        mCurrentWeapon.gameObject.SetActive(true);
        mCurrentWeapon.OnEquip();
        
        animator.OnEquip();

        ///TESTING
        /*string msg = $"Equipped weapon: {mCurrentEquipped.DisplayName} , Index: {mCurrentIndex}";
        Debug.Log(msg);*/
    }
    
    public void OnLook(Vector2 aLookAmount)
    {
        Vector2 tLookAmount = aLookAmount;
        if (mCurrentWeapon.aimDownSights != null)
        {
            if (mCurrentWeapon.aimDownSights.isADS == true)
            {
                tLookAmount *= mADSLookMultiplier;
            }
        }
        animator.OnLook(tLookAmount);
    }

    public void OnMove(Vector3 aVelocity, bool aIsGrounded)
    {
        Vector3 tVelocity = aVelocity;
        if (mCurrentWeapon.aimDownSights != null)
        {
            if (mCurrentWeapon.aimDownSights.isADS == true)
            {
                tVelocity *= mADSMoveMultiplier;
            }
        }
        animator.OnMove(tVelocity, aIsGrounded);
    }

    public void ApplyRecoil (Vector3 aRecoilPosition, Vector3 aRecoilRotation)
    {
        float tMultiplier = 1f;
        if (mCurrentWeapon.aimDownSights != null)
        {
            if (mCurrentWeapon.aimDownSights.isADS == true)
            {
                tMultiplier = mADSRecoilMultiplier;
            }
        }
        animator.ApplyRecoil(aRecoilPosition * tMultiplier, aRecoilRotation * tMultiplier);
    }

    public void OnUse(InputAction.CallbackContext context)
    {
        // note:     context.canceled is key up
        //           context.started is key down
        //           context.performed is key held
        // but started+performed are both just called when key is down
        // so ignore context.performed
        
        mUseHeld = !context.canceled;

        if (!context.performed && !animator.equipping)
            mCurrentWeapon.Use(context.canceled);
    }
    
    public void OnChangeItem(InputAction.CallbackContext context)
    {
        // only do change when scroll happens
        if (context.started)
        {
            var tScroll = context.ReadValue<Vector2>();

            var tNewIndex = mCurrentIndex;

            bool tFoundItem = false;
            while (!tFoundItem)
            {
                tNewIndex -= (int)tScroll.y;
                
                // we iterated the whole loadout and didn't find anything
                // so doesn't matter
                if (tNewIndex == mCurrentIndex)
                    return;
                
                // wrap index around
                if (tNewIndex < 0)
                    tNewIndex = mLoadout.Count - 1;
                else if (tNewIndex >= mLoadout.Count)
                    tNewIndex = 0;
                
                // check if the new index has a valid item
                // if so: equip this item, loop breaks
                // if not: loop continues
                var tItem = mLoadout.GetItem(tNewIndex);
                tFoundItem = tItem != null;
                if (tFoundItem)
                {
                    mCurrentIndex = tNewIndex;
                    Equip(tItem);
                }
            }
        }
    }

    // Equipped weapon opacity
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            UpdateEquip('1');
        }
        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            UpdateEquip('2');
        }
        if (Input.GetKeyUp(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            UpdateEquip('3');
        }
        if (Input.GetKeyUp(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            UpdateEquip('4');
        }
        if (Input.GetKeyUp(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            UpdateEquip('5');
        }
        UpdateOpacity(mCurrentIndex);
    }
    private void UpdateOpacity(int aCurrentWeaponIndex)
    {
        switch (aCurrentWeaponIndex)
        {
            case 0:
                ResetOpacity();
                SetUIOpacity(mSniperImage, mEquippedOpacity);
                break;
            case 1:
                ResetOpacity();
                SetUIOpacity(mShtogunImage, mEquippedOpacity);
                break;
            case 2:
                ResetOpacity();
                SetUIOpacity(mAssaultRifleImage, mEquippedOpacity);
                break;
            case 3:
                ResetOpacity();
                SetUIOpacity(mGrenadeLauncherImage, mEquippedOpacity);
                break;
            case 4:
                ResetOpacity();
                SetUIOpacity(mTurretSpawnerImage, mEquippedOpacity);
                break;
            default:
                ResetOpacity();
                break;
                
        }
    }

    private void ResetOpacity()
    {
        SetUIOpacity(mShtogunImage, mUnequippedOpacity);
        SetUIOpacity(mSniperImage, mUnequippedOpacity);
        SetUIOpacity(mAssaultRifleImage, mUnequippedOpacity);
        SetUIOpacity(mGrenadeLauncherImage, mUnequippedOpacity);
        SetUIOpacity(mTurretSpawnerImage, mUnequippedOpacity);
    }

    void SetUIOpacity(Image aImage, float aOpacity)
    {
        if (aImage != null)
        {
            Color color = aImage.color;
            color.a = Mathf.Clamp(aOpacity, 0f, 1f);
            aImage.color = color;
        }
    }

    private void UpdateEquip(char key)
    {
        switch (key)
        {
            case '1':
                if (mCurrentIndex == 0)
                {
                    break;
                }
                mCurrentIndex = 0;
                Equip(mLoadout.GetItem(mCurrentIndex));
                break;
            case '2':
                if (mCurrentIndex == 1)
                {
                    break;
                }
                mCurrentIndex = 1;
                Equip(mLoadout.GetItem(mCurrentIndex));
                break;
            case '3':
                if (mCurrentIndex == 2)
                {
                    break;
                }
                mCurrentIndex = 2;
                Equip(mLoadout.GetItem(mCurrentIndex));
                break;
            case '4':
                if (mCurrentIndex == 3)
                {
                    break;
                }
                mCurrentIndex = 3;
                Equip(mLoadout.GetItem(mCurrentIndex));
                break;
            case '5':
                if (mCurrentIndex == 4)
                {
                    break;
                }
                mCurrentIndex = 4;
                Equip(mLoadout.GetItem(mCurrentIndex));
                break;
            default:
                break;
        }
    }


}