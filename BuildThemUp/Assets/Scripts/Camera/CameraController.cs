using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance => mInstance;
    private static CameraController mInstance;

    //Camera Settings
    public static int defaultFOV = 100;

    public Camera fpvCamera;
    
    [SerializeField] private VolumeProfile mVolProfile;
    private Vignette mVignette;
    private bool mPulseVignette;
    private float mOffset;
    private float mAmplitude;
    private float mFreq;
    private float mTimer;

    void Awake()
    {
        if (mInstance)
            Destroy(mInstance.gameObject);

        mInstance = this;

        Cursor.lockState = CursorLockMode.Locked;

        // assign volume profile var
        mVolProfile.TryGet(out mVignette);
        // deactivate vignette if unintentionally left active
        mVignette.intensity.value = 0;
        mVignette.active = false;
    }

    public void OnDestroy()
    {
        // just to avoid saving values and having them update in the scene
        mVignette.intensity.value = 0;
        mVignette.active = false;
    }

    public void SetVignette(float aIntensity)
    {
        if (aIntensity > 0 && !mVignette.active)
            mVignette.active = true;
        else if (aIntensity <= 0 && mVignette.active)
            mVignette.active = false;
        
        mVignette.intensity.value = aIntensity;
        mPulseVignette = false;
    }

    public void SetVignette(float aMin, float aMax, float aFreq)
    {
        if (!mVignette.active)
            mVignette.active = true;
        
        mTimer = 0; // start at highest intensity when hit
        mAmplitude = (aMax - aMin) / 2;
        mLerpT = 0;
        // 2x more intense vignette when hit
        mLerpAmplitude = mAmplitude * 2f;
        mOffset = (aMin + aMax) / 2;
        
        mFreq = aFreq;
        mPulseVignette = true;
    }

    // more intense vignette pulse when hit
    private float mLerpT;
    private float mLerpAmplitude;
    private float mLerpDuration = 1f;

    void Update()
    {
        if (mPulseVignette)
        {
            mLerpT += Time.deltaTime;
            var tAmplitude = Mathf.Lerp(mLerpAmplitude, mAmplitude, mLerpT / mLerpDuration);
            
            mTimer += Time.deltaTime;
            mVignette.intensity.value = Mathf.Cos(mTimer * Mathf.PI * mFreq) * tAmplitude + mOffset;
        }
    }

    public void SetParent(Transform aParent, Vector3 aInitialPos)
    {
        transform.SetParent(aParent, false);
        transform.localPosition = aInitialPos;
    }

    public void SetAngleX(float aX)
    {
        Vector3 tNewEulerAngles = transform.localEulerAngles;
        tNewEulerAngles.x = aX;
        transform.localEulerAngles = tNewEulerAngles;
    }
}
