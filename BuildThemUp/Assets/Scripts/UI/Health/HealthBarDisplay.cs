
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class HealthBarDisplay : MonoBehaviour
{
    Health mHealth;

    [Header("UI Components")]
    [SerializeField] Image mFrontHealthbar;
    [SerializeField] Image mBackHealthbar;

    [Header("Visuals")]
    [SerializeField, Range(0.1f, 3f)] float mAnimationTime = 1;
    [SerializeField, Range(0.1f, 3f)] float mIdleDisplayDuration = 1f; //Duration after the animation is done.
    [SerializeField] bool mIsDisplayPersistent = false;
    
    private bool mIsDisplayingHealth = false;
    private float mIdleDisplayTimer = 0;
    private float mAnimationTimer = 0;

    float mInitialFrontAlpha;
    float mInitialBackAlpha;

    void Awake()
    {
        mHealth = GetComponent<Health>();
        if (mHealth == null )
        {
            Log.Error(this, "Health componenet was not found! Destroying...");
            Destroy(this); 
            return;
        }

        if (mFrontHealthbar == null )
        {
            Log.Error(this, "The front bar is not assigned! Destorying...");
            Destroy(this);
            return;
        }

        if (mBackHealthbar == null)
        {
            Log.Error(this, "The back bar is not assigned! Destorying...");
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        mInitialFrontAlpha = mFrontHealthbar.color.a;
        mInitialBackAlpha = mBackHealthbar.color.a;

        ShowHealth(mIsDisplayPersistent);

        mHealth.onDamageTaken.AddListener(OnDamageTaken);
    }

    void FixedUpdate()
    {
        if (mIsDisplayPersistent == true)
        {
            PersistentLogic();
        }
        else
        {
            NonPersistentLogic();
        }
    }

    void PersistentLogic()
    {
        if (Time.time - mHealth.lastHealthChangeTimeStamp < mAnimationTime)
        {
            UpdateUI();
        }
    }

    void NonPersistentLogic()
    {
        if (Time.time - mHealth.lastHealthChangeTimeStamp < mAnimationTime)
        {
            UpdateUI();

            if (mIsDisplayingHealth == false)
            {
                ShowHealth(true);
            }
        }
        else if (mIsDisplayingHealth == true)
        {
            mIdleDisplayTimer += Time.deltaTime;

            if (mIdleDisplayTimer > mIdleDisplayDuration)
            {
                ShowHealth(false);
                return;
            }

            float tColorAlpha = 1 - (mIdleDisplayTimer / mIdleDisplayDuration);
            SetImageAlpha(mFrontHealthbar, tColorAlpha);
            SetImageAlpha(mBackHealthbar, tColorAlpha);
        }
    }

    // Added to to Health's ontakedamage UnityEvent.
    // Everytime health takes damage, this function should be called.
    void OnDamageTaken(float aAmount, Transform aAttackerTransform)
    {
        mAnimationTimer = 0;
    }

    void UpdateUI()
    {
        float tNormalisedHealth = mHealth.NormalisedHealth();
        float tFrontBarFillAmount = mFrontHealthbar.fillAmount; //immediately changes to display the current health. 
        float tBackBarFillAmount = mBackHealthbar.fillAmount; //slowly changes to animate the change in health.

        // This means player has taken damage.
        if (tBackBarFillAmount > tNormalisedHealth)
        {
            mBackHealthbar.color = Color.red;
            mFrontHealthbar.fillAmount = tNormalisedHealth;

            mAnimationTimer += Time.deltaTime;
            float t = Mathf.Pow(mAnimationTimer / mAnimationTime, 3); //Cubic rate of change.

            // Delay the Back healthbar
            mBackHealthbar.fillAmount = Mathf.Lerp(tBackBarFillAmount, tNormalisedHealth, t);
            return;
        }

        // This means player has recieved healing.
        if (tFrontBarFillAmount < tNormalisedHealth)
        {
            mBackHealthbar.color = Color.green;
            mBackHealthbar.fillAmount = tNormalisedHealth;

            mAnimationTimer += Time.deltaTime;
            float t = Mathf.Pow(mAnimationTimer / mAnimationTime, 3); //Cubic rate of change.

            // Delay the Back healthbar
            mFrontHealthbar.fillAmount = Mathf.Lerp(tFrontBarFillAmount, mBackHealthbar.fillAmount, t);
            return;
        }
    }

    void ShowHealth(bool aBool)
    {
        mFrontHealthbar.gameObject.SetActive(aBool);
        mBackHealthbar.gameObject.SetActive(aBool);

        if (aBool ==  true)
        {
            SetImageAlpha(mFrontHealthbar, mInitialFrontAlpha);
            SetImageAlpha(mBackHealthbar, mInitialBackAlpha);
        }

        mIsDisplayingHealth = aBool;
        mIdleDisplayTimer = 0;
    }

    void SetImageAlpha(Image aImage, float aAlpha)
    {
        Color tColor = aImage.color;
        tColor.a = aAlpha;
        aImage.color = tColor;
    }
}
