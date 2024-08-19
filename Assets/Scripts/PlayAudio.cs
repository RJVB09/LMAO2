using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayAudio : MonoBehaviour
{
    AudioSource audioSource;
    public float pitchVariation = 0;
    float originalPitch;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        originalPitch = audioSource.pitch;
    }

    public void PlaySound()
    {
        audioSource.pitch = originalPitch + Random.Range(-pitchVariation, pitchVariation);
        audioSource.Play();
    }
}
