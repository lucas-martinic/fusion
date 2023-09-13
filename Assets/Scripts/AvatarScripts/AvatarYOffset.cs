using UnityEngine;
using Fusion;
using Fusion.XR.Host.Rig;
public class AvatarYOffset : NetworkBehaviour
{
    public NetworkRig networkRig;
    
    [Networked]
    public float networkedYOffset { get; set; }
    [Networked]
    public float networkFlooroffset { get; set; }

    public bool doOffset = true;

    public override void FixedUpdateNetwork()
    {
        if(networkRig.transform.parent != null)
        {
            doOffset = false;
        }

        if (GetInput(out RigInput rigInput))
        {
            networkedYOffset = rigInput.networkYoffsetBounds;
            networkFlooroffset = rigInput.networkFloorOffset;
        }

        if (doOffset)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, networkFlooroffset - networkedYOffset, transform.localPosition.z);
        }
    }
}
