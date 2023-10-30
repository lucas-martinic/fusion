using Fusion;
using RootMotion.FinalIK;
using UnityEngine;

public class Avatar : NetworkBehaviour
{
    [SerializeField] VRIK vrIK;

    [SerializeField] Transform headTarget;
    public Transform leftArmTarget;
    public Transform rightArmTarget;

    [SerializeField] Transform headBone;

    [SerializeField] HitReaction hitReaction;

    [SerializeField] Transform avatarParent;
    [SerializeField] Collider[] colliders;

    [SerializeField] int nonCollisionLayer;

    private MatchManager matchManager;
    [SerializeField] AudioSource hitAudioSource;

    [SerializeField] PlayerHealthManager healthManager;

    [SerializeField] bool dontDestroyOwnBodyColliders;

    private void Start()
    {
        matchManager = FindObjectOfType<MatchManager>();

        if (HasStateAuthority)
        {
            transform.SetPositionAndRotation(Player.Instance.transform.position, Player.Instance.transform.rotation);

            //Set our own head scale to 0 so it doesn't get in the way of the camera
            headBone.localScale = Vector3.zero;

            //Set our body layer that doesn't collide with our hands
            SetLayerAllChildren(avatarParent, nonCollisionLayer);

            //We destroy our own BodyColliders if its our own avatar (we don't wanna hit ourselves)
            var children = avatarParent.GetComponentsInChildren<BodyCollider>(includeInactive: true);

            if (!dontDestroyOwnBodyColliders)
            {
                foreach (var item in children)
                {
                    Destroy(item);
                }
            }
        }
        else
        {
            //We destroy our own Knuckles if its our own avatar (we don't wanna hit ourselves)
            var children = GetComponentsInChildren<Knuckle>(includeInactive: true);
            foreach (var item in children)
            {
                Destroy(item);
            }
        }
    }

    public void ColliderHit(Collider collider, Vector3 direction, float hitForce, Vector3 position, bool receiveDamage)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            //Search for the collider that was hit
            if (colliders[i] == collider)
            {
                //Local hit reaction
                hitReaction.Hit(colliders[i], direction * hitForce, position);
                //Add one point, this can be used for different amounts in the future
                if (receiveDamage)
                {
                    //Calculate points/Health
                    var damageDone = healthManager.TakeDamage(hitForce, collider.gameObject);
                    AddPoints((int)damageDone);
                }
                //Remote hit reaction
                if(healthManager.currentHealth > 0)
                {
                    RPC_HitReaction(i, direction, hitForce, position);
                }
                return;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_HitReaction(int colliderIndex, Vector3 direction, float hitForce, Vector3 position)
    {
        hitReaction.Hit(colliders[colliderIndex], direction * hitForce, position);
        PlayHitSound(colliderIndex);
    }

    public void PlayHitSound(int colliderIndex)
    {
        hitAudioSource.transform.position = colliders[colliderIndex].transform.position;
        hitAudioSource.Play();
    }
    public void PlayHitSound()
    {
        hitAudioSource.Play();
    }

    private void AddPoints(int points)
    {
        if (matchManager.matchFinished) return;
        matchManager.AddPoints(points, Runner.LocalPlayer.PlayerId);
    }

    private void Update()
    {
        //If the avatar is ours, we update the IK targets to match the XR Rig
        if (HasStateAuthority)
        {
            headTarget.SetPositionAndRotation(Player.Instance.headOffset.position, Player.Instance.headOffset.rotation);
            leftArmTarget.SetPositionAndRotation(Player.Instance.lHandOffset.position, Player.Instance.lHandOffset.rotation);
            rightArmTarget.SetPositionAndRotation(Player.Instance.rHandOffset.position, Player.Instance.rHandOffset.rotation);
        }
    }

    //Method to set the layers of all children
    void SetLayerAllChildren(Transform root, int layer)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }
}
