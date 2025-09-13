using UnityEngine;
using UnityEngine.Serialization;

public class SoundfxPlayer : MonoBehaviour
{
    public float startTime = 0.0f;
    private AudioClip _clip;

    private AudioSource _soundComponent;

    private void Start ()
    {
        _soundComponent = GetComponent<AudioSource>();
        _clip = _soundComponent.clip;
    }

    public void PlaySfxWithDelay()
    {
        Invoke(nameof(PlaySfx), startTime);
    }

    private void PlaySfx()
    {
        _soundComponent.PlayOneShot(_clip);
    }
}
