using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] GameObject mOpenObjectiveDisplay;
    [SerializeField] GameObject mClosedObjectiveDisplay;

    //Checkmarks
    [SerializeField] GameObject mBaseDefenseCheckmark;
    [SerializeField] GameObject mSatelliteRepairCheckmark;

    [SerializeField] KeyCode mOpenObjectivesKey = KeyCode.T;

    bool mIsOpen = true;

    private void Awake()
    {
        SetDisplayState(true);

        //Initiate as false
        mBaseDefenseCheckmark.SetActive(false);
        mSatelliteRepairCheckmark.SetActive(false);
    }

    private void Update()
    {
        //Toogle Display states between open and closed.
        if (Input.GetKeyUp(mOpenObjectivesKey))
        {
            SetDisplayState(!mIsOpen);
        }
    }

    private void FixedUpdate()
    {
        //Defned base check mark
        if (EnemiesManager.instance.waveNumber > 1)
        {
            mBaseDefenseCheckmark.SetActive(true);
        }

        //Satellite check mark
        if (GamePlayManager.instance.numActiveSatellites > 0)
        {
            mSatelliteRepairCheckmark.SetActive(true);
        }

        //Survice check mark
        //None needed since you get teleported to the next scene ;D
    }

    //True == Open, False == Closed
    private void SetDisplayState(bool aBool)
    {
        mIsOpen = aBool;
        mOpenObjectiveDisplay.SetActive(aBool);
        mClosedObjectiveDisplay.SetActive(!aBool);
    }
}

