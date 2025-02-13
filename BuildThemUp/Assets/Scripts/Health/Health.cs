using UnityEngine;
using UnityEngine.Events;

public abstract class Health : MonoBehaviour
{
    public abstract bool isFriendly { get; }

    [Header("Health Stats")]
    [field: SerializeField] public float health { get; private set; }
    [SerializeField] protected float mMaxHealth;

    public float maxHealth => mMaxHealth;

    public float lastHealthChangeTimeStamp { get; private set; } = 0;
    public UnityEvent<float, Transform> onDamageTaken;
    public bool isDead { get; set; }
    

    protected virtual void Start()
    {
        health = mMaxHealth;
    }

    public virtual bool TakeDamage(float aAmount, Transform aAttackerTransform)
    {
        if (aAmount <= 0)
        {
            Log.Error(this,$"![{name}, Health] Cannot accept a damage value for TakeDamage() to be a negative number.");
            return false;
        }

        if (isDead)
            return false;

        lastHealthChangeTimeStamp = Time.time;
        health -= aAmount;

        if (health <= 0)
        {
            health = 0;
            Die();
        }

        onDamageTaken.Invoke(aAmount, aAttackerTransform);
        return true;
    }

    public void Heal(float aHealAmount)
    {
        health += aHealAmount;
        if (health > mMaxHealth)
        {
            health = mMaxHealth;
        }

        lastHealthChangeTimeStamp = Time.time;
    }

    public void IncreaseMaxHealth(float aAmount)
    {
        float tNewHealth = mMaxHealth + aAmount;
        if (tNewHealth < 1)
        {
            Log.Error(this, "The new max health has to be greater than 0.");
            return;
        }

        mMaxHealth = tNewHealth;
    }

    public float NormalisedHealth()
    {
        return health / mMaxHealth;
    }

    public virtual void Die()
    {
        isDead = true;
    }
}
