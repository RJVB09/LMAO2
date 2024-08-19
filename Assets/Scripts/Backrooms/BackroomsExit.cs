using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BackroomsExit : MonoBehaviour
{
    public bool available = false;
    bool lastAvailable = true;

    public Material[] exitSignmaterials;

    public Light[] exitSignLights;

    public Collider doorBlockage;

    public PlayerTrigger triggerOpen;
    public PlayerTrigger triggerClose;

    Animator animator;

    public bool playerWentThrough = false;

    public GameObject environment;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (available != lastAvailable)
        {
            lastAvailable = available;
            foreach (Material material in exitSignmaterials)
            {
                material.SetColor("_EmissionColor", available ? Color.white : Color.black);
                doorBlockage.enabled = !available;
            }
            foreach (Light light in exitSignLights)
            {
                light.intensity = available ? 1f : 0f;
            }
        }

        if (triggerClose.touched)
        {
            playerWentThrough = true;
            //doorBlockage.enabled = true;
        }

        animator.SetBool("Open", !(!(available && triggerOpen.touched) || triggerClose.touched));

        if (triggerOpen.touched && available)
            environment.SetActive(true);
    }
}
