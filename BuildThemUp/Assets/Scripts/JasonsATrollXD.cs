using UnityEngine;

public class JasonsATrollXD : MonoBehaviour
{
    [SerializeField] KeyCode X = KeyCode.C;
    [SerializeField] KeyCode D = KeyCode.LeftShift;
    private Transform L;
    private MovementController mMovementController;
    private float mDefaultMaxSpeed;
    private bool mIsCrouched;
    private bool mIsRunning;

    void Start()
    {
        
        mMovementController = GetComponent<MovementController>();
        if (mMovementController == null)
        {
            Debug.Log("No Movement CONTROLLER NERD");
            return;
        }

        L = GameObject.FindGameObjectWithTag("MainCamera").transform;
        if (L == null)
        {
            Debug.Log("No Camera OBJECT");
            return;
        }

        mDefaultMaxSpeed = mMovementController.mMaxSpeed;
    }

    void Update()
    {
        if (Input.GetKey(X))
        {
            if (Input.GetMouseButton(1))
            {
                Debug.Log("Cant C while ADS");
                return;
            }
            else
            {
                Crouch();
            }
        }
        else
        {
            UnCrouch();
        }

        if (Input.GetKey(D)) 
        {
            Run();
        }
        else
        { 
            StopRun();
        }
    }

    private void Crouch()
    {
        if (mIsCrouched) 
        { 
            return; 
        }

        L.transform.localPosition = new Vector3(L.transform.localPosition.x, (L.transform.localPosition.y - .06f), L.transform.localPosition.z);
        mIsCrouched = true;
    }
    private void UnCrouch()
    {
        if (!mIsCrouched)
        {
            return;
        }
        L.transform.localPosition = new Vector3(L.transform.localPosition.x, (L.transform.localPosition.y + .06f), L.transform.localPosition.z);
        mIsCrouched = false;
    }

    private void Run()
    {
        if (mIsRunning)
        { 
            return;
        }
        mMovementController.mMaxSpeed = 9f;
        mIsRunning = true;
    }

    private void StopRun()
    {
        if (!mIsRunning)
        {
            return;
        }
        mMovementController.mMaxSpeed = mDefaultMaxSpeed;
        mIsRunning = false;
    }
}
