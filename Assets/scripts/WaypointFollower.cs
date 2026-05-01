using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 10f;
    public float waypointThreshold = 2f;
    public float rotationSpeed = 5f;

    private int currentWaypointIndex = 0;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.centerOfMass = new Vector3(0, -0.5f, 0);
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned!");
        }
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        direction.y = 0;

        float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        bool isTurning = Mathf.Abs(angle) > 15f;

        float actualRotationSpeed = isTurning ? rotationSpeed * 0.5f : rotationSpeed;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * actualRotationSpeed);
        }

        if (rb != null)
        {
            float currentYVelocity = rb.linearVelocity.y;
            rb.linearVelocity = new Vector3(transform.forward.x * speed, currentYVelocity, transform.forward.z * speed);
        }

        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                          new Vector3(targetWaypoint.position.x, 0, targetWaypoint.position.z));

        if (distance < waypointThreshold)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }
}