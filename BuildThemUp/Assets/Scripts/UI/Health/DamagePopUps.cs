using System.Collections.Generic;
using TMPro;
using UnityEngine;
using RangeAttribute = UnityEngine.RangeAttribute;

[RequireComponent(typeof(Health))]
public class DamagePopUps : MonoBehaviour
{
    Health mHealth;

    [Header("Components")]
    [SerializeField] TextMeshProUGUI mTextPrefab;
    [SerializeField] Transform mSpawnPoint;

    [Header("Visuals")]
    [SerializeField, Range(1, 10)] int mMaxNumberOfPopUps = 5;
    [SerializeField, Range(0.1f, 3)] float mLifeTime = 1;
    [SerializeField] float mMaxHeight = 2;
    [SerializeField] float mMaxStartHorizontal = 2;
    [SerializeField] float mMaxEndHorizontal = 2;
    [SerializeField] Color mEndColor;

    PopUpsData[] mPopUpsData; //Assigned in Start();
    List<int> mPopUpsActiceIndices; //Assigned in Start();

    Color mStartColor; // Assigned in Start();

    private void Awake()
    {
        mHealth = GetComponent<Health>();
        if (mHealth == null)
        {
            Log.Error(this, "Health componenet was not found! Destroying...");
            Destroy(this);
            return;
        }
    }

    void Start()
    {
        mPopUpsActiceIndices = new List<int>();
        mPopUpsData = new PopUpsData[mMaxNumberOfPopUps];
        for (int i = 0; i < mPopUpsData.Length; i++)
        {
            TextMeshProUGUI tText = Instantiate(mTextPrefab, mSpawnPoint);
            mPopUpsData[i] = new PopUpsData(tText);
        }

        mStartColor = mTextPrefab.color;

        mHealth.onDamageTaken.AddListener(ActivateVacantPopUp);
    }

    void FixedUpdate()
    {
        float tTime = Time.time;

        for (int i = mPopUpsActiceIndices.Count - 1; i >= 0; i--)
        {
            PopUpsData tPopUpData = mPopUpsData[mPopUpsActiceIndices[i]];

            if (tTime - tPopUpData.lastActiveTimeStamp >= mLifeTime)
            {
                tPopUpData.Deactivate();
                mPopUpsActiceIndices.RemoveAt(i);
                continue;
            }

            float tRatioComplete = (tTime - tPopUpData.lastActiveTimeStamp) / mLifeTime;

            tPopUpData.popUp.transform.localPosition = Vector3.Lerp(tPopUpData.startPosition, tPopUpData.endPosition, tRatioComplete); ;
            tPopUpData.popUp.color = Color.Lerp(mStartColor, mEndColor, tRatioComplete);
        }
    }

    // Added to to Health's ontakedamage UnityEvent.
    // Everytime health takes damage, this function should be called.
    void ActivateVacantPopUp(float aAmount, Transform aAttackerTransform) 
    {
        //Log.Info(this, "Adding popup with damage value of " + aAmount.ToString());
        int tOldestPopUpIndex = 0;
        for (int i = 0; i < mPopUpsData.Length; i++)
        {
            if (tOldestPopUpIndex == i)
            {
                continue;
            }

            //Since its a timestamp, smaller value means its older. 
            if (mPopUpsData[i].lastActiveTimeStamp < mPopUpsData[tOldestPopUpIndex].lastActiveTimeStamp)
            {
                tOldestPopUpIndex = i;
            }
        }

        Vector3 tStartPosition = new Vector3(Random.Range(-mMaxStartHorizontal / 2, mMaxStartHorizontal / 2), 0, 0);
        Vector3 tEndPositin = new Vector3(Random.Range(-mMaxEndHorizontal / 2, mMaxEndHorizontal / 2), mMaxHeight, 0);
        mPopUpsData[tOldestPopUpIndex].Activate(-aAmount, tStartPosition, tEndPositin);

        if (mPopUpsActiceIndices.Contains(tOldestPopUpIndex) == false) 
        {
            mPopUpsActiceIndices.Add(tOldestPopUpIndex);
        }
    }
}

internal struct PopUpsData
{
    public TextMeshProUGUI popUp { get; private set; }
    public float lastActiveTimeStamp { get; private set; }
    public Vector3 startPosition { get; private set; }
    public Vector3 endPosition { get; private set; }
    

    public PopUpsData(TextMeshProUGUI aPopUp)
    {
        popUp = aPopUp;
        lastActiveTimeStamp = 0; 
        startPosition = Vector3.zero;
        endPosition = Vector3.zero;

        popUp.gameObject.SetActive(false);
    }

    public void Activate(float aAmount, Vector3 aStartPosition, Vector3 aEndPosition)
    {
        lastActiveTimeStamp = Time.time;

        startPosition = aStartPosition;
        endPosition = aEndPosition;

        popUp.gameObject.SetActive(true);

        popUp.text = Mathf.Floor(aAmount).ToString();
    }

    public void Deactivate()
    {
        popUp.gameObject.SetActive(false);
    }
}
