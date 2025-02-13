
using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI Counter;

    private double mTotal;
    private int mCounts;
    private float mTimer;
    
    public void Update()
    {
        mTotal += Time.unscaledDeltaTime;
        mCounts++;
        
        mTimer -= Time.unscaledDeltaTime;

        if (mTimer <= 0)
        {
            Counter.SetText(Convert.ToString((int)(1f / (mTotal / mCounts)), CultureInfo.InvariantCulture));
            
            mCounts = 0;
            mTotal = 0;
            mTimer = 0.5f;
        }
    }
}