using Fusion;
using RootMotion.FinalIK;
using System;
using System.Collections;
using UnityEngine;

public class PlayerHealthManager : NetworkBehaviour
{
    [SerializeField] VRIK ik;
    public float maxHealth, currentHealth, minDamage, maxDamage, minVelocity, maxVelocity, headshotMultiplier, respawnTime;
    [SerializeField] float regeneration;
    public bool dead;
    [SerializeField] Rigidbody[] rigidbodies;
    [SerializeField] NetworkObject knockoutAvatar;
    public Avatar mainAvatar;
    private NetworkRunner runner;
    private NetworkObject koAvatar;
    [SerializeField] Transform rootBone;
    private MatchManager matchManager;
    [SerializeField] private AnimationCurve lowHealthUICurve;

    [Networked(OnChanged = nameof(NetworkedAvatarRendererEnabledChanged))]
    bool NetworkedAvatarRendererEnabled { get; set; }

    private static void NetworkedAvatarRendererEnabledChanged(Changed<PlayerHealthManager> changed)
    {
        foreach (var item in changed.Behaviour.mainAvatar.meshRenderer)
        {
            item.enabled = changed.Behaviour.NetworkedAvatarRendererEnabled;
        }
    }

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        matchManager = FindAnyObjectByType<MatchManager>();
        NetworkedAvatarRendererEnabled = true;
    }

    private void Update()
    {
        if (currentHealth < maxHealth)
            currentHealth += regeneration * Time.deltaTime;

        var color = Player.Instance.hurtScreen.color;
        Player.Instance.hurtScreen.color = new Color(color.r, color.g, color.b, lowHealthUICurve.Evaluate((maxHealth - currentHealth) / maxHealth));
    }

    public void TakeDamage(float velocity, GameObject hitObject)
    {
        RPC_TakeDamage(velocity, hitObject.tag);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_TakeDamage(float velocity, string hitObjectTag)
    {
        float damageDone = 0;
        if (!dead)
        {
            if (hitObjectTag == "PlayerHead")
            {
                Player.Instance.damageAnimator.SetTrigger("HeadHit");

                damageDone = CalculateDamage(velocity);
                currentHealth -= damageDone;
                Debug.Log("VELOCITY: " + velocity + " DAMAGE: " + CalculateDamage(velocity) * headshotMultiplier);

                StartCoroutine(EnableHealthAnimationsAfterDelay());

            }
            else if (hitObjectTag == "Player")
            {
                Player.Instance.damageAnimator.SetTrigger("BodyHit");

                damageDone = CalculateDamage(velocity);
                currentHealth -= damageDone;
                Debug.Log("VELOCITY: " + velocity + " DAMAGE: " + CalculateDamage(velocity));

                StartCoroutine(EnableHealthAnimationsAfterDelay());
            }

            if (currentHealth <= 0)
                Death();
        }
    }

    private float CalculateDamage(float velocity)
    {
        return velocity;
    }

    private IEnumerator EnableHealthAnimationsAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // 2 seconds or put here the time the animation takes
    }

    private IEnumerator RespawnTimer()
    {
        float timer = respawnTime;
        Player.Instance.respawnScreen.gameObject.SetActive(true);
        while (dead)
        {
            Player.Instance.respawnScreen.text = "Respawning in " + timer.ToString("F2") + " seconds";
            timer -= Time.deltaTime;
            if (timer < 0)
                dead = false;
            yield return null;
        }
        RPC_Respawn();
        yield return null;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_CallDeath()
    {
        Death();
    }

    [ContextMenu("Death")]
    private void Death()
    {
        mainAvatar.PlayHitSound();
        dead = true;
        if(koAvatar == null)
        {
            koAvatar = runner.Spawn(knockoutAvatar, transform.GetChild(0).position, transform.GetChild(0).rotation, null, BeforeKOAvatarSpawned);
        }
        matchManager.RPC_PlayerKO(runner.LocalPlayer);
        NetworkedAvatarRendererEnabled = false;
        StartCoroutine(RespawnTimer());
    }

    private void BeforeKOAvatarSpawned(NetworkRunner runner, NetworkObject obj)
    {
        obj.GetComponent<KnockoutAvatar>().SetAvatarID(Id);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_Respawn()
    {
        NetworkedAvatarRendererEnabled = true;
        if(koAvatar)
            runner.Despawn(koAvatar);
        Player.Instance.respawnScreen.gameObject.SetActive(false);
        currentHealth = maxHealth / 2;
    }

    [ContextMenu("KinematicFalse")]
    private void KinematicFalse()
    {
        foreach (var item in rigidbodies)
        {
            item.velocity = Vector3.zero;
            item.angularVelocity = Vector3.zero;
            item.isKinematic = false;
        }
    }
    [ContextMenu("KinematicTrue")]
    private void KinematicTrue()
    {
        foreach (var item in rigidbodies)
        {
            item.isKinematic = true;
        }
    }
}