using Fusion;
using RootMotion.FinalIK;
using UnityEngine;

public class Avatar : NetworkBehaviour
{
    [SerializeField] VRIK vrIK;

    [SerializeField] Transform headTarget;
    [SerializeField] Transform leftArmTarget;
    [SerializeField] Transform rightArmTarget;

    [SerializeField] Transform headBone;

    [SerializeField] HitReaction hitReaction;

    [SerializeField] Transform avatarParent;
    [SerializeField] Collider[] colliders;

    [SerializeField] int nonCollisionLayer;

    private void Start()
    {
        if (HasStateAuthority)
        {
            transform.SetPositionAndRotation(Player.Instance.transform.position, Player.Instance.transform.rotation);
            headBone.localScale = Vector3.zero;
            SetLayerAllChildren(avatarParent, nonCollisionLayer);
        }
        else
        {
            var children = avatarParent.GetComponentsInChildren<BodyCollider>(includeInactive: true);
            foreach (var item in children)
            {
                Destroy(item);
            }
        }
    }

    public void ColliderHit(Collider collider, Vector3 direction, float hitForce, Vector3 position)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == collider)
            {
                RPC_HitReaction(i, direction, hitForce, position);
                return;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_HitReaction(int colliderIndex, Vector3 direction, float hitForce, Vector3 position)
    {
        hitReaction.Hit(colliders[colliderIndex], direction * hitForce, position);
    }

    private void Update()
    {
        if (HasStateAuthority)
        {
            headTarget.SetPositionAndRotation(Player.Instance.headOffset.position, Player.Instance.headOffset.rotation);
            leftArmTarget.SetPositionAndRotation(Player.Instance.lHandOffset.position, Player.Instance.lHandOffset.rotation);
            rightArmTarget.SetPositionAndRotation(Player.Instance.rHandOffset.position, Player.Instance.rHandOffset.rotation);
        }
    }

    void SetLayerAllChildren(Transform root, int layer)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }
}
