using UnityEngine;
using TMPro;

public class TextAndBackgroundManager : MonoBehaviour
{
    [SerializeField] private GameObject mText;
    [SerializeField] private GameObject mBackgroundRectangle;
    bool mAlreadyEnabled = true;

    private void Update()
    {
        if (!mText.activeInHierarchy)
        {
            if (!mAlreadyEnabled)
            {
                return;
            }
            DisableTextAndBackground();
        }
        else 
        {
            if (mAlreadyEnabled) { 
                return;
            }
            EnableTextAndBackground();
        }

    }
    public void DisableTextAndBackground()
    {
        mBackgroundRectangle.SetActive(false);
        mAlreadyEnabled = false;
    }
    public void EnableTextAndBackground()
    {
        mBackgroundRectangle.SetActive(true);
        mAlreadyEnabled = true;
    }
}
