using System;
using System.Runtime.InteropServices;
using root.External.SimpleDriver;
using Unity.Collections;
using UnityEngine;

namespace root.AudioLink
{
    public sealed class AudioLinker : MonoBehaviour
    {
        private int _channel = 1;

        public int Channel
        {
            get => _channel;
            set
            {
                if (value == _channel) return;
                
                _channel = value; 
                SelectInputChannel(_channel);
            }
        }
        
        public int SampleRate => _stream?.SampleRate ?? 0;
        public float Volume { get; set; } = 1;
        public ReadOnlySpan<float> AudioDataSpan =>
            _audioData.AsSpan().Slice(0, _audioDataFilled);
        public NativeSlice<float> AudioDataSlice =>
            new NativeSlice<float>(_audioData, 0, _audioDataFilled);

        InputStream _stream;
        NativeArray<float> _audioData;
        int _audioDataFilled;

        private void Start() =>_audioData = new NativeArray<float>(4096, Allocator.Persistent);

        private void Update()
        {
            if (_stream == null)
            {
                _audioDataFilled = 0;
                return;
            }
            
            var input = MemoryMarshal.Cast<byte, float>(_stream.LastFrameWindow);
            var stride = _stream.ChannelCount;
            var offset = Channel;

            _audioDataFilled = Mathf.Min(input.Length, input.Length / stride);

            for (var i = 0; i < _audioDataFilled; i++)
                _audioData[i] = input[i * stride + offset] * Volume;
        }

        void OnDestroy()
        {
            _stream?.Dispose();
            _audioData.Dispose();
        }

        
        void SelectInputChannel(int index)
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null; 
            }

            try
            {
                _stream = DeviceDriver.OpenInputStream(index);
            }
            catch (System.InvalidOperationException e)
            {
                Debug.LogError(e);
                return; 
            }
            
            
        }
    }
}
