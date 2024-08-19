using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(CharacterController))]
public class Noclip : MonoBehaviour
{
    PlayerController playerController;
    CharacterController characterController;
    public AudioSource clippingAudioSource;
    bool noclipped = false;
    public float ceilingHeight;
    public AudioClip[] noclipSounds;

    public GameObject tutorialText;

    public Vector3 offset;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();

        characterController.enabled = false;
        gameObject.transform.position = Vector3.up * 60 + offset;
        characterController.enabled = true;

        playerController.allowedToStandUp = false;

        StartCoroutine(playerController.Trip(false));
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.head.position.y < ceilingHeight && !noclipped)
        {
            clippingAudioSource.clip = noclipSounds[Random.Range(0, noclipSounds.Length)];
            clippingAudioSource.Play();
            noclipped = true;
            StartCoroutine(WaitBeforeMove());
        }
        if (tutorialText != null)
            if (playerController.standingUp && tutorialText.activeSelf)
                tutorialText.SetActive(false);
    }

    IEnumerator WaitBeforeMove()
    {
        yield return new WaitForSecondsRealtime(2.2f);
        if (tutorialText != null)
            tutorialText.SetActive(true);
        playerController.GetComponent<Player>().playerUI.SetActive(true);
        playerController.allowedToStandUp = true;
    }
}
