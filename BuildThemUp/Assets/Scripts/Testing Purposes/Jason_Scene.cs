using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject targetObject;
    public GameObject EnemyObject;
    public KeyCode keyToDamageSelf = KeyCode.E;
    public KeyCode keyToHeal = KeyCode.R;
    public KeyCode keyToDamageEnemy = KeyCode.T;

    private void Update()
    {
        if (Input.GetKeyDown(keyToDamageSelf))
        {
            Damage();
        }
        else if (Input.GetKeyDown(keyToHeal))
        {
            Heal();
        }
        else if (Input.GetKeyDown(keyToDamageEnemy))
        {
            Attack();
        }
    }

    private void Damage()
    {
        if (targetObject != null)
        {
            PlayerHealth targetScript = targetObject.GetComponent<PlayerHealth>();
            if (targetScript != null)
            {
                //targetScript.TakeDamage(10); 
            }
            else
            {
                Log.Warning(this, "Target object does not have the required script attached.");
            }
        }
        else
        {
            Log.Warning(this, "Target object is not assigned.");
        }
    }

    private void Heal()
    {
        if (targetObject != null)
        {
            PlayerHealth targetScript = targetObject.GetComponent<PlayerHealth>();
            if (targetScript != null)
            {
                targetScript.Heal(10); 
            }
            else
            {
                Log.Warning(this, "Target object does not have the required script attached.");
            }
        }
        else
        {
            Log.Warning(this, "Target object is not assigned.");
        }
    }

    private void Attack()
    {
        if (targetObject != null)
        {
            EnemyHealth enemyScript = EnemyObject.GetComponent<EnemyHealth>();
            if (enemyScript != null)
            {
                //enemyScript.TakeDamage(10);
            }
            else
            {
                Log.Warning(this, "Target object does not have the required script attached.");
            }
        }
        else
        {
            Log.Warning(this, "Target object is not assigned.");
        }
    }
}
