using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] bool mIgnoreY = true;
    private void FixedUpdate()
    {
        Vector3 tLookAtPosition = GamePlayManager.instance.playerPosition;

        if (mIgnoreY == true)
        {
            tLookAtPosition.y = transform.position.y;
        }

        transform.LookAt(tLookAtPosition);
    }
}
