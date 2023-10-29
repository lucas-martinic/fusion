using Fusion;
using RootMotion.FinalIK;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerHealthManager : MonoBehaviour
{
    [SerializeField] VRIK ik;
    public float maxHealth, currentHealth, minDamage, maxDamage, minVelocity, maxVelocity, headshotMultiplier, respawnTime;
    [SerializeField] float regeneration;
    public bool dead;
    [SerializeField] SkinnedMeshRenderer mainRenderer;
    [SerializeField] Rigidbody[] rigidbodies;
    [SerializeField] NetworkObject knockoutAvatar;
    [SerializeField] Avatar mainAvatar;
    private NetworkRunner runner;
    private NetworkObject koAvatar;
    [SerializeField] Transform rootBone;

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
    }

    private void Update()
    {
        if (currentHealth < maxHealth)
            currentHealth += regeneration * Time.deltaTime;
    }

    public float TakeDamage(float velocity, GameObject hitObject)
    {
        return SyncDamageRPC(velocity, hitObject.tag);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    float SyncDamageRPC(float velocity, string hitObjectTag)
    {
        float damageDone = 0;
        if (!dead)
        {
            if (hitObjectTag == "PlayerHead")
            {
                Player.Instance.damageAnimator.SetTrigger("HeadHit");

                damageDone = CalculateDamage(velocity) * headshotMultiplier;
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

            else { return damageDone; }
            if (currentHealth <= 0)
                Death();
        }
        return damageDone;
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
        Player.Instance.respawnScreen.SetActive(true);
        while (dead)
        {
            Player.Instance.respawnTimer.text = "Respawning in " + timer.ToString("F2") + " seconds";
            timer -= Time.deltaTime;
            if (timer < 0)
                dead = false;
            yield return null;
        }
        Respawn();
        yield return null;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void CallDeath()
    {
        Death();
    }

    [ContextMenu("Death")]
    private void Death()
    {
        dead = true;
        if(koAvatar == null)
        {
            koAvatar = runner.Spawn(knockoutAvatar, transform.position, transform.rotation);
            var knockoutAvatarComponent = koAvatar.gameObject.GetComponent<KnockoutAvatar>();
            knockoutAvatarComponent.MatchBodyPosition(rootBone.GetComponentsInChildren<Transform>());
        }
        mainRenderer.enabled = false;
        StartCoroutine(RespawnTimer());
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Respawn()
    {
        mainRenderer.enabled = true;
        if(koAvatar)
            runner.Despawn(koAvatar);
        Player.Instance.respawnScreen.SetActive(false);
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