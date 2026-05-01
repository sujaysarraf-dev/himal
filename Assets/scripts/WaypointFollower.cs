using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 10f;
    public float waypointThreshold = 2f;
    public float rotationSpeed = 5f; // Lower = smoother turns

    private int currentWaypointIndex = 0;
    private Rigidbody rb;
    private bool isTurning = false;

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
            Debug.LogError("WaypointFollower: No waypoints assigned to " + gameObject.name);
        }
        else
        {
            Debug.Log("WaypointFollower: Car " + gameObject.name + " will follow " + waypoints.Length + " waypoints");
        }
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        direction.y = 0;

        // Calculate angle to target - detect if we need to turn
        float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        isTurning = Mathf.Abs(angle) > 15f;

        // Rotate towards waypoint - slower when turning, faster on straights
        float actualRotationSpeed = isTurning ? rotationSpeed * 0.5f : rotationSpeed;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * actualRotationSpeed);
        }

        // Move forward - maintain Y velocity for gravity
        if (rb != null)
        {
            float currentYVelocity = rb.linearVelocity.y;
            rb.linearVelocity = new Vector3(transform.forward.x * speed, currentYVelocity, transform.forward.z * speed);
        }

        // Check if reached waypoint
        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                          new Vector3(targetWaypoint.position.x, 0, targetWaypoint.position.z));

        if (distance < waypointThreshold)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0; // Loop back
            }
            Debug.Log("Car " + gameObject.name + " reached waypoint " + currentWaypointIndex);
        }
    }
}
