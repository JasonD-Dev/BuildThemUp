using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypeWriterEffect : MonoBehaviour
{
    [field: SerializeField] public bool isFinished { get; private set; } = false;

    [Header ("Text Settings")]
    [Range(0, 0.5f), SerializeField] float mDelayBetweenCharacters = 0.1f;

    [Header("Audio Settings")]
    [SerializeField] AudioClip mAudioClip;
    [Range(0, 1), SerializeField] float mAudioVolume = 1;
    [Range(0, 3), SerializeField] int mAudioCharacterInterval = 1;

    public TextMeshProUGUI textMesh { get; private set; }
    AudioSource mAudioSource;

    float mCurrentDelayTimer = 0;
    int mCurrentAudioCharacterInterval;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();

        if (textMesh == null )
        {
            Log.Error(this, "Couldn't find a text component.");
        }

        if (mAudioClip != null)
        {
            mAudioSource = gameObject.AddComponent<AudioSource>();
            mAudioSource.resource = mAudioClip;
            mAudioSource.playOnAwake = false;
        }
        else
        {
            Log.Info(this, "An audio clip was not provided. No audio source was created.");
        }
    }

    void Start()
    {
        SanityCheck();
    }

    private void OnEnable()
    {
        SanityCheck();
        Animate();
    }

    void FixedUpdate()
    {
        if (isFinished == true)
        {
            return;
        }

        mCurrentDelayTimer += Time.deltaTime;
        if (mCurrentDelayTimer >= mDelayBetweenCharacters)
        {
            IncrementCharacterLimit();
            PlayAudio();
        }

        if (textMesh.maxVisibleCharacters >= textMesh.text.Length) 
        { 
            isFinished = true;
        }
    }

    private void IncrementCharacterLimit()
    {
        textMesh.maxVisibleCharacters += 1;
        mCurrentDelayTimer = 0;  
    }

    private void PlayAudio()
    {
        if (mAudioSource == null)
        {
            return;
        }

        mCurrentAudioCharacterInterval++;
        if (mCurrentAudioCharacterInterval >= mAudioCharacterInterval)
        {
            mCurrentAudioCharacterInterval = 0;
            mAudioSource.Play();
        }
    }

    public void Animate()
    {
        isFinished = false;
        textMesh.maxVisibleCharacters = 0;

        if (mAudioSource != null)
        {
            mCurrentAudioCharacterInterval = mAudioCharacterInterval;
            mAudioSource.volume = mAudioVolume; //Update every time its played incase we want the volume to change.
        }
    }

    private bool SanityCheck()
    {
        if (textMesh == null)
        {
            Log.Error(this, $"There is no {typeof(TextMeshProUGUI)} attached to this gameobject! Destroying this {typeof(TypeWriterEffect)} component!");
            Destroy(this);
            return false;
        }

        return true;
    }

    public void SetText(string aText, bool aReanimate = true)
    {
        Log.Info(this, textMesh.ToString());
        textMesh.text = aText;

        if (aReanimate == true)
        {
            Animate();
        }
    }
}
