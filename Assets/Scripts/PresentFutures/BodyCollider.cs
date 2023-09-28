using UnityEngine;

public class BodyCollider : MonoBehaviour
{
    [SerializeField] Avatar avatar;
    public Collider _collider;

    //Fill values in editor automatically
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

    private const float totalCooldown = 0.1f;
    private float cooldown = totalCooldown;
    private bool onCooldown;

    public void Hit(Vector3 direction, float speed, Vector3 position)
    {
        if (onCooldown) return;
        Debug.Log("Hit " + _collider.name);
        onCooldown = true;
        avatar.ColliderHit(_collider, direction, speed, position);
    }

    private void Update()
    {
        if (onCooldown)
        {
            cooldown -= Time.deltaTime;
            if(cooldown <= 0)
            {
                onCooldown = false;
                cooldown = totalCooldown;
            }
        }
    }
}
