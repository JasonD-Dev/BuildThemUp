using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
///     This is a parent class for projectiles.
///     You shouldn't use this as a projectile
///     as it doesn't contain any collision or real update logic.
/// </summary>
public abstract class Projectile : MonoBehaviour
{
    // DON'T TOUCH! This is only set on the original instance by the ProjectileManager!
    public int PoolingId => mPoolingId;
    private int mPoolingId;
    
    protected bool mIsFriendly;
    protected float mLifeTimer;

    public Vector3 MuzzleOrigin;
    public Vector3 Origin;
    public Vector3 Direction;
    public float Speed;
    public bool ShowHitmarkerOnHit;

    public float Lifetime;
    
    protected float mDamage;
    
    // why are we storing it here instead of just accessing GameObject.transform?
    // le optimisation (about 5% performance increase)
    protected Transform mTransform;

    public virtual void Start()
    {
        mTransform = transform;
    }

    
    public virtual void Initialise(bool aIsFriendly, Vector3 aOrigin, Vector3 aDirection, float aSpeed, float aDamage, bool aShowHitmarkerOnHit)
    {
        mIsFriendly = aIsFriendly;

        transform.position = aOrigin;
        transform.forward = aDirection;

        Origin = aOrigin;
        Direction = aDirection;
        Speed = aSpeed;
        ShowHitmarkerOnHit = aShowHitmarkerOnHit;
        
        mLifeTimer = Lifetime;
        mDamage = aDamage;

        gameObject.SetActive(true);
    }

    public virtual void Update()
    {
        mLifeTimer -= Time.deltaTime;
        
        // time's up
        if (mLifeTimer <= 0)
            ProjectileManager.Instance.AddToPool(this);
        //    Destroy(gameObject);
    }
    
    public virtual void OnPool()
    {
        gameObject.SetActive(false);
    }

    public void SetPoolingId(int aId)
    {
        mPoolingId = aId;
    }
}