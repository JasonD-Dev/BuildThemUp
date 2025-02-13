using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TestShotgun : Weapon
{
    public Projectile Shot;

    public int ShotCount = 5;
    public float Spread; //This should also be moved into the Weapon script. All weapons have spread.

    public override bool Shoot()
    {
        if(base.Shoot() == false)
        {
            return false;
        }
                
        for (int i = 0; i < ShotCount; i++)
        {
            Spread = 0.2f;
            if (this.aimDownSights.isADS)
            {
                Spread = 0.1f;
            }
            var xOffset = Random.Range(-Spread, Spread);
            var yOffset = Random.Range(-Spread, Spread);
            
            
            var tVectors = GetOriginDirection(xOffset, yOffset);
            float tDamage = Random.Range(damage * (1 - mDamageVariance), damage * (1 + mDamageVariance));
            Projectile tProjectile = ProjectileManager.Instance.GetFromPool(Shot, true, tVectors.Item1, tVectors.Item2, bulletSpeed, tDamage, true);

            if (tProjectile == null)
            {
                Log.Error(this, "The projectile instance shot was null!");
                return false;
            }
        }

        return true;
    }
}