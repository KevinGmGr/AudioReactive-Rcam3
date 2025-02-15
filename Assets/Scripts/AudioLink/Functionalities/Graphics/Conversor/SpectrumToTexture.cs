using System;
using root.AudioLink.Core;
using UnityEngine;
using Unity.Collections; 
using UnityEngine.Serialization;

namespace root.AudioLink.Graphics.DataToTextures
{
    public class SpectrumToTexture : ComputeShaderFromAudio
    {
        [SerializeField] private Material material;
        private RenderTexture _texture;

        private void Start()
        {
            base.Start();
            _texture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
            _texture.enableRandomWrite = true;
            _texture.Create(); 
            
            Buffer = new ComputeBuffer(resolution, sizeof(float));
        }

        private void Update()
        {
            Buffer.SetData(spectrum.DftBuffer.Spectrum.ToArray());
            var kernel = shader.FindKernel("CSMain");
            
            shader.SetTexture(kernel, "Result", _texture);
            shader.SetBuffer(kernel, "Spectrum", Buffer);
            
            shader.Dispatch(kernel, resolution / 32, resolution / 32, 1);
            
            material.SetTexture("_MainTex", _texture);
        }
    }
}