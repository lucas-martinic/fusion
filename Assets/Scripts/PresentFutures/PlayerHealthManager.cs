using Fusion;
using RootMotion.FinalIK;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerHealthManager : MonoBehaviour
{
    [SerializeField] VRIK ik;
    public float maxHealth, currentHealth, minDamage, maxDamage, minVelocity, maxVelocity, headshotMultiplier, respawnTime;
    [SerializeField] private GameObject hurtScreen, mildHurtScreen;
    [SerializeField] float regeneration;

    [SerializeField] private Animator damageAnimator;
    [SerializeField] private GameObject respawnScreen;
    //private GameObject deadBody;
    [SerializeField] private TextMeshProUGUI respawnTimer;
    public bool dead;

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
                damageAnimator.SetTrigger("HeadHit");

                currentHealth -= CalculateDamage(velocity) * headshotMultiplier;

                Debug.Log("VELOCITY: " + velocity + " DAMAGE: " + CalculateDamage(velocity) * headshotMultiplier);

                StartCoroutine(EnableHealthAnimationsAfterDelay());
            }
            else if (hitObjectTag == "Player")
            {
                damageAnimator.SetTrigger("BodyHit");

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
        respawnScreen.SetActive(true);
        while (dead)
        {
            respawnTimer.text = "Respawning in " + timer.ToString("F2") + " seconds";
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

    private void Death()
    {
        dead = true;
        /*ik.gameObject.SetActive(false);
        deadBody = Instantiate(ik.gameObject, ik.transform.position, ik.transform.rotation);
        deadBody.SetActive(true);

        /*foreach (Rigidbody rb in deadBody.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = false;
        deadBody.GetComponent<Animator>().enabled = false;
        deadBody.GetComponent<VRIK>().enabled = false;
        */
        StartCoroutine(RespawnTimer());
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Respawn()
    {
        /*Destroy(deadBody);
        deadBody = null;*/
        respawnScreen.SetActive(false);
        ik.gameObject.SetActive(true);

        currentHealth = maxHealth / 2;
    }
}