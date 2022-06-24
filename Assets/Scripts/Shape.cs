using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Shape monobehaviour, marking a GameObject as a RayMarchable shape.
/// </summary>
public class Shape : MonoBehaviour
{

    public enum ShapeType {Sphere,Cube,Torus};                                      // The type of shape.
    public enum Operation {None, Blend, Cut,Mask}                                   // The operation to perform on the shape.

    public ShapeType shapeType;                                                     
    public Operation operation;
    public Color colour = Color.white;                                              // The colour of the shape.
    [Range(0,1)]
    public float blendStrength;                                                     // The strength of the blend operation.
    [HideInInspector]
    public int numChildren;                                                         // The number of children of this shape.

    public Vector3 Position {
        get {
            return transform.position;                                              
        }
    }

    public Vector3 Scale {
        get {
            Vector3 parentScale = Vector3.one;
            if (transform.parent != null && transform.parent.GetComponent<Shape>() != null) {
                parentScale = transform.parent.GetComponent<Shape>().Scale;                          // Get the scale of the parent shape.
            }
            return Vector3.Scale(transform.localScale, parentScale);                                // Return the local scale of the shape.
        }
    }
}
