using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class Controls
{
    public enum MovementControlMode
    { 
        WASD,
        Arrows,
        Joystick
    }

    static Dictionary<string, KeyCode> keyMapping;
    static string[] keyMaps = new string[3]
    {
        "Interact",
        "Jump",
        "Run"
    };
    static KeyCode[] defaults = new KeyCode[3]
    {
        KeyCode.E,
        KeyCode.Space,
        KeyCode.LeftShift
    };

    static Controls()
    {
        InitializeDictionary();
    }

    private static void InitializeDictionary()
    {
        keyMapping = new Dictionary<string, KeyCode>();
        for (int i = 0; i < keyMaps.Length; ++i)
        {
            keyMapping.Add(keyMaps[i], defaults[i]);
        }
    }

    public static void SetKeyMap(string keyMap, KeyCode key)
    {
        if (!keyMapping.ContainsKey(keyMap))
            throw new ArgumentException("Invalid KeyMap in SetKeyMap: " + keyMap);
        keyMapping[keyMap] = key;
    }

    public static bool GetButtonDown(string keyMap)
    {
        return Input.GetKeyDown(keyMapping[keyMap]);
    }

    public static bool GetButton(string keyMap)
    {
        return Input.GetKey(keyMapping[keyMap]);
    }

    public static bool GetButtonUp(string keyMap)
    {
        return Input.GetKeyUp(keyMapping[keyMap]);
    }

    public static float GetMovement(string axis)
    {
        return Input.GetAxis(axis);
    }

    public static string GetKeyName(string keyMap)
    {
        return Enum.GetName(typeof(KeyCode), keyMapping[keyMap]).ToUpper();
    }
}