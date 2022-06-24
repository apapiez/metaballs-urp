using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// The 3D Ray marching compute asset.
/// Responsibility:
/// - Holds the reference to the compute shader
/// - Prepares shape data for the compute shader
/// </summary>

[CreateAssetMenu(menuName = "Rendering/RayMarch3DCompute")]
public class RayMarch3DCompute : ComputeAsset
{
    Camera cam;                                                                                 // The camera to use.
    Light lightSource;                                                                          // The light source to use.            
    List<ComputeBuffer> buffersToDispose;                                                       // The list of buffers to dispose.


    /// <summary>
    /// Instantiates the camera and lightsource
    /// </summary>
    public override void Setup()
    {
        cam = Camera.current;
        lightSource = FindObjectOfType<Light>();

    }
    
    /// <summary>
    /// The main function of the asset
    /// </summary>
    public override void Render(CommandBuffer commandBuffer, int kernelHandle)
    {
        buffersToDispose = new List<ComputeBuffer> ();
        CreateScene ();
        SetParameters ();    }

    /// <summary>
    /// Creates the scene data for the compute shader
    /// </summary>
 void CreateScene () {
        List<Shape> allShapes = new List<Shape> (FindObjectsOfType<Shape> ());                             // Get all the shapes in the scene.
        allShapes.Sort ((a, b) => a.operation.CompareTo (b.operation));                                    // Sort the shapes by operation.

        List<Shape> orderedShapes = new List<Shape> ();                                                    // Create a new orderec list of shapes.

        for (int i = 0; i < allShapes.Count; i++) {                                                         // For each shape in the scene.
            // Add top-level shapes (those without a parent)
            if (allShapes[i].transform.parent == null) {                                                    // If the shape has no parent.

                Transform parentShape = allShapes[i].transform;                                             // Get the transform of the shape.
                orderedShapes.Add (allShapes[i]);                                                           // Add the shape to the ordered list
                allShapes[i].numChildren = parentShape.childCount;                                         // Set the number of children of the shape.                                   
                // Add all children of the shape (nested children not supported currently)
                for (int j = 0; j < parentShape.childCount; j++) {                                         // For each child of the shape.
                    if (parentShape.GetChild (j).GetComponent<Shape> () != null) {                         // If the child is a shape.
                        orderedShapes.Add (parentShape.GetChild (j).GetComponent<Shape> ());                // Add the shape to the ordered list.
                        orderedShapes[orderedShapes.Count - 1].numChildren = 0;                             // Set the number of children of the shape to 0 as we don't support nested children.
                    }
                }
            }

        }

        ShapeData[] shapeData = new ShapeData[orderedShapes.Count];                                        // Create a new array of shape data.
        for (int i = 0; i < orderedShapes.Count; i++) {                                                   // For each shape in the ordered list of shapes.
            var s = orderedShapes[i];                                                                      // Get the shape.
            Vector3 col = new Vector3 (s.colour.r, s.colour.g, s.colour.b);                               // Get the colour of the shape.
            shapeData[i] = new ShapeData () {                                                             // Create a new shape data object.
                position = s.Position,                                                                    // Set the shape data position to the position of the shape.
                scale = s.Scale, colour = col,                                                            // Set the shape data scale and colour to the scale and colour of the shape.
                shapeType = (int) s.shapeType,                                                            // Set the shape data shape type to the shape type of the shape.
                operation = (int) s.operation,                                                            // Set the shape data operation to the operation of the shape.
                blendStrength = s.blendStrength*3,                                                        // Set the shape data blend strength to the blend strength of the shape.
                numChildren = s.numChildren                                                               // Set the shape data number of children to the number of children of the shape.
            };
        }

        ComputeBuffer shapeBuffer = new ComputeBuffer (shapeData.Length, ShapeData.GetSize ());           // Create a new compute buffer for the shape data.
        shapeBuffer.SetData (shapeData);                                                                  // Set the data of the compute buffer to the shape data.
        shader.SetBuffer (0, "shapes", shapeBuffer);                                                 // Set the shape buffer of the compute shader.  
        shader.SetInt ("numShapes", shapeData.Length);                                               // Set the number of shapes in the scene on the compute shader.

        buffersToDispose.Add (shapeBuffer);                                                               // Add the shape buffer to the list of buffers to dispose of.
    }

    /// <summary>
    /// Sets the parameters of the compute shader
    /// </summary>
    void SetParameters()
    {
        bool lightIsDirectional = lightSource.type == LightType.Directional;
        shader.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
        shader.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);
        shader.SetVector("_Light", (lightIsDirectional) ? lightSource.transform.forward : lightSource.transform.position);
        shader.SetBool("positionLight", !lightIsDirectional);
    }

   struct ShapeData {
        public Vector3 position;                    // Position of the shape.
        public Vector3 scale;                       // Scale of the shape.
        public Vector3 colour;                      // Colour of the shape.
        public int shapeType;                       // Shape type of the shape.
        public int operation;                       // Operation of the shape.
        public float blendStrength;                 // Blend strength of the shape.
        public int numChildren;                     // Number of children of the shape.

        /// <summary>
        /// Gets the size of the struct, 3 ints and 10 floats. Size will vary depending on the system but must be set to the correct value to avoid segmentation faults or other errors.
        public static int GetSize () {
            return sizeof (float) * 10 + sizeof (int) * 3;
        }
    }
}
