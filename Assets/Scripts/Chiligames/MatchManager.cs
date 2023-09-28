using Fusion;
using System;
using UnityEngine;

//Manages the match logic
public class MatchManager : NetworkBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI timeText;
    [SerializeField] TMPro.TextMeshProUGUI score1Text;
    [SerializeField] TMPro.TextMeshProUGUI score2Text;

    [SerializeField] private float totalTime = 180;
    private float time = 0;
    int playersOnline = 0;
    int score1 = 0;
    int score2 = 0;

    public bool matchFinished;

    [Networked(OnChanged = nameof(NetworkTimeChanged))]
    float networkedTime { get; set; }
    [Networked(OnChanged = nameof(NetworkScore1Changed))]
    int networkedScore1 { get; set; }
    [Networked(OnChanged = nameof(NetworkScore2Changed))]
    int networkedScore2 { get; set; }
    [Networked(OnChanged = nameof(NetworkMatchFinishedChanged))]
    bool networkedMatchFinished { get; set; }


    private void Start()
    {
        time = totalTime;
    }

    public void PlayerJoined()
    {
        playersOnline++;
    }

    public void PlayerLeft()
    {
        playersOnline--;
    }

    public void RestartMatch()
    {
        if (HasStateAuthority)
        {
            networkedTime = totalTime;
            networkedScore1 = 0;
            networkedScore2 = 0;
            networkedMatchFinished = false;
        }
    }

    //For score
    public void AddPoints(int amount, int player)
    {
        RPC_AddPoints(amount, player);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddPoints(int amount, int player)
    {
        if (player == 0)
        {
            networkedScore1 += amount;
        }
        else if (player == 1)
        {
            networkedScore2 += amount;
        }
    }

    private void Update()
    {
        if (HasStateAuthority)
        {
            //If 2 playsers online, match starts
            if (playersOnline == 2 && !matchFinished)
            {
                if(time > 0)
                {
                    networkedTime = time -= Time.deltaTime;
                }
                else
                {
                    time = 0;
                    matchFinished = true;
                }
            }
        }
    }

    //Update timer in the network
    private static void NetworkTimeChanged(Changed<MatchManager> changed)
    {
        changed.Behaviour.time = changed.Behaviour.networkedTime;
        changed.Behaviour.timeText.text = TimeSpan.FromSeconds(changed.Behaviour.time).ToString(@"m\:ss");
    }

    //Update scores in the network
    private static void NetworkScore1Changed(Changed<MatchManager> changed)
    {
        changed.Behaviour.score1 = changed.Behaviour.networkedScore1;
        changed.Behaviour.score1Text.text = changed.Behaviour.score1.ToString();
    }
    private static void NetworkScore2Changed(Changed<MatchManager> changed)
    {
        changed.Behaviour.score2 = changed.Behaviour.networkedScore2;
        changed.Behaviour.score2Text.text = changed.Behaviour.score2.ToString();
    }
    private static void NetworkMatchFinishedChanged(Changed<MatchManager> changed)
    {
        changed.Behaviour.matchFinished = changed.Behaviour.networkedMatchFinished;
    }
}
