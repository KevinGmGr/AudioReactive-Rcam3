using System;
using root.AudioLink.Core;
using UnityEngine;

namespace root.AudioLink.Graphics.DataToTextures
{
    public abstract class ComputeShaderFromAudio : AudioElement
    {
        [SerializeField] protected ComputeShader shader;
        [SerializeField] protected int resolution;
        
        protected ComputeBuffer Buffer;

        protected void Start()
        {
            if (shader == null)
            {
                Debug.LogError("Compute Shader missing on: " + name);
                return; 
            }
            
            resolution = settings.SpectrumResolution;
        }
    }
}