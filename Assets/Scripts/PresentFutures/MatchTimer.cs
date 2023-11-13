using System;
using System.Collections;
using UnityEngine;

public class MatchTimer : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI timerText;

    public void StartTimer(int n, Action onFinished)
    {
        StartCoroutine(Co_Timer(n, onFinished));
    }

    IEnumerator Co_Timer(int n, Action onFinished)
    {
        while(n >= 0)
        {
            timerText.text = n.ToString();
            yield return new WaitForSeconds(1);
            n--;
        }
        onFinished?.Invoke();
    }
}
