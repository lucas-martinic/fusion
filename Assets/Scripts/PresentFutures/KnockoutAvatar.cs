using Fusion;
using UnityEngine;

public class KnockoutAvatar : NetworkBehaviour
{
    [SerializeField] Transform[] bodyParts;
    [SerializeField] Rigidbody[] rigidBodies;
    [SerializeField] SkinnedMeshRenderer meshRenderer;
    [Networked(OnChanged = nameof(NetworkedAvatarParentIDChanged))]
    NetworkBehaviourId NetworkedAvatarParentID { get; set; }
    private Avatar avatar;

    public void SetAvatarID(NetworkBehaviourId id)
    {
        NetworkedAvatarParentID = id;
    }

    private static void NetworkedAvatarParentIDChanged(Changed<KnockoutAvatar> changed)
    {
        changed.Behaviour.SetParent();
    }

    private void SetParent()
    {
        var obj = Runner.FindObject(NetworkedAvatarParentID.Object);
        transform.parent = obj.transform;
        avatar = GetComponentInParent<Avatar>();
        MatchBodyPosition(avatar.bodyParts);
        meshRenderer.material.color = avatar.Object.StateAuthority == 0 ? Color.red : Color.blue;
        avatar.healthManager.mainRenderer.enabled = false;
        meshRenderer.enabled = true;
    }

    public void MatchBodyPosition(Transform[] _bodyParts)
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            bodyParts[i].SetPositionAndRotation(_bodyParts[i].transform.position, _bodyParts[i].transform.rotation);
        }

        DeactivateRigidbodies();
    }

    private void DeactivateRigidbodies()
    {
        foreach (var item in rigidBodies)
        {
            item.isKinematic = false;
        }
    }
}
