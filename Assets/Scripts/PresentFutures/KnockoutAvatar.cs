using Fusion;
using UnityEngine;

public class KnockoutAvatar : NetworkBehaviour
{
    [SerializeField] Transform[] bodyParts;
    [SerializeField] Rigidbody[] rigidBodies;
    [SerializeField] SkinnedMeshRenderer meshRenderer;
    private Avatar avatar;

    public void SetParent(NetworkRunner runner, NetworkBehaviourId id, int color)
    {
        var obj = runner.FindObject(id.Object);
        transform.parent = obj.transform;
        avatar = GetComponentInParent<Avatar>();
        MatchBodyPosition(avatar.bodyParts);
        meshRenderer.material.color = color == 0 ? Color.red : Color.blue;
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
