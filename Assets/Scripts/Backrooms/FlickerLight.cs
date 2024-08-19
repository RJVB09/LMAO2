using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    public MeshRenderer lampMesh;
    public Material materialOn;
    public Material materialOff;
    Light light;
    AudioSource audioSource;
    float flickerChance = 0.5f;

    private void Start()
    {
        light = GetComponent<Light>();
        audioSource = GetComponent<AudioSource>();
        flickerChance = Random.Range(0.2f, 0.8f);

        if (Random.Range(0, 1f) < 0.97f)
            Destroy(this);
    }

    private void Update()
    {
        if (Mathf.PerlinNoise(Time.time * 3f + transform.position.x, transform.position.z) < flickerChance)
        {
            lampMesh.material = materialOn;
            light.enabled = true;
            audioSource.enabled = true;
        }
        else
        {
            lampMesh.material = materialOff;
            light.enabled = false;
            audioSource.enabled = false;
        }
    }
}
