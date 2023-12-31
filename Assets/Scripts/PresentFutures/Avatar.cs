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
    [SerializeField] AudioSource blockAudioSource;

    public PlayerHealthManager healthManager;

    [SerializeField] bool dontDestroyOwnBodyColliders;

    public SkinnedMeshRenderer[] meshRenderer;

    public Transform[] bodyParts;
    public Transform rootPart;

    private void OnValidate()
    {
        if(bodyParts.Length == 0)
        {
            bodyParts = rootPart.GetComponentsInChildren<Transform>();
        }
    }

    private void Start()
    {
        matchManager = FindObjectOfType<MatchManager>();

        SetColor();

        if (HasStateAuthority)
        {
            transform.SetPositionAndRotation(Player.Instance.transform.position, Player.Instance.transform.rotation);

            //Set our own head scale to 0 so it doesn't get in the way of the camera
            headBone.localScale = Vector3.zero;

            //Set our body layer that doesn't collide with our hands
            SetLayerAllChildren(avatarParent, nonCollisionLayer);

            //We destroy our own BodyColliders if its our own avatar (we don't wanna hit ourselves)
            var children = avatarParent.GetComponentsInChildren<BodyCollider>(includeInactive: true);

            //For local testing
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
                if (receiveDamage)
                {
                    PlayHitSound(i, hitForce);
                    //Calculate points/Health
                    int damage = Mathf.FloorToInt(hitForce * 3);
                    healthManager.TakeDamage(damage, collider.gameObject);
                    AddPoints((int)damage);
                    //Remote hit reaction
                    if (healthManager.currentHealth > 0)
                    {
                        RPC_HitReaction(i, direction, hitForce, position);
                    }
                }
                else
                {
                    PlayBlockSound(i, hitForce);
                    RPC_BlockReaction(i, hitForce);
                }
                return;
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_HitReaction(int colliderIndex, Vector3 direction, float hitForce, Vector3 position)
    {
        hitReaction.Hit(colliders[colliderIndex], direction * hitForce, position);
        PlayHitSound(colliderIndex, hitForce);
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_BlockReaction(int colliderIndex, float hitForce)
    {
        PlayBlockSound(colliderIndex, hitForce);
    }

    public void SetColor()
    {
        if (HasStateAuthority)
        {
            foreach (var item in meshRenderer)
            {
                item.material.color = Runner.LocalPlayer == 0 ? Color.red : Color.blue;
            }
        }
        else
        {
            foreach (var item in meshRenderer)
            {
                item.material.color = Runner.LocalPlayer == 1 ? Color.red : Color.blue;
            }
        }
    }

    public void PlayHitSound(int colliderIndex, float force)
    {
        hitAudioSource.transform.position = colliders[colliderIndex].transform.position;
        hitAudioSource.volume = force;
        hitAudioSource.pitch = Random.Range(0.8f, 1.2f);
        hitAudioSource.Play();
    }
    public void PlayBlockSound(int colliderIndex, float force)
    {
        blockAudioSource.transform.position = colliders[colliderIndex].transform.position;
        blockAudioSource.volume = force;
        blockAudioSource.pitch = Random.Range(0.4f, 0.5f);
        blockAudioSource.Play();
    }
    public void PlayHitSound()
    {
        hitAudioSource.Play();
    }

    private void AddPoints(float points)
    {
        matchManager.RPC_AddPoints(points, Runner.LocalPlayer);
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
