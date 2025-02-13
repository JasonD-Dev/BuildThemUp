using UnityEngine;
using UnityEngine.Pool;

public class Decal : MonoBehaviour
{
    [SerializeField] private MeshRenderer mDecalRenderer;
    [SerializeField] private AudioSource mAudioSource;
    
    private float mLifeTimer;
    private bool mImmortal;

    public void Initialise(Vector3 aPosition, Vector3 aRotation, Vector3 aSize, Material aMaterial, float aLifetime = -1f)
    {
        var tTransform = transform;
        tTransform.position = aPosition;
        tTransform.eulerAngles = aRotation;
        tTransform.localScale = aSize;

        mDecalRenderer.sharedMaterial = aMaterial;

        mImmortal = aLifetime < 0;
        mLifeTimer = aLifetime;
    }

    public void PlaySound(AudioClip aClip, float aPitch, float aVolume = 1)
    {
        mAudioSource.pitch = aPitch;
        mAudioSource.PlayOneShot(aClip, aVolume);
    }

    public void Update()
    {
        if (!mImmortal)
        {
            mLifeTimer -= Time.deltaTime;
        
            if (mLifeTimer <= 0)
                gameObject.SetActive(false);
        }
    }
}