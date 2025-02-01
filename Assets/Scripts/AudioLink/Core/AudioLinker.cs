using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using root.External.SimpleDriver;
using Unity.Collections;
using UnityEngine;

namespace root.AudioLink.Core
{
    public class AudioLinker : MonoBehaviour
    {
        [CanBeNull] public AudioSettings audioSettings;
        public int SampleRate => _stream?.SampleRate ?? 0;
        public float Volume { get; set; } = 1;
        public ReadOnlySpan<float> AudioDataSpan =>
                _audioData.GetSubArray(0, _audioDataFilled).GetReadOnlySpan();
        
        public NativeSlice<float> AudioDataSlice =>
            new NativeSlice<float>(_audioData, 0, _audioDataFilled);

        InputStream _stream;
        NativeArray<float> _audioData;
        int _audioDataFilled;

        private void Start()
        {
            _audioData = new NativeArray<float>(4096, Allocator.Persistent);
            
            if (audioSettings != null) 
                SetInputChannel(audioSettings.Channel);
        }

        private void Update()
        {
            if (_stream == null)
            {
                _audioDataFilled = 0;
                return;
            }
            
            var input = MemoryMarshal.Cast<byte, float>(_stream.LastFrameWindow);
            var stride = _stream.ChannelCount;
            
            if (audioSettings == null) 
                return;
            
            var offset = audioSettings.Channel;

            _audioDataFilled = Mathf.Min(input.Length, input.Length / stride);

            for (var i = 0; i < _audioDataFilled; i++)
                _audioData[i] = input[i * stride + offset] * Volume;
        }

        private void OnDestroy()
        {
            _stream?.Dispose();
            _audioData.Dispose();
        }

        private void SetInputChannel(int index)
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null; 
            }

            try
            {
                _stream = DeviceDriver.OpenInputStream(index);
                Debug.Log("Current Audio Device: " + DeviceDriver.GetDeviceName(index));
            }
            catch (System.InvalidOperationException e)
            {
                Debug.LogError(e);
                return; 
            }
        }
    }
}
