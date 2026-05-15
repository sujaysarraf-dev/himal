using UnityEngine;
using HealthbarGames;

public class WaypointFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 10f;
    public float waypointThreshold = 2f;
    public float rotationSpeed = 5f;
    public float zebraStopDistance = 15f;
    public Transform[] zebraCrossings;
    public TrafficLightBase[] trafficLights;
    public float lightStopDistance = 10f;
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
        bool shouldStopForZebra = CheckZebraCrossing();
        bool shouldStopForLight = CheckTrafficLights();

        if (shouldStopForZebra || shouldStopForLight)
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

        // Check car in front using raycast
        float targetSpeed = speed;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f + transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, transform.forward, out hit, carStopDistance))
        {
            if (hit.collider.GetComponent<WaypointFollower>() != null && hit.collider.gameObject != gameObject)
            {
                targetSpeed = 0f;
                Debug.Log("Car STOPPED - car ahead via raycast! dist=" + hit.distance);
            }
        }
        else if (Physics.Raycast(rayOrigin, transform.forward, out hit, carStopDistance * 2f))
        {
            if (hit.collider.GetComponent<WaypointFollower>() != null && hit.collider.gameObject != gameObject)
            {
                targetSpeed = Mathf.Min(targetSpeed, speed * (hit.distance / (carStopDistance * 2f)));
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

    bool CheckTrafficLights()
    {
        if (trafficLights == null || trafficLights.Length == 0) return false;

        foreach (TrafficLightBase light in trafficLights)
        {
            if (light == null) continue;

            // Get the state of the light
            TrafficLightBase.State state = light.GetState();
            
            // If it's Go or Blank or Malfunction, we don't stop
            if (state == TrafficLightBase.State.Go || state == TrafficLightBase.State.Blank || state == TrafficLightBase.State.YellowBlink)
                continue;

            // Check distance
            float dist = Vector3.Distance(transform.position, light.transform.position);
            if (dist < lightStopDistance)
            {
                // Check if it's in front of us
                Vector3 toLight = light.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, toLight);
                
                if (angle < 60f) // If light is in front
                {
                    return true;
                }
            }
        }

        return false;
    }
}