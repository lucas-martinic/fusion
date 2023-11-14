using Fusion;
using System;
using System.Collections;
using UnityEngine;

public class MatchTimer : NetworkBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI timerText;
    public event Action OnFinished;

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_StartTimer(int n)
    {
        Debug.Log("Starting match timer");
        StartCoroutine(Co_Timer(n));
    }

    IEnumerator Co_Timer(int n)
    {
        while(n >= 0)
        {
            timerText.text = n.ToString();
            yield return new WaitForSeconds(1);
            n--;
        }
        timerText.text = "";
        OnFinished?.Invoke();
    }
}
