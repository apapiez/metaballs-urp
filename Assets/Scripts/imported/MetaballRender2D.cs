using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// A scriptable render feature to add 2D metaballs
/// </summary>
public class MetaballRender2D : ScriptableRendererFeature
{
    /// <summary>
    /// Holds the settings for render feature
    /// </summary>
    [System.Serializable]
    public class MetaballRender2DSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;                      // The point in the rendering pipeline where this pass should be executed.

        [Range(0f, 1f), Tooltip("Outline size.")] 
        public float outlineSize = 1.0f;                                                                            // The size of the outline around the metaball.

        [Tooltip("Inner color.")]
        public Color innerColor = Color.white;                                                                     // The inner color of the metaball.                              

        [Tooltip("Outline color.")]
        public Color outlineColor = Color.black;                                                                    // The outline color of the metaball.
    }

    public MetaballRender2DSettings settings = new MetaballRender2DSettings();                                      // Instantiate the settings object.

    /// <summary>
    /// The scriptable render pass
    /// </summary>
    class MetaballRender2DPass : ScriptableRenderPass
    {
        private Material material;                                                                                  // THe material to use on this pass

        public float outlineSize;                                                                                   // The size of the outline around the metaball.
        public Color innerColor;                                                                                    // The inner color of the metaball.
        public Color outlineColor;

        private bool isFirstRender = true;                                                                          // Whether this is the first pass of the render.

        private RenderTargetIdentifier source;                                                                      // The source render target.        
        private string profilerTag;                                                                                 // The profiler tag to use.

        /// <summary>
        /// Initializes the pass.
        /// </summary>
        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;                                                                                   // Set the source render target.

            material = new Material(Shader.Find("Hidden/Metaballs2D"));                                             // Create a new material using the Metaballs2D shader
        }

        /// <summary>
        /// constructor for the pass
        /// </summary>
        /// <param name="profilerTag">The profiler tag to use.</param>
        public MetaballRender2DPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        /// <summary>
        /// Executes the pass.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="renderingData">The rendering data.</param>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);                                             // Get a command buffer from the pool.

            if(isFirstRender)                                                                                   // If this is the first pass of the render...
            {
                isFirstRender = false;                                                                          // Set the flag to false.
                cmd.SetGlobalVectorArray("_MetaballData", new Vector4[1000]);                                   // Set the global array of metaball data with a size of 1000.
            }

            List<Metaball2D> metaballs = MetaballSystem2D.Get();                                                // Get the metaballs from the metaball system.
            List<Vector4> metaballData = new List<Vector4>(metaballs.Count);                                    // Create a new list of Vector4s to hold the metaball data.

            for(int i = 0; i < metaballs.Count; ++i)                                                            // For each metaball...
            {
                Vector2 pos = renderingData.cameraData.camera.WorldToScreenPoint(metaballs[i].transform.position);  // Get the screen position of the metaball.
                float radius = metaballs[i].GetRadius();                                                            // Get the radius of the metaball.
                metaballData.Add(new Vector4(pos.x, pos.y, radius, 0.0f));                                          // Add the data to the metaball data list.
            }

            if(metaballData.Count > 0)                                                                        // If there are metaballs...                        
            {
                cmd.SetGlobalInt("_MetaballCount", metaballs.Count);                                           // Set the global metaball count.
                cmd.SetGlobalVectorArray("_MetaballData", metaballData);                                       // Set the global metaball data.
                cmd.SetGlobalFloat("_OutlineSize", outlineSize);                                               // Set the global outline size.
                cmd.SetGlobalColor("_InnerColor", innerColor);                                                  // Set the global inner color.
                cmd.SetGlobalColor("_OutlineColor", outlineColor);                                              // Set the global outline color.
                cmd.SetGlobalFloat("_CameraSize", renderingData.cameraData.camera.orthographicSize);            // Set the global camera size.

                cmd.Blit(source, source, material);                                                             // Blit the source render target to the source render target, using the metaball material.

                    context.ExecuteCommandBuffer(cmd);                                                          // Execute the command buffer.
                }
            
            cmd.Clear();                                                                                        // Clear the command buffer.
            CommandBufferPool.Release(cmd);                                                                     // Release the command buffer.
        }
    }

    MetaballRender2DPass pass;                                                                                  // Declare our pass variable

    public override void Create()                                                                               // Called when the script is created.
    {
        name = "Metaballs (2D)";                                                                                // Set the name of the render feature.

        pass = new MetaballRender2DPass("Metaballs2D");                                                         // Instantiate a new pass

        pass.outlineSize = settings.outlineSize;                                                                // Pass data from the settings to the pass.
        pass.innerColor = settings.innerColor;
        pass.outlineColor = settings.outlineColor;

        pass.renderPassEvent = settings.renderPassEvent;
    }

    /// <summary>
    /// Queues the pass for execution.
    /// </summary>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(pass);
    }
}
