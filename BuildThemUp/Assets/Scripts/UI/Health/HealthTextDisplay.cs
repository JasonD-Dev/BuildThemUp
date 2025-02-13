using TMPro;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class HealthTextDisplay : MonoBehaviour
{
    Health mHealth;

    [Header("Text Component")]
    [SerializeField] TextMeshProUGUI mHealthText;

    [Header("Visuals")]
    [SerializeField] bool mIsDisplayPersistent = false;
    [SerializeField, Range(0.1f, 3f)] float mDisplayDuration = 1f;

    private bool mIsDisplayingHealth = false;

    void Awake()
    {
        mHealth = GetComponent<Health>();
        if (mHealth == null)
        {
            Log.Error(this, "Health componenet was not found! Destroying...");
            Destroy(this);
            return;
        }

        if (mHealthText == null)
        {
            Log.Error(this, "Health Text component was not assigned! Destroying...");
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        ShowHealth(mIsDisplayPersistent);
    }

    void FixedUpdate()
    {
        if (mIsDisplayPersistent == true)
        {
            UpdateUI();
            return;
        }

        //Only if display is not persistent.
        if (Time.time - mHealth.lastHealthChangeTimeStamp < mDisplayDuration)
        {
            UpdateUI();

            if (mIsDisplayingHealth == false)
            {
                ShowHealth(true);
            }
        }
        else if (mIsDisplayingHealth == true)
        {
            ShowHealth(false);
        }
    }

    void UpdateUI()
    {
        float tNormalisedHealth = mHealth.NormalisedHealth();
        mHealthText.text = $"{Mathf.Floor(tNormalisedHealth * 100)}%";
    }

    void ShowHealth(bool aBool)
    {
        mHealthText.gameObject.SetActive(aBool);
        mIsDisplayingHealth = aBool;
    }
}
