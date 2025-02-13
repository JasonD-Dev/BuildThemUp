using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestSniper : Weapon
{

    public Projectile Shot;
    [SerializeField] private float Spread = 0.3f;

    public override bool Shoot()
    {
        if (base.Shoot() == false)
        {
            return false;
        }

        float xOffset = 0;
        float yOffset = 0;
        if (!aimDownSights.isADS)
        {
            xOffset = Random.Range(-Spread, Spread);
            yOffset = Random.Range(-Spread, Spread);

        }

        var tVectors = GetOriginDirection(xOffset, yOffset);
        float tDamage = Random.Range(damage * (1 - mDamageVariance), damage * (1 + mDamageVariance));
        Projectile tProjectile = ProjectileManager.Instance.GetFromPool(Shot, true, tVectors.Item1, tVectors.Item2, bulletSpeed, tDamage, true);
        return (tProjectile != null);
    }



    //public override bool Shoot()
    //{
    //    if (base.Shoot() == false)
    //    {
    //        return false;
    //    }
    //    float xOffset = 0;
    //    float yOffset = 0;
    //    if (!mADS)
    //    {

    //         xOffset = Random.Range(-Spread, Spread);
    //         yOffset = Random.Range(-Spread, Spread);

    //    }

    //    var tVectors = GetOriginDirection(xOffset, yOffset);
    //    float tDamage = Random.Range(damage * (1 - mDamageVariance), damage * (1 + mDamageVariance));
    //    Projectile tProjectile = ProjectileManager.Instance.GetFromPool(Shot, true, tVectors.Item1, tVectors.Item2, bulletSpeed, tDamage, true);
    //    return (tProjectile != null);


}
