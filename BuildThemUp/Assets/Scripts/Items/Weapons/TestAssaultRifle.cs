using Random = UnityEngine.Random;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TestAssaultRifle : Weapon
{
    public Projectile Shot;
    [SerializeField] private float Spread = 0.04f;

    public override bool Shoot()
    {
        if (base.Shoot() == false)
        {
            return false;
        }
        float xOffset = 0.01f;
        float yOffset = 0.01f;
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
}
