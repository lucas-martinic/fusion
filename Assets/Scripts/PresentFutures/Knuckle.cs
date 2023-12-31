using UnityEngine;

public class Knuckle : MonoBehaviour
{
    [SerializeField] private float hitDistance = 0.2f;
    [SerializeField] LayerMask layerMask;
    private Transform objectTransform;
    private Vector3[] previousPositions = new Vector3[3];
    private Vector3 direction;
    private float speed;
    [SerializeField] PunchHeuristic punchHeuristc;
    [SerializeField] PlayerHealthManager healthManager;
    private BodyCollider colliderCandidate;
    private float lastDistance;
    private bool probablyGonnaHit;

    void Start()
    {
        objectTransform = transform;

        // Initialize previous positions with the current position.
        for (int i = 0; i < 3; i++)
        {
            previousPositions[i] = objectTransform.position;
        }
        cooldown = knuckleCooldown;
    }

    public float knuckleCooldown = 0.35f;
    private float cooldown = 0;
    public bool onCooldown;

    void FixedUpdate()
    {
        // Shift the previous positions array to make room for the new position.
        for (int i = previousPositions.Length - 1; i > 0; i--)
        {
            previousPositions[i] = previousPositions[i - 1];
        }

        // Store the current position in the first slot of the array.
        previousPositions[0] = objectTransform.position;

        // Calculate the direction as the average of the last 2 position changes.
        direction = (previousPositions[0] - previousPositions[1] + previousPositions[1] - previousPositions[2]).normalized;

        // Calculate the speed as the average speed of the last 2 frames.
        speed = (Vector3.Distance(previousPositions[0], previousPositions[1]) + Vector3.Distance(previousPositions[1], previousPositions[2])) / (2 * Time.deltaTime);

        if (healthManager)
            if (healthManager.dead) return;

        if (onCooldown)
        {
            cooldown -= Time.deltaTime;
            if (cooldown <= 0)
            {
                onCooldown = false;
                cooldown = knuckleCooldown;
            }
        }
        else
        {
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 0.5f, layerMask, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.TryGetComponent(out BodyCollider bodyCollider))
                {
                    colliderCandidate = bodyCollider;

                    if (lastDistance > hit.distance)
                    {
                        probablyGonnaHit = true;
                    }
                    else
                    {
                        probablyGonnaHit = false;
                    }
                    lastDistance = hit.distance;

                    if (hit.distance <= hitDistance)
                    {
                        if (bodyCollider.onCooldown) return;
                        if (bodyCollider.receiveDamage)
                        {
                            bodyCollider.Hit(direction, speed, hit.point);
                            punchHeuristc.ProcessCollision();
                        }
                        else
                        {
                            bodyCollider.Hit(direction, speed, hit.point);
                            //When blocked, knuckle gets a cooldown
                            onCooldown = true;
                        }
                    }
                }
                else
                {
                    if (probablyGonnaHit)
                    {
                        if (colliderCandidate.onCooldown) return;
                        colliderCandidate.Hit(direction, speed, hit.point);
                        punchHeuristc.ProcessCollision();
                        probablyGonnaHit = false;
                        lastDistance = 0;
                    }
                }
            }
        }

    }
}
