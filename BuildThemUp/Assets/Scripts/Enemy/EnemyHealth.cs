
using System;
using UnityEngine;
using UnityEngine.VFX;

[Serializable]
struct ScrapDrop
{
    public GameObject scrapPrefab;
    [Range(0f, 1f)]
    public float chanceOfDrop;
}

public class EnemyHealth : Health
{
    public override bool isFriendly => false;

    //Kill Reward Scrap
    [Header("Drop")]
    [SerializeField] private ScrapDrop[] mScrapDrops;

    [Header("Death Effects")]
    [SerializeField] VisualEffect mDeathVisualEffect;
    [SerializeField] CameraShake mDeathCameraShake = new CameraShake();

    private EnemyAnimatorController mEnemyAnimatorController;

    public override void Die()
    {

        base.Die();
        
        // Animator
        //mEnemyAnimatorController = GetComponent<EnemyAnimatorController>();
        //float DeathTimer = 0.01f;
        //if (mEnemyAnimatorController.isActiveAndEnabled == true || mEnemyAnimatorController == null)
        //{
        //    DeathTimer = mEnemyAnimatorController.PlayDieAnimation();
        //}

        foreach (ScrapDrop tScrapDrop in mScrapDrops)
        {
            if (UnityEngine.Random.Range(0, 1) <= tScrapDrop.chanceOfDrop)
            {
                Instantiate(tScrapDrop.scrapPrefab, transform.position, Quaternion.identity);
                break;
            }
        }

        if (mDeathVisualEffect != null)
        {
            //So when the enemy get destoryed the effect still plays as a seperate gameobject.
            VisualEffect tDeathEffect = Instantiate(mDeathVisualEffect, gameObject.transform.position, Quaternion.identity);
            tDeathEffect.Play();

            mDeathCameraShake.AddShakeEvent(transform.position);

            Destroy(tDeathEffect.gameObject, 3);
        }

        Destroy(gameObject);
    }
}
