using Random = UnityEngine.Random;
using UnityEngine;

public class TestGrenadeLauncher : Weapon
{
    [Header("Grenade stats")]
    [SerializeField] private GameObject mGrenadePrefab;
    [SerializeField] public float mBlastRadius = 10f;
    [SerializeField] private float mLaunchForce = 20f;
    [SerializeField] private float mSpinForce = 50f;

    public override bool Shoot()
    {
        if (base.Shoot() == false)
        {
            return false;
        }

        if (mGrenadePrefab == null)
        {
            Log.Error(this, "The Grenade instance shot was null!");
            return false;
        }

        SpawnGrenade();
        return true;
    }

    private void SpawnGrenade()
    {
        GameObject grenade = Instantiate(mGrenadePrefab, mMuzzleTransform.position, mMuzzleTransform.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce((mMuzzleTransform.forward) * mLaunchForce, ForceMode.Impulse);
        Vector3 randomTorque = new Vector3(Random.Range(-mSpinForce, mSpinForce), Random.Range(-mSpinForce, mSpinForce), Random.Range(-mSpinForce, mSpinForce));
        rb.AddTorque(randomTorque);


    }
}
