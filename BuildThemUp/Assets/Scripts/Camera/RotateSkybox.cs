using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    // Rotation speed variable that can be set from the Inspector
    public float rotationSpeed = 1.0f;

    void Update()
    {
        // Rotate the skybox around the Y-axis
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed);
    }
}
