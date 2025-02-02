using System;
using root.AudioLink.Core;
using UnityEngine;

namespace root.AudioLink.Graphics.DataToTextures
{
    public abstract class DataToTexture : MonoBehaviour
    {
        [SerializeField] protected ComputeShader shader;
        [SerializeField] protected SpectrumData spectrum;
        [SerializeField] protected AudioSettings settings;
        [SerializeField] protected int resolution;
        
        protected ComputeBuffer Buffer;

        protected void Start()
        {
            if (shader == null || spectrum == null || settings == null)
            {
                Debug.LogError("Null references on SpectrumToTexture");
                return; 
            }
            
            resolution = settings.SpectrumResolution;
        }
    }
}