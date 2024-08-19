using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackRoomsEntityJumpscare : MonoBehaviour
{
    public AudioSource stab;
    public AudioSource jumpscare;

    public void PlayStab()
    {
        stab.Play();
    }

    public void PlayJumpscare()
    {
        jumpscare.Play();
    }
}
