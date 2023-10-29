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
    [SerializeField] Rigidbody[] rigidbodies;

    private void Update()
    {
        if (currentHealth < maxHealth)
            currentHealth += regeneration * Time.deltaTime;
    }

    public void TakeDamage(float velocity, Vector3 direction, GameObject hitObject)
    {
        SyncDamageRPC(velocity, direction, hitObject.tag);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void SyncDamageRPC(float velocity, Vector3 direction, string hitObjectTag)
    {
        if (!dead)
        {
            if (hitObjectTag == "PlayerHead")
            {
                Player.Instance.damageAnimator.SetTrigger("HeadHit");

                currentHealth -= CalculateDamage(velocity) * headshotMultiplier;

                Debug.Log("VELOCITY: " + velocity + " DAMAGE: " + CalculateDamage(velocity) * headshotMultiplier);

                StartCoroutine(EnableHealthAnimationsAfterDelay());
            }
            else if (hitObjectTag == "Player")
            {
                Player.Instance.damageAnimator.SetTrigger("BodyHit");

                currentHealth -= CalculateDamage(velocity); ;

                Debug.Log("VELOCITY: " + velocity + " DAMAGE: " + CalculateDamage(velocity));

                StartCoroutine(EnableHealthAnimationsAfterDelay());
            }

            else { return; }
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
        ik.enabled = false;
        KinematicFalse();
        StartCoroutine(RespawnTimer());
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Respawn()
    {
        KinematicTrue();
        ik.enabled = true;
        Player.Instance.respawnScreen.SetActive(false);
        currentHealth = maxHealth / 2;
    }

    [ContextMenu("KinematicFalse")]
    private void KinematicFalse()
    {
        foreach (var item in rigidbodies)
        {
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