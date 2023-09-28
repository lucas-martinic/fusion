using UnityEngine;

public class Knuckle : MonoBehaviour
{
    [SerializeField] private float hitDistance = 0.2f;
    [SerializeField] LayerMask layerMask;
    private Transform objectTransform;
    private Vector3[] previousPositions = new Vector3[3];
    private Vector3 direction;
    private float speed;

    void Start()
    {
        objectTransform = transform;

        // Initialize previous positions with the current position.
        for (int i = 0; i < 3; i++)
        {
            previousPositions[i] = objectTransform.position;
        }
    }

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

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 0.25f, layerMask, QueryTriggerInteraction.Collide))
        {
            if (hit.distance <= hitDistance)
            {
                if (hit.collider.TryGetComponent(out BodyCollider bodyCollider))
                {
                    bodyCollider.Hit(direction, speed, hit.point);
                }
            }
        }
    }
}
