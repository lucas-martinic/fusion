using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using BNG;
using Photon.Voice.Unity;

namespace Fusion.XR.Host
{
    /**
     * 
     * Handles:
     * - connexion launch
     * - user representation spawn on connection (on the host)
     * - user despawn by the host on associated player disconnection
     * 
     **/

    public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [Header("Room configuration")]
        public GameMode mode = GameMode.AutoHostOrClient;
        public string roomName = "SampleFusionVR";
        public bool connectOnStart = false;

        [Header("Fusion settings")]
        [Tooltip("Fusion runner. Automatically created if not set")]
        public NetworkRunner runner;
        public INetworkSceneManager sceneManager;

        [Header("Local user spawner")]
        public NetworkObject userPrefab;
        public NetworkObject voiceSetup;
        [HideInInspector] public Recorder recorder;
        [HideInInspector] public Speaker speaker;

        [Header("Event")]
        public UnityEvent onWillConnect = new UnityEvent();

        // Dictionary of spawned user prefabs, to destroy them on disconnection
        private Dictionary<PlayerRef, NetworkObject> _spawnedUsers = new Dictionary<PlayerRef, NetworkObject>();

        [SerializeField] Transform[] spawnPos;

        private void Awake()
        {
            // Check if a runner exist on the same game object
            if (runner == null) runner = GetComponent<NetworkRunner>();

            // Create the Fusion runner and let it know that we will be providing user input
            if (runner == null) runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
        }

        private async void Start()
        {
            // Launch the connection at start
            if (connectOnStart) await Connect();
        }

        public async void ConnectToFusion()
        {
            await Connect();
        }

        public async Task Connect()
        {
            // Create the scene manager if it does not exist
            if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

            if (onWillConnect != null) onWillConnect.Invoke();
            // Start or join (depends on gamemode) a session with a specific name
            var args = new StartGameArgs()
            {
                GameMode = mode,
                SessionName = roomName,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = sceneManager
            };
            await runner.StartGame(args);
        }


        #region INetworkRunnerCallbacks
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            //If it's myself, spawn an avatar and the voicechat setup
            if (player == runner.LocalPlayer)
            {
                Player.Instance.transform.SetLocalPositionAndRotation
                    (spawnPos[runner.LocalPlayer.PlayerId].position,
                    spawnPos[runner.LocalPlayer.PlayerId].rotation);

                var networkPlayerObject = runner.Spawn(userPrefab);
                var obj = runner.Spawn(voiceSetup, Player.Instance.head.position, Player.Instance.head.rotation, runner.LocalPlayer);
                obj.transform.SetParent(Player.Instance.head.transform);
                recorder = obj.GetComponent<Recorder>();
                speaker = obj.GetComponent<Speaker>();
            }
        }

        #endregion

        #region Unused INetworkRunnerCallbacks 
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        #endregion
    }

}
