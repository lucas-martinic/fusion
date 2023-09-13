using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsolToUI : MonoBehaviour
{
    public Text logText;

    void OnEnable()
    {
        Application.logMessageReceived += LogCallback;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }

    void LogCallback(string logString, string stackTrace, LogType type)
    {
        logText.text = logString;
        //Or Append the log to the old one
        //logText.text += logString + "\r\n";
    }
}
