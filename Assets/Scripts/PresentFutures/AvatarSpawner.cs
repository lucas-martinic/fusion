using Fusion;
using UnityEngine;

public class AvatarSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] NetworkObject avatarPrefab;
    [SerializeField] Transform[] spawnPos;

    public void PlayerJoined(PlayerRef player)
    {
        //If it's myself, spawn an avatar
        if(player == Runner.LocalPlayer)
        {
            Player.Instance.transform.SetLocalPositionAndRotation
                (spawnPos[Runner.LocalPlayer.PlayerId % spawnPos.Length].position,
                spawnPos[Runner.LocalPlayer.PlayerId % spawnPos.Length].rotation);

            Runner.Spawn(avatarPrefab);
        }
    }
}
