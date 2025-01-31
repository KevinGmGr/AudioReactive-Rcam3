using JetBrains.Annotations;
using root.External.SimpleDriver;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace root.AudioLink.Core
{
    public class SpectrumData : MonoBehaviour
    {
        [CanBeNull] [SerializeField] private AudioLinker linker;
        [SerializeField] [CanBeNull] private AudioSettings audioSettings; 
        MultibandFilter _filter;
        private float _bypass;
        private float _lowPass;
        private float _bandPass;
        private float _highPass;
        [CanBeNull] private DftBuffer _dftBuffer;

        public DftBuffer DftBuffer => _dftBuffer; 
        private float ByPass => _bypass;
        private float LowPass => _lowPass; 
        private float BandPass => _bandPass;
        private float HighPass => _highPass;

        [Unity.Burst.BurstCompile(CompileSynchronously = true)]
        struct FilterRmsJob : IJob
        {
            [ReadOnly] public NativeSlice<float> Input;
            [WriteOnly] public NativeArray<float4> Output;
            public NativeArray<MultibandFilter> Filter;
            
            public void Execute()
            {
                var filter = Filter[0];

                var ss = float4.zero;

                foreach (var t in Input)
                {
                    var vf = filter.FeedSample(t);
                    ss += vf * vf;
                }

                var rms = math.sqrt(ss / Input.Length);

                const float refLevel = 0.7071f;
                const float zeroOffset = 1.5849e-13f;
                var level = 20 * math.log10(rms / refLevel + zeroOffset);

                Output[0] = level;
                Filter[0] = filter;
            }
        }

        private void Start()
        {
            if (_dftBuffer != null || audioSettings == null)
                return;

            _dftBuffer = new DftBuffer(audioSettings.SpectrumResolution);
        }

        private void OnDestroy()
        {
            if (_dftBuffer != null) 
                _dftBuffer.Dispose();
        }

        private void Update()
        {
            var tempFilter = new NativeArray<MultibandFilter>
                (1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var tempLevel = new NativeArray<float4>
                (1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            if (linker is null)
                return;

            if (_dftBuffer is null)
                return; 
            
            _dftBuffer.Push(linker.AudioDataSpan);
            _dftBuffer.Analyze();
            
            _filter.SetParameter(960.0f / linker.SampleRate, 0.15f);
            tempFilter[0] = _filter;

            new FilterRmsJob
            {
                Input = linker.AudioDataSlice,
                Filter = tempFilter, Output = tempLevel
            }.Run();
            
            _filter = tempFilter[0];

            if (audioSettings == null)
                return; 
            
            var sc = math.max(0, audioSettings.SpectrumRange + tempLevel[0]) / audioSettings.SpectrumRange;

            _bypass = sc.x;
            _lowPass = sc.y;
            _bandPass = sc.z;
            _highPass = sc.w;

            tempFilter.Dispose();
            tempLevel.Dispose();
        }
    }
}