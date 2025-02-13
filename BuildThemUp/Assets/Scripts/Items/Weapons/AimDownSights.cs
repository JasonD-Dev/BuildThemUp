using System;
using Unity.IntegerTime;
using Unity.VisualScripting;
using UnityEngine;

public class AimDownSights : MonoBehaviour
{
    [Header("ADS Settings")]
    [SerializeField] public bool canADS = true;
    [SerializeField] Vector3 mAdsPosition = new Vector3(-0.115f, 0.01f, -0.36f);
    [SerializeField, Range(30f, 100f)] float mZoomFOV = 60f;
    [SerializeField, Range(0.1f, 0.5f)] float mZoomTime = 1f;

    //Keeping Track
    public bool isADS { get; private set; } = false;
    float mLastADSTimeStamp = 0;

    Camera mCamera; //Assigned in start();
    private Vector3 mIdlePosition = Vector3.zero; //Assigned in start();

    void Start()
    {
        mCamera = Camera.main;
        mIdlePosition = transform.localPosition;
    }

    private void Update()
    {
        if (canADS == true)
        {
            if (Input.GetMouseButtonDown(1) == true) //Only activate when the button is initially pressed.
            {
                mLastADSTimeStamp = Time.time;
            }

            if (Input.GetMouseButton(1) == true)
            {
                ADS(true);
            }
            if (Input.GetMouseButton(1) == false || Input.GetMouseButtonUp(1) == true)
            {
                ADS(false);
            }
        }
        else
        {
            ADS(false);
        }
    }

    void ADS(bool aADS)
    {
        if (aADS == true) //Is currently not ADS, thus we should ADS.
        {
            //Smoothing Zoom in.
            float t = (Time.time - mLastADSTimeStamp) / mZoomTime;
            mCamera.fieldOfView = Mathf.Lerp(CameraController.defaultFOV, mZoomFOV, t);
            transform.localPosition = Vector3.Lerp(mIdlePosition, mIdlePosition + mAdsPosition, t);
            

            if (Mathf.Abs(mCamera.fieldOfView - mZoomFOV) <= 1f)
            {
                mCamera.fieldOfView = mZoomFOV;
                transform.localPosition = mIdlePosition + mAdsPosition;

                isADS = true;
                return;
            }
        }
        else
        {
            mCamera.fieldOfView = CameraController.defaultFOV;
            transform.localPosition = mIdlePosition;
            isADS = false;
            return;
        }
    }
}
