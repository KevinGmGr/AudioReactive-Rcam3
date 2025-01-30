using JetBrains.Annotations;
using root.External.SimpleDriver;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace root.AudioLink
{
    public class SpectrumData : MonoBehaviour
    {
        [CanBeNull] [SerializeField] private AudioLinker linker;

        [SerializeField] private float range = 60; 
        MultibandFilter _filter;
        private float _bypass;
        private float _lowPass;
        private float _bandPass;
        private float _highPass;
        
        public float ByPass => _bypass;
        public float LowPass => _lowPass; 
        public float BandPass => _bandPass;
        public float HighPass => _highPass;

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
        
        void Update()
        {
            var tempFilter = new NativeArray<MultibandFilter>
                (1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var tempLevel = new NativeArray<float4>
                (1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            if (linker is null)
                return; 
            
            _filter.SetParameter(960.0f / linker.SampleRate, 0.15f);
            tempFilter[0] = _filter;

            new FilterRmsJob
            {
                Input = linker.AudioDataSlice,
                Filter = tempFilter, Output = tempLevel
            }.Run();
            
            _filter = tempFilter[0];

            // Meter scale
            var sc = math.max(0, range + tempLevel[0]) / range;

            _bypass = sc.x;
            _lowPass = sc.y;
            _bandPass = sc.z;
            _highPass = sc.w; 

            tempFilter.Dispose();
            tempLevel.Dispose();
        }
    }
}