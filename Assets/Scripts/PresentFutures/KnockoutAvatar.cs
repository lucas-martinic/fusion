
using UnityEngine;

public class KnockoutAvatar : MonoBehaviour
{
    [SerializeField] Transform[] bodyParts;
    [SerializeField] Rigidbody[] rigidBodies;

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
