using Fusion;
using System;
using UnityEngine;

//Manages the match logic
public class MatchManager : NetworkBehaviour
{
    public enum MatchState 
    { 
        Waiting = 0,
        Round1 = 1,
        Break1 = 2,
        Round2 = 3,
        Break2 = 4,
        Round3 = 5,
        Finished = 6
    }
    [SerializeField] private MatchState matchState = MatchState.Waiting;

    [SerializeField] GameObject xrOrigin;

    [SerializeField] Transform[] spawnPositions;
    [SerializeField] Transform[] ringPositions;

    [SerializeField] TMPro.TextMeshProUGUI timeText;
    [SerializeField] TMPro.TextMeshProUGUI score1Text;
    [SerializeField] TMPro.TextMeshProUGUI score2Text;
    [SerializeField] TMPro.TextMeshProUGUI stateText;

    [SerializeField] private float roundTime = 60;

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
    [Networked(OnChanged = nameof(NetworkMatchStateChanged))]
    int networkedMatchState { get; set; }

    private void Start()
    {
        time = roundTime;
    }

    [ContextMenu("StartRound")]
    private void StartRound()
    {
        switch (matchState)
        {
            case MatchState.Waiting:
                networkedMatchState = (int)MatchState.Round1;
                MoveToRingPos();
                networkedTime = roundTime;
                break;
            case MatchState.Break1:
                networkedMatchState = (int)MatchState.Round2;
                MoveToRingPos();
                networkedTime = roundTime;
                break;
            case MatchState.Break2:
                networkedMatchState = (int)MatchState.Round3;
                MoveToRingPos();
                networkedTime = roundTime;
                break;
            default:
                break;
        }
    }

    [ContextMenu("EndRound")]
    private void EndRound()
    {
        switch (matchState)
        {
            case MatchState.Round1:
                networkedMatchState = (int)MatchState.Break1;
                MoveToSpawnPos();
                break;
            case MatchState.Round2:
                networkedMatchState = (int)MatchState.Break2;
                MoveToSpawnPos();
                break;
            case MatchState.Round3:
                networkedMatchState = (int)MatchState.Finished;
                break;
            default:
                break;
        }
    }

    public void SetMatchState(int matchState)
    {
        networkedMatchState = matchState;
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
            networkedTime = roundTime;
            networkedScore1 = 0;
            networkedScore2 = 0;
            networkedMatchState = 6;
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
            if ((int)matchState == 1 || (int)matchState == 3 || (int)matchState == 5)
            {
                if(time > 0)
                {
                    networkedTime = time -= Time.deltaTime;
                }
                else
                {
                    time = 0;
                    EndRound();
                }
            }
        }
    }

    private void MoveToSpawnPos()
    {
        xrOrigin.transform.position = spawnPositions[Runner.LocalPlayer.PlayerId].position;
    }
    private void MoveToRingPos()
    {
        xrOrigin.transform.position = ringPositions[Runner.LocalPlayer.PlayerId].position;
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
    private static void NetworkMatchStateChanged(Changed<MatchManager> changed)
    {
        changed.Behaviour.matchState = (MatchState)changed.Behaviour.networkedMatchState;
        changed.Behaviour.stateText.text = ((MatchState)changed.Behaviour.networkedMatchState).ToString();
    }
}
