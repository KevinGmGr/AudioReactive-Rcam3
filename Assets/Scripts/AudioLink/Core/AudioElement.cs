using UnityEngine;

namespace root.AudioLink.Core
{
    public abstract class AudioElement : MonoBehaviour
    {
        [SerializeField] protected SpectrumData spectrum;
        [SerializeField] protected AudioSettings settings;
        
        protected void Start()
        {
            if (spectrum == null || settings == null)
            {
                Debug.LogError("Null references on AudioElement from: " + name);
                return; 
            }
        }
    }
}