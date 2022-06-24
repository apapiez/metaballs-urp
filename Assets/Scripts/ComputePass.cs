using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class ComputePass : ScriptableRenderPass
{
    private string profilerTag;                                                                        // The profiler tag for debugging.
    private ComputeAsset computeAsset;

    private readonly int TemporaryBufferID = Shader.PropertyToID("temporaryBuffer");
    private readonly int TargetBufferID = Shader.PropertyToID("targetBuffer");                         // The UID of the target buffer write-to by the compute shader.

    private readonly int ConvergedBufferID = Shader.PropertyToID("convergedBuffer");
    private Material addMaterial;
    private int currentSample;

    public ComputePass(string profilerTag, ComputeFeature.ComputeSettings settings)
    {
        this.profilerTag = profilerTag;
        computeAsset = settings.computeAsset;
        renderPassEvent = settings.passEvent;
        currentSample = 0;
        computeAsset.Setup();
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (computeAsset == null || computeAsset.shader == null) {return;}                             // If no compute shader is assigned, return.
        if(addMaterial == null)                                                                        // If no material is assigned, create one.
        {
            //addMaterial = new Material(Shader.Find("Hidden/Add"));                                     // Create a material to be applied when rendering the compute shader
            Debug.Log("No add material assigned, skipping add material");
        }
        
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);                                       // Get a command buffer from the pool
        ScriptableRenderer renderer = renderingData.cameraData.renderer;
       
        

        int kernelHandle = computeAsset.shader.FindKernel("CSMain");                                  // Get a handle for the main function of the compute shader

        using (new ProfilingScope(cmd, new ProfilingSampler(profilerTag)))                            // Queue instructions for the compute shader command buffer
        {
            
            
            computeAsset.Render(cmd, kernelHandle);      
            cmd.GetTemporaryRT(TemporaryBufferID, renderingData.cameraData.camera.pixelWidth, renderingData.cameraData.camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(TargetBufferID, renderingData.cameraData.camera.pixelWidth, renderingData.cameraData.camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1, true, RenderTextureMemoryless.None , false);
            //new temporary render texture with UAV flag set
            Blit(cmd, renderer.cameraColorTarget, TemporaryBufferID);
            cmd.SetComputeTextureParam(computeAsset.shader, kernelHandle, "Destination", TargetBufferID);
            cmd.SetComputeTextureParam(computeAsset.shader, kernelHandle, "Source", TemporaryBufferID);                                             // Set the shader parameters according to the current scene state
            cmd.DispatchCompute(computeAsset.shader, kernelHandle, Mathf.CeilToInt(Screen.width / 8), Mathf.CeilToInt(Screen.height / 8), 1); // Dispatch the compute shader
            if (addMaterial == null)
            {
                Blit(cmd, TargetBufferID, renderer.cameraColorTarget);                     // Blit the result of the compute shader to the screen
            }
            else
            {

                Blit(cmd, TargetBufferID, ConvergedBufferID, addMaterial);
                Blit(cmd, ConvergedBufferID, renderer.cameraColorTarget);                // Blit the result of the compute shader to the screen
            }
        }

        context.ExecuteCommandBuffer(cmd);                                                            // Execute the command buffer
        cmd.Clear();                                                                                  // clear the command buffer
        CommandBufferPool.Release(cmd);                                                               // release the command buffer



    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        base.OnCameraCleanup(cmd);                                                                    // Call the base method
        cmd.ReleaseTemporaryRT(TargetBufferID);                                                       // Release temporary render targets
        cmd.ReleaseTemporaryRT(ConvergedBufferID);
        cmd.ReleaseTemporaryRT(TemporaryBufferID);
    }

    public void Dispose() => Material.Destroy(addMaterial);                                          // Dispose of the material

}
