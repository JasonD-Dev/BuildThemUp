using UnityEngine;
using Random = UnityEngine.Random;

public class HitscanBullet : Projectile
{
    [Tooltip("How fast the bullet trail will travel, in units per second")]
    public float BulletTrailSpeed = 900;

    public TrailRenderer TrailRenderer;

    // these are used to lerp the bullet to the target
    // to avoid trail clipping through the target
    private bool mHitSomething;
    private Vector3 mHitPoint;

    [SerializeField] private AudioClip[] mImpactSounds;

    public override void Start()
    {
        base.Start();
    }
    
    //TODO "Speed" can be used for trail speed in hitscan.
    public override void Initialise(bool aIsFriendly, Vector3 aOrigin, Vector3 aDirection, float aSpeed, float aDamage, bool aShowHitmarkerOnHit)
    {
        base.Initialise(aIsFriendly, aOrigin, aDirection, aSpeed, aDamage, aShowHitmarkerOnHit);
        
        Hitscan();
    }

    public override void Update()
    {
        mLifeTimer -= Time.deltaTime;
        
        // this prevents the trail from phasing through when we've hit something
        if (!mHitSomething && mLifeTimer > 0)
            mTransform.position += transform.forward * (BulletTrailSpeed * Time.deltaTime);
        
        // wait until TrailRenderer has finished before adding it to the pool
        if (mLifeTimer <= -TrailRenderer.time)
            ProjectileManager.Instance.AddToPool(this);
    }

    public void Hitscan()
    {
        // if doing bullet penetration, you would need to use an array-based raycast instead.
        // not in scope so just using basic raycast
        var tShootRay = new Ray(Origin, Direction);

        // TODO: layer mask for collision layers
        // returns closest raycast hit
        mHitSomething = Physics.Raycast(tShootRay, out var hit);
        if (mHitSomething)
        {
            mHitPoint = hit.point;
            mTransform.position = mHitPoint;
            
            // kill trail early, as we collided with something (don't want it to phase thru)
            mLifeTimer = Mathf.Min(mLifeTimer, hit.distance / BulletTrailSpeed);
            
            var aImpactPos = mHitPoint + (hit.normal * 0.001f);
            var aImpactRot = Quaternion.LookRotation(hit.normal).eulerAngles;
            aImpactRot.z = Random.Range(0, 90f);

            var aScale = Random.Range(0.05f, 0.15f);
            var aImpactSize = new Vector3(aScale, aScale, aScale);

            // check if what we just hit has health and act accordingly
            if (hit.transform.gameObject.TryGetComponent<Health>(out var health))
            {
                // check if the thing we just hit should feel the pain
                if (mIsFriendly != health.isFriendly)
                {
                    //health.TakeDamage(mDamage);
                    ParticleManager.Instance.GetFromPool(aImpactPos, aImpactRot, "EnemyBlood");
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
                var tVol = Random.Range(0.95f, 1.05f);
                
                tDecal.PlaySound(mImpactSounds[tIdx], tPitch, tVol);
                
                ParticleManager.Instance.GetFromPool(aImpactPos, aImpactRot, "BulletSmoke");
            }
        }
    }
}