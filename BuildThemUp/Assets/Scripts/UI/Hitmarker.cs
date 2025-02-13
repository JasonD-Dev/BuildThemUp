using UnityEngine;

public class Hitmarker : MonoBehaviour
{
    public static Hitmarker instance { get; private set; }

    [Header("Hitmarker Components")]
    [SerializeField] ImageFillAnimation[] mHitmarkers;
    [SerializeField] AudioSource mHitMarkerAudioSource;
    float mDefaultAudioSourcePitch = 1; //Assigned in Start();

    [Header("Parameters")]
    [SerializeField, Range(0f, 1f)] float mSmallestSizeRatio; //Hitmarkers are of random size, clamp the smallest size.

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Log.Error(this, $"There were multiple instances of {typeof(Hitmarker)} found. Destroying this one!");
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        if (mHitMarkerAudioSource == null)
        {
            Log.Warning(this, "The hitmarker audio source is not assigned!");
        }
    }

    private void Start()
    {
        mDefaultAudioSourcePitch = mHitMarkerAudioSource.pitch;
    }

    void FixedUpdate()
    {
        foreach (ImageFillAnimation tHitmarkerFillAnimation in mHitmarkers)
        {
            if (tHitmarkerFillAnimation.isFinished == true)
            {
                tHitmarkerFillAnimation.gameObject.SetActive(false);
            }
        }
    }

    public void RegisterHit()
    {
        float tRandomSizeRation = Random.Range(mSmallestSizeRatio, 1);

        foreach (ImageFillAnimation tHitmarkerFillAnimation in mHitmarkers)
        {
            tHitmarkerFillAnimation.gameObject.SetActive(true);
            tHitmarkerFillAnimation.Animate(tRandomSizeRation);
        }

        if (mHitMarkerAudioSource != null)
        {
            mHitMarkerAudioSource.Play();
        }
    }
}