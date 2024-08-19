using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorExplode : MonoBehaviour
{
    public Rigidbody leftDoor;
    public Rigidbody rightDoor;

    // Start is called before the first frame update
    void Start()
    {
        Explode();
    }

    public void Explode()
    {
        leftDoor.useGravity = true;
        rightDoor.useGravity = true;
        leftDoor.AddForce(new Vector3(100, 30, -21), ForceMode.Acceleration);
        rightDoor.AddForce(new Vector3(-100, 30, 56), ForceMode.Acceleration);
        gameObject.GetComponent<AudioSource>().Play();
    }
}
