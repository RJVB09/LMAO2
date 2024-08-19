using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(PlayerController))]
public class Concussion : MonoBehaviour
{
    float concussed = 5f; //Controls the concussion blur

    public PostProcessVolume concussionVolume;
    DepthOfField depthOfField;
    float standardConcussionValue;
    public float concussionTime = 10f;
    public bool canConcuss = true;

    PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (canConcuss && concussionVolume != null && concussionVolume.profile.HasSettings<DepthOfField>())
        {
            concussionVolume.profile.TryGetSettings(out depthOfField);
            standardConcussionValue = depthOfField.focusDistance.value;
            concussed = standardConcussionValue;
        }
        else
            canConcuss = false;
    }

    private void Update()
    {
        if (canConcuss)
        {
            if (concussed < standardConcussionValue)
                concussed += Time.deltaTime * standardConcussionValue / concussionTime;
            else
                concussed = standardConcussionValue;

            depthOfField.focusDistance.value = concussed;
        }
    }

    private void FixedUpdate()
    {
        //Debug.Log(head.position.magnitude + ", " + velocity.magnitude + ", " + acceleration.magnitude + ", " + jerk.magnitude);
        if (playerController.jerk.magnitude >= 10000 && (playerController.justTripped || playerController.tripped) && !playerController.standingUp && canConcuss)
        {
            //Debug.Log("FELL");
            concussed = 0;
        }
    }
}
