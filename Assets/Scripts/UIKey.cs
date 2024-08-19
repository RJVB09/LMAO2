using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UIKey : MonoBehaviour
{
    Text text;
    public string wildCard = "_";
    public string keyMap;

    private void Start()
    {
        if (TryGetComponent(out text))
            text.text = text.text.Replace(wildCard, Controls.GetKeyName(keyMap));
    }
}
