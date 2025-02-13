using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private float mBlastRadius;
    [SerializeField] private float mDamage;
    [SerializeField] private AudioSource mAudioSource;
    [SerializeField] private AudioClip mExplosionAudio;
    [SerializeField] private float mActivationDelay = 0f; 
    private float mElapsedTime = 0;
    private Collider mGrenadeCollider;
    private bool mIsCollisionEnabled;

    private Collider[] mHitColliders;

    private void Start()
    {
        mBlastRadius = FindAnyObjectByType<TestGrenadeLauncher>().mBlastRadius;
        mDamage = FindAnyObjectByType<TestGrenadeLauncher>().damage;
        mGrenadeCollider = GetComponent<Collider>();
        if (mGrenadeCollider != null)
        {
            mGrenadeCollider.enabled = false;
        }

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            mAudioSource = player.GetComponent<AudioSource>(); // Get AudioSource from the player
            mAudioSource.clip = mExplosionAudio;
        }

        if (mAudioSource == null)
        {
            Debug.LogWarning("AudioSource component not found on Player!");
        }
    }

    private void Update()
    {
        if (!mIsCollisionEnabled)
        {
            mElapsedTime += Time.deltaTime;

            if (mElapsedTime >= mActivationDelay)
            {
                EnableCollision();
                mIsCollisionEnabled = true;
            }
        }
    }

    private void EnableCollision()
    {
        if (mGrenadeCollider != null)
        {
            mGrenadeCollider.enabled = true;
        }
    }

    private void OnCollisionEnter(Collision aCollision)
    {
        //Debug.Log("Grenade collided with: " + aCollision.gameObject.name);
          
       
        if (mGrenadeCollider != null && mGrenadeCollider.enabled)
        {
            DestroyGrenade(0);
            GrenadeExplosion(aCollision.contacts[0].point);
        }
    }

    private HashSet<Health> hitEnemies = new HashSet<Health>();

    private void GrenadeExplosion(Vector3 aExplosionPoint)
    {
        mHitColliders = Physics.OverlapSphere(aExplosionPoint, mBlastRadius);

        foreach (Collider collider in mHitColliders)
        {
            Transform vObjectTransform = collider.gameObject.GetComponent<Transform>();
            DynamicHitbox tObjectHitbox = collider.gameObject.GetComponent<DynamicHitbox>();
            if (tObjectHitbox != null)
            {
                // bypass part damage multiplier
                var tHealth = tObjectHitbox.parent.health;

                // we already hit this enemy, this is just another one of their hitboxes
                if (hitEnemies.Contains(tHealth))
                    continue;
                
                tHealth.TakeDamage(mDamage, vObjectTransform);
                hitEnemies.Add(tHealth);
            }
        }

    }

    private void DestroyGrenade(float aDestroytime)
    {
        Destroy(gameObject, aDestroytime); // Destroy the grenade GameObject

        if (mAudioSource == null)
        {
            Debug.Log("No audioSource for Grenade.");
        }
        else if (mExplosionAudio == null)
        {
            Debug.Log("No ExplosionAudio for Grenade.");
        }
        else
        {
            mAudioSource.Play();
        }
    }
}