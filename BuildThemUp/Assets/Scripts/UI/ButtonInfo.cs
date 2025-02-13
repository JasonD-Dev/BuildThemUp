using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInfo : MonoBehaviour
{
    public int itemID;
    public TextMeshProUGUI priceTxt;
    public TextMeshProUGUI quantityTxt;
    public GameObject hQManager;

    /*
    void Update()
    {
        priceTxt.text = "Price: " + hQManager.GetComponent<MainBase>().hQItems[2, itemID].ToString() + " scrap.";
        quantityTxt.text = hQManager.GetComponent<MainBase>().hQItems[3, itemID].ToString();
    }
    */
}
