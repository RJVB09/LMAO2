using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerSoundEffects : MonoBehaviour
{
    public AudioSource windAudioSource;
    public AudioSource footstepAudioSource;
    public AudioClip wind;

    PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        windAudioSource.clip = wind;
        windAudioSource.loop = true;
        windAudioSource.volume = 0;
        windAudioSource.Play();
    }

    private void FixedUpdate()
    {
        windAudioSource.volume = 0.4f * Mathf.Pow(playerController.velocity.magnitude / playerController.runSpeed,3);
    }
}
