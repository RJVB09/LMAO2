using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldInteractable : InteractableObject
{
    public bool interactionComplete = false;
    public bool deleteAterFullInteraction;

    public override void OnInteractDown()
    {
        base.OnInteractDown();
        interactionComplete = false;
    }

    public override void OnInteractHold()
    {
        base.OnInteractHold();
        //gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(Color.white, Color.red, timeHeldFor * 2));
        if (timeHeldFor >= interactionTime)
            interactionComplete = true;
    }

    public override void OnInteractUp()
    {
        base.OnInteractUp();
        interactionComplete = false;
    }
}
