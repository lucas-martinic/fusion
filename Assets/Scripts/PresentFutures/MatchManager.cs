using Fusion;
using Fusion.XR.Host;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] ConnectionManager connectionManager;

    [SerializeField] Transform[] spawnPositions;
    [SerializeField] Transform[] ringPositions;

    [SerializeField] TMPro.TextMeshProUGUI timeText;
    [SerializeField] TMPro.TextMeshProUGUI score1Text;
    [SerializeField] TMPro.TextMeshProUGUI score2Text;
    [SerializeField] TMPro.TextMeshProUGUI stateText;

    [SerializeField] private float roundTime = 60;

    [SerializeField] InputActionReference inputAction;

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
    [Networked(OnChanged = nameof(NetworkPlayer1RoundsChanged))]
    int networkPlayer1RoundsWon { get; set; }
    [Networked(OnChanged = nameof(NetworkPlayer2RoundsChanged))]
    int networkPlayer2RoundsWon { get; set; }

    [Header("RingBounds")]
    //Ring Bounds
    [SerializeField] private Collider ringCollider;
    [SerializeField] private float maxTimeOutOfRing = 3;
    private float playerOutOfRingCounter = 0;

    private void Start()
    {
        time = roundTime;
        inputAction.action.performed += ToggleVoiceChat;
    }

    private void ToggleVoiceChat(InputAction.CallbackContext obj)
    {
        if(connectionManager.recorder != null)
            VoiceEnabled(!connectionManager.recorder.TransmitEnabled);
    }

    [ContextMenu("StartRound")]
    private void StartRound()
    {
        switch (matchState)
        {
            case MatchState.Waiting:
                networkedMatchState = (int)MatchState.Round1;
                MoveToMatchPosition();
                networkedTime = roundTime;
                break;
            case MatchState.Break1:
                networkedMatchState = (int)MatchState.Round2;
                MoveToMatchPosition();
                networkedTime = roundTime;
                break;
            case MatchState.Break2:
                networkedMatchState = (int)MatchState.Round3;
                MoveToMatchPosition();
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
                MoveToCornerPosition();
                break;
            case MatchState.Round2:
                networkedMatchState = (int)MatchState.Break2;
                MoveToCornerPosition();
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

        //If a match is playing, check if within ring bounds
        if((int)matchState == 1 || (int)matchState == 3 || (int)matchState == 5)
        {
            CheckIfWithinRing();
        }
    }

    private void CheckIfWithinRing()
    {
        if(!ringCollider.bounds.Contains(Player.Instance.head.position)
            && !ringCollider.bounds.Contains(Player.Instance.handL.position)
                && !ringCollider.bounds.Contains(Player.Instance.handR.position))
        {
            playerOutOfRingCounter += Time.deltaTime;
            if(playerOutOfRingCounter > maxTimeOutOfRing)
            {
                EndRound();
                Disqualified();
            }
        }
    }

    private void Disqualified()
    {
        RPC_LostRound(Runner.LocalPlayer.PlayerId);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_WonRound(int player)
    {
        if (player == 0)
        {

        }
        else if (player == 1)
        {

        }
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_LostRound(int player)
    {
        if (player == 0)
        {

        }
        else if (player == 1)
        {

        }
    }

    //Enable/disable voice chat
    private void VoiceEnabled(bool enabled)
    {
        connectionManager.recorder.TransmitEnabled = enabled;
        RPC_Mute(enabled);
    }
    [Rpc(RpcSources.All, RpcTargets.Proxies)]
    private void RPC_Mute(bool enabled)
    {
        connectionManager.speaker.enabled = enabled;
    }

    private void MoveToCornerPosition()
    {
        xrOrigin.transform.position = spawnPositions[Runner.LocalPlayer.PlayerId].position;
    }
    private void MoveToMatchPosition()
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
    private static void NetworkPlayer1RoundsChanged(Changed<MatchManager> changed)
    {

    }
    private static void NetworkPlayer2RoundsChanged(Changed<MatchManager> changed)
    {

    }
}
