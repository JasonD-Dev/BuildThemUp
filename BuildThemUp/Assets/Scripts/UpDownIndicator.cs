using UnityEngine;

public class UpDownIndicator : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.5f; // Height of the up-down movement
    [SerializeField] private float frequency = 1f;   // Speed of the up-down movement

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
