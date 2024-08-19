using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class RandomDeathMessage : MonoBehaviour
{
    public string[] messages;
    Text text;

    private void Start()
    {
        text = GetComponent<Text>();
        if (messages.Length != 0)
            text.text = messages[Random.Range(0, messages.Length)];
    }


}
