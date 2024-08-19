using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchSceneButton : InteractableObject
{
    bool switched;

    public override void OnInteractHold()
    {
        base.OnInteractHold();

        if (timeHeldFor >= interactionTime && !switched)
        {
            SceneManager.LoadScene(0);
            switched = true;
        }
    }
}
