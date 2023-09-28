using UnityEngine;

public class BodyCollider : MonoBehaviour
{
    [SerializeField] Avatar avatar;
    public Collider _collider;

    private void OnValidate()
    {
        if(avatar == null)
        {
            avatar = GetComponentInParent<Avatar>();
        }
        if(_collider == null)
        {
            _collider = GetComponent<Collider>();
        }
    }

    private float cooldown = 1;
    private bool onCooldown;
    private void OnCollisionEnter(Collision collision)
    {
        if (onCooldown) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            onCooldown = true;
            avatar.ColliderHit(_collider, -collision.contacts[0].normal, collision.impulse.magnitude, collision.contacts[0].point);
        }
    }

    private void Update()
    {
        if (onCooldown)
        {
            cooldown -= Time.deltaTime;
            if(cooldown <= 0)
            {
                onCooldown = false;
                cooldown = 1;
            }
        }
    }
}
