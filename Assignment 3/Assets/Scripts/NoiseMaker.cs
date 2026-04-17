using UnityEngine;
using UnityEngine.Events;

public class NoiseMaker : MonoBehaviour
{
    public UnityEvent<NoiseMaker> OnSoundTriggered;

    AudioSource source;
    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
