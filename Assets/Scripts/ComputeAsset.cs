using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Rendering;

/// <summary>
/// The base class for all compute assets.
/// </summary>
public abstract class ComputeAsset : UnityEngine.ScriptableObject
{
    public UnityEngine.ComputeShader shader;

    public virtual void Setup() { }
    public abstract void Render(CommandBuffer commandBuffer, int kernelHandle);
    public virtual void Cleanup() { }
}
