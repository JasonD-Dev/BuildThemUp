using UnityEngine;

public class ShootingStarBehaviour : MonoBehaviour
{
    [SerializeField, Range(0f, 50f)] float mSpeed = 10.0f;
    [SerializeField, Range(0f, 15f)] float mLifeTIme = 2.0f;

    void Start()
    {
        // Apply a random direction
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        randomDirection.y = Mathf.Clamp01(randomDirection.y);
        transform.rotation = Quaternion.LookRotation(randomDirection);
        transform.localScale *= Random.Range(0.6f, 1.4f);

        mSpeed *= Random.Range(0.5f, 2f);

        // Destroy the shooting star after the specified lifetime
        Destroy(gameObject, mLifeTIme);
    }

    void FixedUpdate()
    {
        // Move the shooting star forward
        transform.Translate(Vector3.forward * mSpeed * Time.deltaTime);
    }
}
