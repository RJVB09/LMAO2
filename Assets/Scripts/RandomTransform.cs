using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTransform : MonoBehaviour
{
    public bool positionLocal = true;
    public bool rotationLocal = true;

    public bool randomPosition = true;
    public bool randomRotation = true;

    public Vector3 positionMax;
    public Vector3 positionMin;

    public Vector3 rotationMax;
    public Vector3 rotationMin;
    void Start()
    {
        if (randomPosition)
            if (positionLocal)
                transform.localPosition = new Vector3(Random.Range(positionMin.x, positionMax.x), Random.Range(positionMin.y, positionMax.y), Random.Range(positionMin.z, positionMax.z));
            else
                transform.position = new Vector3(Random.Range(positionMin.x, positionMax.x), Random.Range(positionMin.y, positionMax.y), Random.Range(positionMin.z, positionMax.z));

        if (randomRotation)
            if (rotationLocal)
                transform.localRotation = Quaternion.Euler(new Vector3(Random.Range(rotationMin.x, rotationMax.x), Random.Range(rotationMin.y, rotationMax.y), Random.Range(rotationMin.z, rotationMax.z)));
            else
                transform.rotation = Quaternion.Euler(new Vector3(Random.Range(rotationMin.x, rotationMax.x), Random.Range(rotationMin.y, rotationMax.y), Random.Range(rotationMin.z, rotationMax.z)));

        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
