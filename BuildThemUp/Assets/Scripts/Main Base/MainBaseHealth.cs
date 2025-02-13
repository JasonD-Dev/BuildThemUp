using UnityEngine;

public class MainBaseHealth : Health
{
    [SerializeField] private BaseDamageNotifier baseDamageNotifier;

    public override bool isFriendly => true;

    protected override void Start()
    {
        if (baseDamageNotifier == null)
        {
            Debug.LogError("BaseDamageNotifier reference is missing! Please assign it in the Inspector.");
        }
    }

    public override bool TakeDamage(float aAmount, Transform aAttackerTransform)
    {
        bool damageTaken = base.TakeDamage(aAmount, aAttackerTransform);

        if (damageTaken && baseDamageNotifier != null)
        {
            baseDamageNotifier.OnBaseUnderAttack();
        }

        return damageTaken;
    }

    public override void Die()
    {
        Log.Info(this, $"Main base has been destroyed. Game Over!");
        GamePlayManager.instance.SetGameOver();
    }

}
