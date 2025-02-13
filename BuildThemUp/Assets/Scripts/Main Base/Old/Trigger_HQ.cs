using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_HQ : MonoBehaviour
{
    [SerializeField] private bool triggerActive = false;
    [SerializeField] GameObject pressE;
    [SerializeField] GameObject hQUI;


    private void Update()
    {
        //Log.Info(this, $"Distance from shop: {Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position)}");
        if (Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) <= 10)
        {
            pressE.SetActive(true);
            triggerActive = true;
        }
        else 
        { 
            pressE.SetActive(false);
            triggerActive = false;
        }

        if (triggerActive && Input.GetKeyDown(KeyCode.E))
        {
            OpenHQ();
        }
        if (!triggerActive && hQUI.activeInHierarchy)
        {
            hQUI.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            //Debug.Log("Store closed");
        }
    }

    public void OpenHQ()
    {
        if (!hQUI.activeInHierarchy)
        {
            hQUI.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            //Debug.Log("Store open");
        }
        else if(hQUI.activeInHierarchy)
        {
            hQUI.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            //Debug.Log("Store closed");
        }
        else { Debug.Log("Error opening store"); }
    }
}
