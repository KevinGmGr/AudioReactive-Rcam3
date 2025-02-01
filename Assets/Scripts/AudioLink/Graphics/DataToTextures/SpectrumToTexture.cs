using System;
using root.AudioLink.Core;
using UnityEngine;
using Unity.Collections; 
using UnityEngine.Serialization;

namespace root.AudioLink.Graphics.DataToTextures
{
    public class SpectrumToTexture : MonoBehaviour
    {
        [SerializeField] private ComputeShader shader;
        [SerializeField] private SpectrumData spectrum;
        [SerializeField] private AudioSettings settings; 
        [SerializeField] private Material material;
        
        private RenderTexture _texture;
        private ComputeBuffer _buffer;
        private int _resolution = 1024;   
        

        private void Start()
        {
            if (shader == null || spectrum == null || settings == null)
            {
                Debug.LogError("Null references on SpectrumToTexture");
                return; 
            }

            _resolution = settings.SpectrumResolution;
            _texture = new RenderTexture(_resolution, 1, 0, RenderTextureFormat.RFloat);
            _texture.enableRandomWrite = true;
            _texture.Create(); 
            
            _buffer = new ComputeBuffer(_resolution, sizeof(float));
        }

        private void Update()
        {
            _buffer.SetData(spectrum.DftBuffer.Spectrum.ToArray());
            int kernel = shader.FindKernel("CSMain");
            
            shader.SetTexture(kernel, "Result", _texture);
            shader.SetBuffer(kernel, "Spectrum", _buffer);
            
            shader.Dispatch(kernel, _resolution / 32, 1, 1);
            
            material.SetTexture("_MainTex", _texture);
        }
    }
}