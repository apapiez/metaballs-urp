using System;
using UnityEngine.Rendering.Universal;

public class ComputeFeature : ScriptableRendererFeature
{

    /// <summary>
    /// Holds the settings for render feature
    /// </summary>
    [Serializable]
    public class ComputeSettings {
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingOpaques;                                       // The point in the rendering pipeline where this pass should be executed.
        public ComputeAsset computeAsset;                                                                               // The compute asset to use.
    }

    public ComputeSettings settings = new ComputeSettings();                                                            // The settings for this feature.

    private ComputePass computePass;                                                                                    // The compute pass to use.

    /// <summary>
    /// Initializes the feature.
    /// </summary>
    public override void Create()
    {
        if (settings.computeAsset == null) { return;}                                                                  // If the compute asset is null, return.

        settings.computeAsset.Setup();                                                                                 // Setup the compute asset.
        computePass = new ComputePass(name, settings);                                                                 // Create a new compute pass.
    }

    /// <summary>
    /// Add the compute pass to the render queue.
    /// </summary>
    /// <param name="renderer">The renderer to add the pass to.</param>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.computeAsset == null) { return;}                                                                 // If the compute asset is null, return.
        renderer.EnqueuePass(computePass);                                                                            // Otherwise add the pass to the render queue.
    }

    /// <summary>
    /// Clean up the feature.
    /// </summary>
    void OnDisable()
    {
        if (settings.computeAsset == null) { return;}                                                                 // If the compute asset is null, return.
        settings.computeAsset.Cleanup();                                                                              // Clean up the compute asset.
    }

 

    
}