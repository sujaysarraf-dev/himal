using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 10f;
    public float waypointThreshold = 2f;
    public float rotationSpeed = 5f;
    public float zebraStopDistance = 15f;
    public Transform[] zebraCrossings;
    public float carStopDistance = 5f;

    private int currentWaypointIndex = 0;
    private Rigidbody rb;
    private bool isTurning = false;
    private bool isStoppedForZebra = false;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.centerOfMass = new Vector3(0, -0.5f, 0);
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        currentSpeed = speed;

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned!");
        }
    }

    void FixedUpdate()
    {
        bool shouldStop = CheckZebraCrossing();

        if (shouldStop)
        {
            if (!isStoppedForZebra)
            {
                isStoppedForZebra = true;
                Debug.Log("Car STOPPED for zebra crossing!");
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
            return;
        }
        else
        {
            if (isStoppedForZebra)
            {
                isStoppedForZebra = false;
                Debug.Log("Car GOING - zebra clear!");
            }
        }

        // Check car in front
        float targetSpeed = speed;
        WaypointFollower[] allCars = FindObjectsOfType<WaypointFollower>();

        foreach (WaypointFollower otherCar in allCars)
        {
            if (otherCar == this) continue;

            float dist = Vector3.Distance(transform.position, otherCar.transform.position);
            Vector3 toOther = otherCar.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, toOther);

            if (angle < 30f && dist < carStopDistance)
            {
                targetSpeed = 0f;
                Debug.Log("Car STOPPED - car ahead! dist=" + dist);
                break;
            }
            else if (angle < 30f && dist < carStopDistance * 2f)
            {
                targetSpeed = Mathf.Min(targetSpeed, speed * (dist / (carStopDistance * 2f)));
            }
        }

        if (waypoints == null || waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        direction.y = 0;

        float angleToWaypoint = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        isTurning = Mathf.Abs(angleToWaypoint) > 15f;

        float actualRotationSpeed = isTurning ? rotationSpeed * 0.5f : rotationSpeed;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * actualRotationSpeed);
        }

        if (rb != null && targetSpeed > 0f)
        {
            float currentYVelocity = rb.linearVelocity.y;
            rb.linearVelocity = new Vector3(transform.forward.x * targetSpeed, currentYVelocity, transform.forward.z * targetSpeed);
        }
        else if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
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

    bool CheckZebraCrossing()
    {
        if (zebraCrossings == null || zebraCrossings.Length == 0) return false;

        foreach (Transform zebra in zebraCrossings)
        {
            if (zebra == null) continue;

            ZebraCrossing zebraScript = zebra.GetComponent<ZebraCrossing>();
            if (zebraScript == null) continue;
            if (!zebraScript.isPlayerOnZebra) continue;

            Vector3 toZebra = zebra.position - transform.position;
            toZebra.y = 0;
            float angle = Vector3.Angle(transform.forward, toZebra);

            if (angle > 45f) continue;

            float dist = Vector3.Distance(transform.position, zebra.position);
            if (dist < zebraStopDistance)
            {
                return true;
            }
        }

        return false;
    }
}