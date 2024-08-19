using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Computer : InteractableObject
{
    public BackroomsManager manager;
    public BackroomEntity entity;

    public AudioSource engineTry;
    public AudioSource engineStart;
    public AudioSource engineFail;
    public AudioSource engineRun;

    public MeshRenderer PC;
    public MeshRenderer setup;

    public GameObject display;

    bool started = false;
    bool fail = false;
    bool failed = false;

    float failProgress = 0f;

    public float failChance = 1f;

    private void Start()
    {
        interactionTime = engineTry.clip.length - 0.2f;
    }

    public override void OnInteractDown()
    {
        base.OnInteractDown();
        fail = Random.Range(0f, 1f) < failChance; //wheter it is destined to fail
        failProgress = Random.Range(0.1f, 0.9f);
        failed = false;

        Debug.Log(fail + ", " + failed + ", " + failProgress);

        if (!started)
        {
            engineTry.Play();
        }
    }

    public override void OnInteractHold()
    {
        base.OnInteractHold();
        if (!started && !failed)
        {
            entity.wanderPos = manager.maze.WorldPosToMazePos(gameObject.transform.position);
            PC.material.SetColor("_EmissionColor", Color.white * 2f);
        }


        if (timeHeldFor >= interactionTime && !started && !fail)
        {
            setup.material.SetColor("_EmissionColor", Color.white * 1.4f);
            started = true;
            interactable = false;
            engineStart.Play();
            engineRun.Play();
            StartPC();
        }

        if (timeHeldFor / interactionTime >= failProgress && fail && !failed)
        {
            PC.material.SetColor("_EmissionColor", Color.black);
            engineFail.Play();
            engineTry.Stop();
            failed = true;
        }

        if (timeHeldFor / interactionTime >= failProgress && fail)
        {
            stopHold = true;
        }
    }

    public override void OnInteractUp()
    {
        base.OnInteractUp();
        if (!started && !failed)
        {
            PC.material.SetColor("_EmissionColor", Color.black);
            engineFail.Play();
            engineTry.Stop();
        }
        stopHold = false;
    }

    public void StartPC()
    {
        manager.activatedComputers += 1;
        display.SetActive(true);
    }
}
