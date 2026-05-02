using UnityEngine;
using HealthbarGames;

public class WaypointFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform[] zebraCrossings; // Drag zebra prefabs here
    public TrafficLightBase[] trafficLights; // Traffic lights that affect this car
    public float speed = 10f;
    public float waypointThreshold = 2f;
    public float rotationSpeed = 5f;
    public float zebraStopDistance = 15f; // Distance to stop before zebra
    public float trafficLightStopDistance = 20f; // Distance to stop before red light

    private int currentWaypointIndex = 0;
    private Rigidbody rb;
    private bool isTurning = false;
    private bool isStoppedForZebra = false;
    private bool isStoppedForTrafficLight = false;

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

        if (zebraCrossings == null || zebraCrossings.Length == 0)
        {
            Debug.LogWarning("No zebra crossings assigned - car won't stop for zebras!");
        }
    }

    void FixedUpdate()
    {
        // Check if should stop for zebra
        bool shouldStopForZebra = CheckZebraCrossing();

        // Check if should stop for traffic light
        bool shouldStopForLight = CheckTrafficLight();

        // Stop if either zebra or traffic light requires stopping
        if (shouldStopForZebra || shouldStopForLight)
        {
            if (shouldStopForZebra && !isStoppedForZebra)
            {
                isStoppedForZebra = true;
                Debug.Log("Car STOPPED for zebra crossing!");
            }

            if (shouldStopForLight && !isStoppedForTrafficLight)
            {
                isStoppedForTrafficLight = true;
                Debug.Log("Car STOPPED for red light!");
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

            if (isStoppedForTrafficLight)
            {
                isStoppedForTrafficLight = false;
                Debug.Log("Car GOING - light is green!");
            }
        }

        if (waypoints == null || waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        direction.y = 0;

        float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        isTurning = Mathf.Abs(angle) > 15f;

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

    bool CheckZebraCrossing()
    {
        if (zebraCrossings == null || zebraCrossings.Length == 0) return false;

        foreach (Transform zebra in zebraCrossings)
        {
            if (zebra == null) continue;

            ZebraCrossing zebraScript = zebra.GetComponent<ZebraCrossing>();
            if (zebraScript == null) continue;

            // Check if player is on this zebra
            if (!zebraScript.isPlayerOnZebra) continue;

            // Check if zebra is ahead of car (within 45 degree angle)
            Vector3 toZebra = zebra.position - transform.position;
            toZebra.y = 0;
            float angle = Vector3.Angle(transform.forward, toZebra);
            
            if (angle > 45f) continue; // Zebra is behind or to the side

            // Check distance
            float dist = Vector3.Distance(transform.position, zebra.position);
            
            if (dist < zebraStopDistance)
            {
                Debug.Log("Zebra ahead! dist=" + dist + " | stopping");
                return true;
            }
        }

        return false;
    }

    bool CheckTrafficLight()
    {
        if (trafficLights == null || trafficLights.Length == 0) return false;

        foreach (TrafficLightBase trafficLight in trafficLights)
        {
            if (trafficLight == null) continue;

            // Check if traffic light is ahead of car (within 45 degree angle)
            Vector3 toLight = trafficLight.transform.position - transform.position;
            toLight.y = 0;
            float angle = Vector3.Angle(transform.forward, toLight);

            if (angle > 45f) continue; // Light is behind or to the side

            // Check distance
            float dist = Vector3.Distance(transform.position, trafficLight.transform.position);

            if (dist > trafficLightStopDistance) continue; // Light is too far

            // Get current state of traffic light
            TrafficLightBase.State lightState = trafficLight.GetState();

            // Stop for red light (Stop) or yellow light (PrepareToStop)
            if (lightState == TrafficLightBase.State.Stop || lightState == TrafficLightBase.State.PrepareToStop)
            {
                Debug.Log("Traffic light ahead! State: " + lightState + " | dist=" + dist + " | stopping");
                return true;
            }
        }

        return false;
    }
}