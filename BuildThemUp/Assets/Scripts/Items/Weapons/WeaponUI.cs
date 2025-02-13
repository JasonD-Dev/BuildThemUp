using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Weapon))]
public class WeaponUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mCoolingPercentageText;
    [SerializeField] Image mCoolingBar;
    Weapon mWeapon;

    void Awake()
    {
        mWeapon = GetComponent<Weapon>();
        if (mWeapon == null)
        {
            Log.Error(this, "The gameobject does not have a Weapon class attached! Destorying the Weapon UI component.");
            Destroy(this);
        }

        if (mCoolingBar != null)
        {
            if (mCoolingBar.type != Image.Type.Filled)
            {
                Log.Warning(this, "Cannot animate the Cool bar as it must of Image type - Filled.");
                mCoolingBar = null;
            }
        }
    }
    
    void FixedUpdate()
    {
        float tHeatCapacityRemaining = 1 - mWeapon.heatRatio;

        if (mCoolingBar != null)
        {
            mCoolingBar.fillAmount = tHeatCapacityRemaining;
        }

        if (mCoolingPercentageText != null)
        {
            mCoolingPercentageText.text = Mathf.Round(tHeatCapacityRemaining * 100) + "%";
        }
    }
}
