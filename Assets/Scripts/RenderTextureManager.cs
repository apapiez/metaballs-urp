using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureManager : MonoBehaviour
{
    private void Awake()
    {
        RenderTexture tex = new RenderTexture(Screen.width, Screen.height, 24);
        Debug.Assert(tex.Create(), "Failed to create camera render texture"); // Assert that the render texture was created successfully.
        Camera cam = GetComponent<Camera>();                                  // Get the camera component.
        cam.targetTexture = tex;                                              // Set the camera's target texture to the render texture.
    }
    
}
