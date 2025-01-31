using UnityEngine;

namespace root.AudioLink.Core
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "Scriptable Objects/AudioSettings")]
    public class AudioSettings : ScriptableObject
    {
        public int Channel = 0;
        public float SpectrumRange = 60.0f;
        public int SpectrumResolution = 1024; 

    }
}
