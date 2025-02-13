using UnityEngine;

public class BaseDamageNotifier : MonoBehaviour
{
    public GameObject warningUI;  // Drag your UI element here in the Inspector
    public float displayTime = 1f;  // How long the UI element will be displayed
    private float timer;

    void Awake()
    {
        warningUI.SetActive(false);  // Ensure the UI element is hidden at start
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                warningUI.SetActive(false);  // Hide the UI element after the time has passed
            }
        }
    }

    // Call this method when the base takes damage
    public void OnBaseUnderAttack()
    {
        warningUI.SetActive(true);  // Show the UI element
        timer = displayTime;  // Reset the timer
    }
}
