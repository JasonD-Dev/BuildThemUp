using System;
using Unity.VisualScripting;
using UnityEngine;

public class WorldParticle : MonoBehaviour
{
    // DON'T TOUCH! This is only set on the original instance by the ParticleManager!
    public int PoolingId => mPoolingId;
    private int mPoolingId;
    
    [SerializeField] private ParticleSystem mParticleSystem;
    
    private float mLifeTimer;
    private bool mImmortal;

    public void Initialise(Vector3 aPosition, Vector3 aRotation)
    {
        var tTransform = transform;
        tTransform.position = aPosition;
        tTransform.eulerAngles = aRotation;

        gameObject.SetActive(true);
        mParticleSystem.Play();
    }

    public void OnParticleSystemStopped()
    {
        ParticleManager.Instance.AddToPool(this);
    }

    public void OnPool()
    {
        gameObject.SetActive(false);
    }

    // This should only be called by the particle manager
    public void SetPoolingId(int aId)
    {
        mPoolingId = aId;
    }
}