using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public float timeHeldFor { get; private set; } = 0f;
    public bool deleteAterInteraction;
    public float interactionTime = 1f;
    public bool interactable = true;

    [HideInInspector]
    public bool stopHold = false;


    public virtual void OnInteractDown()
    {
        timeHeldFor = 0;
    }

    public virtual void OnInteractHold()
    {
        if (!stopHold)
            timeHeldFor += Time.deltaTime;
    }

    public virtual void OnInteractUp()
    {
        timeHeldFor = 0;
        if (deleteAterInteraction)
            Destroy(this);
    }
}
