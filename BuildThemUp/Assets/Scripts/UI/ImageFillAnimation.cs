using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageFillAnimation : MonoBehaviour
{
    [field: SerializeField] public bool isFinished { get; private set; } = false;

    [Header("Fill Settings")]
    [Range(0f, 1.5f), SerializeField] float mFillTime = 0.5f;

    public float fillTime => mFillTime;

    static Image.FillMethod mDefaultFillMethod => Image.FillMethod.Horizontal;

    private Image mImage;
    private float mCurrentFillTime = 0;
    private float mTargetFillAmount = 1;
    [Range(0, 4), SerializeField] int mRateOfChangePower = 1;

    private void Awake()
    {
        mImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (SanityCheck() == false)
        {
            return;
        }

        if (mImage.type != Image.Type.Filled)
        {
            Log.Warning(this, $"The image type must be {Image.Type.Filled} for the animation to work! Changing image type to this with a Horizontal fill method.");
            mImage.type = Image.Type.Filled;
            mImage.fillMethod = mDefaultFillMethod;
        }
    }

    private void OnEnable()
    {
        ResetAnimation();
    }

    private void FixedUpdate()
    {
        if (isFinished == true) 
        {
            return; 
        }

        mCurrentFillTime += Time.deltaTime;
        mImage.fillAmount = Mathf.Pow(1 / mFillTime * mCurrentFillTime, mRateOfChangePower) * mTargetFillAmount;

        if (mCurrentFillTime > mFillTime)
        {
            isFinished = true;
        }
    }

    private void ResetAnimation()
    {
        if (SanityCheck() == true)
        {
            mCurrentFillTime = 0;
            isFinished = false;
            mImage.fillAmount = 0;
        }
    }

    private bool SanityCheck()
    {
        if (mImage == null)
        {
            Log.Error(this, $"Could not find an {typeof(Image)} component on the gameobject! Destroying the animator!");
            Destroy(this);
            return false;
        }

        if (mImage.sprite == null)
        {
            Log.Warning(this, $"There is not sprite provided to the {typeof(Sprite)} component to animate!");
            return false;
        }

        return true;
    }

    public void Animate(float aTargetFillAmount = 1)
    {
        if (gameObject.activeSelf == false)
        {
            Log.Error(this, "The gameobject is not active, thus the animation cannot be played!");
            return;
        }

        if (mTargetFillAmount != aTargetFillAmount)
        {
            SetTargetFillAmount(aTargetFillAmount);
        }
        
        ResetAnimation();
    }

    public void SetTargetFillAmount(float aAmount)
    {
        if (aAmount < 0 || aAmount > 1)
        {
            Log.Warning(this, "The fill amount given cannot be less than 0 or greater than 1! Clamping...");
            aAmount = Mathf.Clamp(aAmount, 0, 1);
        }

        if (aAmount == 0)
        {
            Log.Warning(this, "The fill amount is being set to 0. The image will not be visible!");
        }

        mTargetFillAmount = aAmount;
    }

    public void SetFillTime(float aFillTime)
    {
        if (aFillTime < 0)
        {
            Log.Error(this, "Fill time cannot be set below 0.");
            return;
        }

        mFillTime = aFillTime;
    }
}
