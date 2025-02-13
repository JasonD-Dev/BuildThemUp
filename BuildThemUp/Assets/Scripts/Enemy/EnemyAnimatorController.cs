using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimatorController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator mAnimator;
    //private NavMeshAgent mAgent;
    //public bool mIsDead { get; private set; }
    private bool mIsAttacking;

    private void Awake()
    {
        if (mAnimator == null)
        {
            Log.Error(this, "Please Instantiate Animator on Enemy!");
            this.enabled = false;
        }
        //mAgent = GetComponent<NavMeshAgent>();
        mAnimator.applyRootMotion = false;
        //mIsDead = false;
    }

    public void PlayAttackAnimation()
    {
        //if (!mIsDead)
        //{
            if (!mIsAttacking)
            {
                int index = Random.Range(1, 2);
                mAnimator.SetTrigger("AttackTrigger" + index);
                mAnimator.SetBool("IsAttacking", true);
                mIsAttacking = true;
            }
        //}
    }

    //public float PlayDieAnimation()
    //{
    //    mIsDead = true;
    //    mAnimator.SetTrigger("DieTrigger");
    //    //mAgent.enabled = false;
    //    return mAnimator.GetCurrentAnimatorStateInfo(0).length;
    //}

    public void ResetAnimator()
    {
        //if (!mIsDead)
        //{
            if (mIsAttacking)
            {
                mAnimator.ResetTrigger("AttackTrigger1");
                mAnimator.ResetTrigger("AttackTrigger2");
                mAnimator.SetBool("IsAttacking", false);
                mIsAttacking = false;
            }
        //}
    }
}
