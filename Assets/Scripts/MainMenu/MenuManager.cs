using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public AudioClip buttonHoverAudio;
    public AudioClip buttonClickAudio;

    AudioSource buttonHoverSource;
    AudioSource buttonClickSource;

    private void Start()
    {
        if (buttonHoverAudio != null)
        {
            buttonHoverSource = gameObject.AddComponent<AudioSource>();
            buttonHoverSource.loop = false;
            buttonHoverSource.playOnAwake = false;
            buttonHoverSource.clip = buttonHoverAudio;
        }

        if (buttonClickAudio != null)
        {
            buttonClickSource = gameObject.AddComponent<AudioSource>();
            buttonClickSource.loop = false;
            buttonClickSource.playOnAwake = false;
            buttonClickSource.clip = buttonClickAudio;
        }
    }

    public void PlayHoverSound()
    {
        if (buttonHoverAudio != null)
            buttonHoverSource.Play();
    }

    public void PlayClickSound()
    {
        if (buttonClickAudio != null)
            buttonClickSource.Play();
    }

    public static void LoadScene(int index)
    {
        SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
    }

    public static void LoadScene(string name)
    {
        SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
    }
}
