using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickAudio : MonoBehaviour
{
    [SerializeField] AudioClip mClip;
    [SerializeField, Range(0f, 1f)] float mVolume = 1f;
    Button mButton; //Assigned in awake
    private void Awake()
    {
        mButton = GetComponent<Button>();
        if (mButton == null )
        {
            Log.Warning(this, "No button component found.");
            Destroy(this);
        }

        mButton.onClick.AddListener(PlayClickSound);
    }

    void PlayClickSound()
    {
        Log.Info(this, "Playing Button Clip");
        PlayerController.instance.PlayOneShot(mClip, mVolume);
    }
}
