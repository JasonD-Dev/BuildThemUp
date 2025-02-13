using UnityEngine;

public class BossEnemy : Enemy
{
    protected override Transform GetClosestTarget()
    {
        float tDistanceToMainBase = Vector3.Distance(transform.position, mMainBaseTransform.position);
        Transform[] tMainBaseTargets = EnemiesManager.instance.mainBaseTargets;
        Transform tClosestTarget = tMainBaseTargets[0];
        float tClosestTargetDistance = tDistanceToMainBase;

        foreach (Transform tMainBaseTarget in tMainBaseTargets)
        {
            float tCurretDistance = Vector3.Distance(transform.position, tMainBaseTarget.position);
            if (tCurretDistance < tClosestTargetDistance)
            {
                tClosestTargetDistance = tCurretDistance;
                tClosestTarget = tMainBaseTarget;
            }
        }
        return tClosestTarget;
    }

    protected override void Attack(Health tHealth)
    {
        if (tHealth is MainBaseHealth mMainBaseHP)
        {
            base.Attack(tHealth);
        }
    }
}
