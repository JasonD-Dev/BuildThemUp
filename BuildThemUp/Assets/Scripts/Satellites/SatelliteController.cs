using System;
using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.VFX;

public class SatelliteController : MonoBehaviour
{
    [SerializeField] private float mSatelliteInteractDistance = 5f;
    [SerializeField] private float mActivationDelay = 10f;
    [SerializeField] private int mActivationCost = 3500;
    [SerializeField] private AudioClip mRepairSound;
    [SerializeField] private Transform mDishTransform;
    [SerializeField] private Transform mRotorTransform;
    [SerializeField] private VisualEffect mSatelliteBeam;

    public float communicationTime = 0;
    public bool activated = false;
    
    private Transform mPlayerTransform;
    private Transform mTransform;
    private bool mIsPlayerClose;

    private byte mPromptId;
    private Vector3 mRotorRotation = new Vector3(0, 0, 10f);
    private bool mActivating;
    [SerializeField] private GameObject mIndicator;

    public void Start()
    {
        mPlayerTransform = GamePlayManager.instance.playerTransform;
        mTransform = transform;
    }

    public void Update()
    {
        if (activated)
        {
            mRotorRotation.z += Time.deltaTime * 5f;
            mRotorTransform.localEulerAngles = mRotorRotation;
        }
    }

    public void FixedUpdate()
    {
        if (activated)
        {
            communicationTime += Time.fixedDeltaTime;
            return;
        }

        // also don't bring up prompt if in the middle of activating
        if (mActivating)
            return;
        
        bool tWasPlayerClose = mIsPlayerClose;
        mIsPlayerClose = Vector3.Distance(mTransform.position, mPlayerTransform.position) < mSatelliteInteractDistance;
        
        if (!tWasPlayerClose && mIsPlayerClose)
            mPromptId = PromptManager.instance.ShowPrompt($"[E] Repair Satellite ({mActivationCost})", Repair, KeyCode.E);
        if (tWasPlayerClose && !mIsPlayerClose)
            PromptManager.instance.RemovePrompt(mPromptId);
    }

    public void Repair()
    {
        if (!GamePlayManager.instance.SpendScrap(mActivationCost) || mActivating || activated)
            return;
        
        // we repaired, so hide the prompt
        PromptManager.instance.RemovePrompt(mPromptId);

        // so the prompt doesn't come up while it's being activated
        mActivating = true;
        
        // make dish upright (just to show it's activated)
        Tween.LocalRotation(mDishTransform, Vector3.zero, mActivationDelay, Ease.InOutCubic);
        //mDishTransform.localEulerAngles = Vector3.zero;

        PlayerController.instance.PlayOneShot(mRepairSound);

        // Remove blue indicator no longer need
        Destroy(mIndicator); 

        // coroutines: not very efficient... but we're doing this once, so who cares
        StartCoroutine(SetActivated(mActivationDelay));
    }

    public IEnumerator SetActivated(float aDelay)
    {
        yield return new WaitForSeconds(aDelay);
        // spool up: ease into the rotation so it isn't jarring
        float tWarmUp = 5f;
        Tween.LocalRotation(mRotorTransform, mRotorRotation, tWarmUp, Ease.InCubic);
        yield return new WaitForSeconds(tWarmUp);
        // enable the beam
        mSatelliteBeam.enabled = true;
        activated = true;
        mActivating = false; // we're not activatING anymore, we've activatED
    }
}