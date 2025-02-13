using UnityEngine;

public class Bullet : Projectile
{
    // AirResistance * Lifetime should never be over Speed!! (unless you want frozen bullets i guess)
    [Tooltip("How much the bullet slows down per second")]
    public float deceleration = 10;

    [Tooltip("How much gravity in units per second is applied to the bullet")]
    public float gravityMult = 1;

    public TrailRenderer trailRenderer;

    [SerializeField] private AudioClip[] mImpactSounds;
    

    private float mRaycastInterval = 0.1f;
    private float mNextRaycastLifetime;
    private Vector3 mLastRaycastPosition;

    private bool mHitSomething;
    private bool mResting;
    private Vector3 mHitPoint;
    
    private float mDeathTime;
    private float mPoolTime;
    private float mGravity => GamePlayManager.instance.gravity * gravityMult;

    public override void Initialise(bool aIsFriendly, Vector3 aOrigin, Vector3 aDirection, float aSpeed, float aDamage, bool aShowHitmarkerOnHit)
    {
        base.Initialise(aIsFriendly, aOrigin, aDirection, aSpeed, aDamage, aShowHitmarkerOnHit);
        
        trailRenderer.Clear();
        mLastRaycastPosition = aOrigin;
        mNextRaycastLifetime = mLifeTimer;
        mPoolTime = -trailRenderer.time;
        ShowHitmarkerOnHit = aShowHitmarkerOnHit;
    }

    public override void Update()
    {
        if (!mHitSomething && mLifeTimer <= mNextRaycastLifetime)
        {
            var tLifeTime = Lifetime - mLifeTimer;
            var tPosition = CalculatePosition(tLifeTime);
            
            mNextRaycastLifetime = mLifeTimer - mRaycastInterval;

            // raycast from our current position, to the next raycast position
            var tFutureTime = tLifeTime + mRaycastInterval;
            var tFuturePosition = CalculatePosition(tFutureTime);
            var tFutureRay = new Ray(mLastRaycastPosition, tFuturePosition - mLastRaycastPosition);

            // Draw raycasts for debugging purposes (note: this absolutely obliterates performance)
            //Debug.DrawRay(tFutureRay.origin, tFutureRay.direction * Vector3.Distance(mLastRaycastPosition, tFuturePosition), Color.red, 1);
            
            mHitSomething = Physics.Raycast(tFutureRay, out var tHit,
                Vector3.Distance(mLastRaycastPosition, tFuturePosition));
            if (mHitSomething)
            {
                // approximate the speed at the time we hit
                var tFutureSpeed = Mathf.Abs(Speed - deceleration * tFutureTime);
                // get distance from our prediction point to the raycast hit
                var tHitDistance = Vector3.Distance(tPosition, tHit.point);
                // approximate the time the projectile dies given the position
                mDeathTime = mLifeTimer - tHitDistance / tFutureSpeed;
                // add some extra time before pooling to allow the trail renderer to finish
                mPoolTime = mDeathTime - trailRenderer.time;
                
                OnHit(tHit);
            }

            mLastRaycastPosition = tPosition;
        }
        
        mLifeTimer -= Time.deltaTime;

        // we don't want to invoke the clamped movement when LifeTimer lapses
        // well we can (but we would need to reset mDeathTime on pooling)
        if (!mHitSomething)
        {
            if (mLifeTimer > 0)
                mTransform.position = CalculatePosition(Lifetime - mLifeTimer);
        }
        else if (!mResting)
        {
            // to avoid just spamming the same position constantly, check if we're
            // already at our final resting position
            if (mLifeTimer <= mDeathTime)
                mResting = true;
            
            var tClampedTime = Mathf.Min(Lifetime - mDeathTime, Lifetime - mLifeTimer);
            mTransform.position = CalculatePosition(tClampedTime);
        }
        
        // wait until TrailRenderer has finished before adding it to the pool
        if (mLifeTimer <= mPoolTime)
            ProjectileManager.Instance.AddToPool(this);
    }

    public Vector3 CalculatePosition(float aTime)
    {
        // calculate the distance over time (subtracting deceleration over time)
        var tDelta = (Speed * aTime) - (0.5f * deceleration * aTime);
        var tOffset = Direction * tDelta;
        
        // gravity, negative is always down so dont need movetowards
        tOffset.y -= CalculateBulletDrop(aTime);

        // apply our calculated offset to the origin
        return Origin + tOffset;
    }

    public float CalculateBulletDrop(float aDuration)
    {
        return 0.5f * mGravity * (aDuration * aDuration);
    }

    public void OnHit(RaycastHit aHit)
    {
        mHitPoint = aHit.point;
            
        // get the impact position for decal / particle
        var aImpactPos = mHitPoint + (aHit.normal * 0.001f);
        var aImpactRot = Quaternion.LookRotation(aHit.normal).eulerAngles;
        aImpactRot.z = Random.Range(0, 90f);

        // random scaling of the decal
        var aScale = Random.Range(0.05f, 0.15f);
        var aImpactSize = new Vector3(aScale, aScale, aScale);

        // check if what we just hit has health and act accordingly
        if (aHit.transform.gameObject.TryGetComponent<DynamicHitbox>(out var hitbox))
        {
            // check if the thing we just hit should feel the pain
            if (mIsFriendly != hitbox.parent.health.isFriendly)
            {
                ParticleManager.Instance.GetFromPool(aImpactPos, aImpactRot, "EnemyBlood");

                bool tIsHitRegistered = hitbox.TakeDamage(mDamage, PlayerController.instance.transform);
                if (tIsHitRegistered == true && ShowHitmarkerOnHit == true)
                    Hitmarker.instance.RegisterHit();
            }
                
            // TODO: proper implementation of parented decals
            //var decal = DecalManager.Instance.GetDecal(aImpactPos, aImpactRot, aImpactSize, "BulletHole");
            //decal.transform.SetParent(entity.transform, true);
        }
        else
        {
            // do bullet hole
            var tDecal = DecalManager.Instance.GetDecal(aImpactPos, aImpactRot, aImpactSize, "BulletHole");
                
            var tIdx = Random.Range(0, mImpactSounds.Length);
            var tPitch = Random.Range(0.95f, 1.05f);
            var tVol = Random.Range(0.5f, 0.6f);
                
            tDecal.PlaySound(mImpactSounds[tIdx], tPitch, tVol);
                
            ParticleManager.Instance.GetFromPool(aImpactPos, aImpactRot, "BulletSmoke");
        }
    }

    public override void OnPool()
    {
        mHitSomething = false;
        mResting = false;
        base.OnPool();
    }
}