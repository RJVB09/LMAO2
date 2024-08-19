using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour
{
    PlayerController playerController;
    public GameObject deathScreen;
    public GameObject playerUI;

    public bool Dead { get; private set; } = false;
    public bool DeathScreen { get; private set; } = false;
    public bool Retry { get; private set; } = false;
    public bool Exit { get; set; } = false;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Retry && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (Exit && Input.GetKeyDown(KeyCode.Tab))
        {
            SceneManager.LoadScene(0);
        }
    }

    public void OnDeath()
    { 
        //wat
    }

    public void Kill()
    {
        Dead = true;
        playerController.canMove = false;
        playerUI.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public IEnumerator ShowDeathScreen()
    {
        deathScreen.SetActive(true);
        DeathScreen = true;
        yield return new WaitForSecondsRealtime(2f);
        Retry = true;
        yield return new WaitForSeconds(1f);
        Exit = true;
    }
}
