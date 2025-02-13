using TMPro;
using UnityEngine;

public class Prompt : MonoBehaviour
{
    // Padding: how much wider the backing rect should be than the text (aka how much padding)
    [SerializeField] private int mPadding = 100;
    
    [SerializeField] private TextMeshProUGUI mTextMesh;
    [SerializeField] private RectTransform mBackRect;

    public bool SetText(string aText)
    {
        if (mTextMesh.text == aText)
        {
            return false;
        }

        // set the text
        mTextMesh.text = aText;

        // set the backing rectangle size
        var tSize = mBackRect.sizeDelta;
        tSize.x = mTextMesh.GetPreferredValues(aText).x + mPadding;
        mBackRect.sizeDelta = tSize;
        return true;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }
    
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}