using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using BNG;
public class GrabInputAuthority : NetworkBehaviour
{
    public GrabbablesInTrigger grabbablesInTrigger;
    public NetworkObject networkObject;
    public override void Spawned()
    {
        grabbablesInTrigger = GetComponent<GrabbablesInTrigger>();
    }
    public void Update()
    {
        if (grabbablesInTrigger.ClosestRemoteGrabbable != null)
        {
            networkObject = grabbablesInTrigger.ClosestRemoteGrabbable.GetComponent<NetworkObject>();
            RPC_RequestInputAuthority(Runner.LocalPlayer, networkObject);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_RequestInputAuthority(int playerRef, NetworkObject networkObject)
    {
        networkObject.AssignInputAuthority(playerRef);
    }
}
