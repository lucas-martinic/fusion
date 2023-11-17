using DG.Tweening;
using Fusion;
using Fusion.XR.Host;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

//Manages the match logic
public class MatchManager : NetworkBehaviour
{
    //Constants for more readability
    private const int RedPlayer = 0;
    private const int BluePlayer = 1;

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
    [SerializeField] TMPro.TextMeshProUGUI scoreRedText;
    [SerializeField] TMPro.TextMeshProUGUI scoreBlueText;
    [SerializeField] TMPro.TextMeshProUGUI stateText;

    [SerializeField] private float roundTime = 60;

    [SerializeField] InputActionReference inputAction;

    private float time = 0;
    private int playersOnline = 0;
    public bool matchFinished;
    bool stopTimer = false;

    [Networked(OnChanged = nameof(NetworkTimeChanged))]
    float NetworkedTime { get; set; }
    [Networked(OnChanged = nameof(NetworkScoreRedChanged))]
    float NetworkedScoreRed { get; set; }
    [Networked(OnChanged = nameof(NetworkScoreBlueChanged))]
    float NetworkedScoreBlue { get; set; }
    [Networked(OnChanged = nameof(KOPlayerRedChanged))]
    int NetworkedKOPlayerRed { get; set; }
    [Networked(OnChanged = nameof(KOPlayerBlueChanged))]
    int NetworkedKOPlayerBlue { get; set; }
    [Networked(OnChanged = nameof(NetworkMatchStateChanged))]
    int NetworkedMatchState { get; set; }
    int NetworkedRound1Winner { get; set; }
    int NetworkedRound2Winner { get; set; }
    int NetworkedRound3Winner { get; set; }
    [Networked(OnChanged = nameof(CurrentRoundChanged))]
    int NetworkedCurrentRound { get; set; }

    [Header("RingBounds")]
    //Ring Bounds
    [SerializeField] private Collider ringCollider;
    [SerializeField] private float maxTimeOutOfRing = 3;
    private float playerOutOfRingCounter = 0;

    [SerializeField] private int currentRound = 0;
    [SerializeField] private GameObject[] roundsUI;

    [SerializeField] private MatchTimer matchTimer;
    [SerializeField] GameObject matchFinishTextUI;

    [SerializeField] ActionBasedContinuousMoveProvider moveProvider;

    [SerializeField] MeshRenderer fadeSphere;

    private void Start()
    {
        time = roundTime;
        inputAction.action.performed += ToggleVoiceChat;
        moveProvider.moveSpeed = 0;
    }

    /// <summary>
    /// Starts the match, should be called when the two players already joined.
    /// </summary>
    [ContextMenu("StartMatch")]
    public void StartMatch()
    {
        Debug.Log("Starting Match");
        if (HasStateAuthority)
        {
            matchTimer.RPC_StartTimer(10);
            matchTimer.OnFinished += StartRound;
        }
    }

    //Listens to left controller button press to toggle voicechat MUTE
    private void ToggleVoiceChat(InputAction.CallbackContext obj)
    {
        if(connectionManager.recorder != null)
            VoiceEnabled(!connectionManager.recorder.TransmitEnabled);
    }

    /// <summary>
    /// Starts a round if possible (if in waiting or break(n) state)
    /// </summary>
    [ContextMenu("StartRound")]
    private void StartRound()
    {
        stopTimer = false;
        RPC_SetMovement(1);
        switch (matchState)
        {
            case MatchState.Waiting:
                NetworkedMatchState = (int)MatchState.Round1;
                RPC_MoveToMatchPosition();
                ResetRoundStats();
                break;
            case MatchState.Break1:
                NetworkedMatchState = (int)MatchState.Round2;
                RPC_MoveToMatchPosition();
                ResetRoundStats();
                NetworkedCurrentRound++;
                break;
            case MatchState.Break2:
                NetworkedMatchState = (int)MatchState.Round3;
                RPC_MoveToMatchPosition();
                ResetRoundStats();
                NetworkedCurrentRound++;
                break;
            default:
                break;
        }
    }

    //Reset round stats
    private void ResetRoundStats()
    {
        NetworkedTime = roundTime;
        NetworkedScoreRed = 0;
        NetworkedScoreBlue = 0;
        NetworkedKOPlayerBlue = 0;
        NetworkedKOPlayerRed = 0;
    }

    /// <summary>
    /// Finishes the current round if possible
    /// </summary>
    [ContextMenu("EndRound")]
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_EndRound()
    {
        if (HasStateAuthority)
        {
            stopTimer = true;
            RPC_SetMovement(0);
            switch (matchState)
            {
                case MatchState.Round1:
                    NetworkedMatchState = (int)MatchState.Break1;
                    RPC_MoveToCornerPosition();
                    matchTimer.RPC_StartTimer(10);
                    break;
                case MatchState.Round2:
                    NetworkedMatchState = (int)MatchState.Break2;
                    RPC_MoveToCornerPosition();
                    matchTimer.RPC_StartTimer(10);
                    break;
                case MatchState.Round3:
                    NetworkedMatchState = (int)MatchState.Finished;
                    EndMatch();
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// For counting amount of players in the room and start the match
    /// </summary>
    /// <param name="player"></param>
    public void PlayerJoined(PlayerRef player)
    {
        playersOnline++;
        if(playersOnline == 2)
        {
            //Wait for the other player to connect to send the RPC
            Invoke(nameof(StartMatch), 2);
        }
    }

    //If the other player leaves, local player wins if the match is still ongoing
    public void PlayerLeft(PlayerRef player)
    {
        if ((int)matchState < 6)
        {
            if(player != Runner.LocalPlayer)
            {
                RPC_EndMatch(Runner.LocalPlayer);
            }
        }
    }

    /// <summary>
    /// Add an amount of points to certain player
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="player"></param>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddPoints(float amount, int player)
    {
        if (player == RedPlayer)
        {
            NetworkedScoreRed += amount;
        }
        else if (player == BluePlayer)
        {
            NetworkedScoreBlue += amount;
        }
    }

    private void Update()
    {
        if (HasStateAuthority)
        {
            //If a round is ongoing, time decreases
            if ((int)matchState == 1 || (int)matchState == 3 || (int)matchState == 5)
            {
                if (stopTimer) return;

                if(time > 0)
                {
                    NetworkedTime = time -= Time.deltaTime;
                }
                else
                {
                    time = 0;
                    //If the time runs out, we determine the winner from the amount of points
                    DetermineRoundWinner();                    
                    Invoke(nameof(RPC_EndRound), 2);
                }
            }
        }

        //If a match is playing, check if within ring bounds
        if((int)matchState == 1 || (int)matchState == 3 || (int)matchState == 5)
        {
            CheckIfWithinRing();
        }
    }

    /// <summary>
    /// Check if the player is within the ring area
    /// </summary>
    private void CheckIfWithinRing()
    {
        if(!ringCollider.bounds.Contains(Player.Instance.head.position)
            && !ringCollider.bounds.Contains(Player.Instance.handL.position)
                && !ringCollider.bounds.Contains(Player.Instance.handR.position))
        {
            Player.Instance.getBackScreen.gameObject.SetActive(true);
            Player.Instance.getBackScreen.text = "Get back to boundary within " + (maxTimeOutOfRing-playerOutOfRingCounter).ToString("F2") + " seconds";

            playerOutOfRingCounter += Time.deltaTime;
            if(playerOutOfRingCounter > maxTimeOutOfRing)
            {
                playerOutOfRingCounter = 0;
                //Other player wins
                RPC_EndMatch(Runner.LocalPlayer == 0 ? 1:0);
            }
        }
        else
        {
            Player.Instance.getBackScreen.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Determine which player won the round from the amount of points
    /// </summary>
    private void DetermineRoundWinner()
    {
        if(NetworkedScoreRed > NetworkedScoreBlue)
        {
            RPC_WonRound(RedPlayer);
        }
        else if (NetworkedScoreBlue > NetworkedScoreRed) 
        {
            RPC_WonRound(BluePlayer);
        }
        else
        {
            RPC_Tie();
        }
    }

    /// <summary>
    /// Automatic
    /// </summary>
    private void ForceRoundLose()
    {
        //Other player wins
        RPC_WonRound((int)Runner.LocalPlayer == 0 ? 1:0);
        RPC_EndRound();
    }

    /// <summary>
    /// Saves who won the round and shows in the UI
    /// </summary>
    /// <param name="winner"></param>
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_WonRound(int winner)
    {
        Debug.Log("Match winner: " + winner.ToString());

        if (HasStateAuthority)
        {
            switch (currentRound)
            {
                case 0:
                    NetworkedRound1Winner = winner;
                    break;
                case 1:
                    NetworkedRound2Winner = winner;
                    break;
                case 2:
                    NetworkedRound3Winner = winner;
                    break;
                default:
                    break;
            }
        }

        roundsUI[currentRound].transform.GetChild(winner).gameObject.SetActive(true);
    }

    /// <summary>
    /// Saves if there was a Tie and shows it in the UI
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_Tie()
    {
        Debug.Log("Tie");
        if (HasInputAuthority)
        {
            switch (currentRound)
            {
                case 0:
                    NetworkedRound1Winner = -1;
                    break;
                case 1:
                    NetworkedRound2Winner = -1;
                    break;
                case 2:
                    NetworkedRound3Winner = -1;
                    break;
                default:
                    break;
            }
        }

        roundsUI[currentRound].transform.GetChild(2).gameObject.SetActive(true);
    }

    /// <summary>
    /// Set movement speed for all player
    /// </summary>
    /// <param name="f"></param>
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SetMovement(float f)
    {
        moveProvider.moveSpeed = f;
    }

    /// <summary>
    /// Calculates who was the Match winner and shows the UI elements
    /// </summary>
    private void EndMatch()
    {
        //Determine winner by number of rounds won
        int winner;
        int redPlayerPoints = NetworkedRound1Winner == RedPlayer ? 1 : 0 + NetworkedRound2Winner == RedPlayer ? 1 : 0 + NetworkedRound3Winner == RedPlayer ? 1 : 0;
        int bluePlayerPoints = NetworkedRound1Winner == BluePlayer ? 1 : 0 + NetworkedRound2Winner == BluePlayer ? 1 : 0 + NetworkedRound3Winner == BluePlayer ? 1 : 0;
        Debug.Log("RedPlayerPoints: " + redPlayerPoints.ToString());
        Debug.Log("BluePlayerPoints: " + bluePlayerPoints.ToString());
        if (redPlayerPoints > bluePlayerPoints) winner = RedPlayer;
        else if (bluePlayerPoints > redPlayerPoints) winner = BluePlayer;
        //Tie
        else winner = 2;
        RPC_EndMatch(winner);
    }
    //End match with a winner
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_EndMatch(int winner)
    {
        stopTimer = true;
        matchFinishTextUI.transform.GetChild(winner).gameObject.SetActive(true);
        PlayerWins(winner);
    }

    /// <summary>
    /// Called after the match ends if one of the two players won, can be used to save stats and win statistics
    /// </summary>
    /// <param name="winner"></param>
    private void PlayerWins(int winner)
    {
        switch (winner)
        {
            case RedPlayer:
                Debug.Log("Red player wins");
                break;
            case BluePlayer:
                Debug.Log("Blue player wins");
                break;
            default:
                break;
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

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_MoveToCornerPosition()
    {
        SphereFadeOutIn(() => xrOrigin.transform.position = spawnPositions[Runner.LocalPlayer].position);
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_MoveToMatchPosition()
    {
        SphereFadeOutIn(() => xrOrigin.transform.position = ringPositions[Runner.LocalPlayer].position);
    }

    //Update timer in the network
    private static void NetworkTimeChanged(Changed<MatchManager> changed)
    {
        changed.Behaviour.time = changed.Behaviour.NetworkedTime;
        changed.Behaviour.timeText.text = TimeSpan.FromSeconds(changed.Behaviour.time).ToString(@"m\:ss");
    }

    //Update scores in the network
    private static void NetworkScoreRedChanged(Changed<MatchManager> changed)
    {
        changed.Behaviour.scoreRedText.text = changed.Behaviour.NetworkedScoreRed.ToString();
    }
    private static void NetworkScoreBlueChanged(Changed<MatchManager> changed)
    {
        changed.Behaviour.scoreBlueText.text = changed.Behaviour.NetworkedScoreBlue.ToString();
    }
    private static void NetworkMatchStateChanged(Changed<MatchManager> changed)
    {
        changed.Behaviour.matchState = (MatchState)changed.Behaviour.NetworkedMatchState;
        changed.Behaviour.stateText.text = ((MatchState)changed.Behaviour.NetworkedMatchState).ToString();
    }
    private static void CurrentRoundChanged(Changed<MatchManager> changed)
    {
        changed.Behaviour.currentRound = changed.Behaviour.NetworkedCurrentRound;
    }
    private static void KOPlayerRedChanged(Changed<MatchManager> changed)
    {
        //If 3 red KO's, blue player wins
        if (changed.Behaviour.NetworkedKOPlayerRed == 3 && changed.Behaviour.HasStateAuthority)
        {
            changed.Behaviour.RPC_WonRound(BluePlayer);
            changed.Behaviour.RPC_EndRound();
        }
    }
    private static void KOPlayerBlueChanged(Changed<MatchManager> changed)
    {
        //If 3 blue KO's, red player wins
        if (changed.Behaviour.NetworkedKOPlayerBlue == 3 && changed.Behaviour.HasStateAuthority)
        {
            changed.Behaviour.RPC_WonRound(RedPlayer);
            changed.Behaviour.RPC_EndRound();
        }
    }
    public void PlayerKO()
    {
        if(Runner.LocalPlayer == RedPlayer)
        {
            NetworkedKOPlayerRed++;
        }
        else if(Runner.LocalPlayer == BluePlayer)
        {
            NetworkedKOPlayerBlue++;
        }
    }

    //To control the black fadein/out sphere attached to head/camera
    public void SphereFadein(float time)
    {
        fadeSphere.material.DOFade(0, time).OnComplete(() => fadeSphere.enabled = false);
    }
    public void SphereFadeout(float time)
    {
        fadeSphere.enabled = true;
        fadeSphere.material.DOFade(1, time);
    }
    public void SphereFadeOutIn(Action onFadedOut)
    {
        fadeSphere.enabled = true;
        fadeSphere.material.DOFade(1, 0.5f).OnComplete(() =>
        {
            onFadedOut?.Invoke();
            SphereFadein(0.5f);
        });
    }

    //Only for testing purposes
    #region Testing
    [ContextMenu("RedWinRound")]
    private void RedWinRound()
    {
        RPC_WonRound(RedPlayer);
    }
    [ContextMenu("RedLostRound")]
    private void RedLostRound()
    {
        RPC_WonRound(BluePlayer);
    }
    #endregion
}
