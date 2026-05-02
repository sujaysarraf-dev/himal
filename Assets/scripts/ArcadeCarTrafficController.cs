using UnityEngine;
using HealthbarGames;

public class ArcadeCarTrafficController : MonoBehaviour
{
    public Transform[] waypoints;
    public TrafficLightBase[] trafficLights;
    public Transform[] zebraCrossings;

    public float motorTorque = 1500f;
    public float brakeTorque = 3000f;
    public float maxSpeed = 20f;
    public float waypointThreshold = 2f;
    public float rotationSpeed = 5f;
    public float trafficLightStopDistance = 20f;
    public float zebraStopDistance = 15f;

    private WheelCollider[] wheelColliders;
    private WheelCollider[] frontWheels;
    private WheelCollider[] rearWheels;
    private int currentWaypointIndex = 0;
    private Rigidbody rb;
    private bool isStoppedForTrafficLight = false;
    private bool isStoppedForZebra = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.centerOfMass = new Vector3(0, -0.5f, 0);
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        CacheWheelColliders();

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned to " + gameObject.name);
        }
    }

    void CacheWheelColliders()
    {
        wheelColliders = GetComponentsInChildren<WheelCollider>();

        if (wheelColliders.Length == 0)
        {
            Debug.LogError("No WheelColliders found on " + gameObject.name);
            return;
        }

        System.Collections.Generic.List<WheelCollider> front = new System.Collections.Generic.List<WheelCollider>();
        System.Collections.Generic.List<WheelCollider> rear = new System.Collections.Generic.List<WheelCollider>();

        foreach (WheelCollider wheel in wheelColliders)
        {
            string name = wheel.name.ToLower();
            if (name.Contains("front"))
            {
                front.Add(wheel);
            }
            else if (name.Contains("rear"))
            {
                rear.Add(wheel);
            }
        }

        if (front.Count == 0 || rear.Count == 0)
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                if (i < wheelColliders.Length / 2)
                    front.Add(wheelColliders[i]);
                else
                    rear.Add(wheelColliders[i]);
            }
        }

        frontWheels = front.ToArray();
        rearWheels = rear.ToArray();
    }

    void FixedUpdate()
    {
        bool shouldStopForZebra = CheckZebraCrossing();
        bool shouldStopForLight = CheckTrafficLight();

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

            ApplyBrakes();
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
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);

        float currentSpeed = rb.linearVelocity.magnitude;
        if (currentSpeed < maxSpeed)
        {
            ApplyMotorTorque();
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

    void ApplyMotorTorque()
    {
        foreach (WheelCollider wheel in rearWheels)
        {
            wheel.motorTorque = motorTorque;
            wheel.brakeTorque = 0;
        }
        foreach (WheelCollider wheel in frontWheels)
        {
            wheel.brakeTorque = 0;
        }
    }

    void ApplyBrakes()
    {
        foreach (WheelCollider wheel in wheelColliders)
        {
            wheel.motorTorque = 0;
            wheel.brakeTorque = brakeTorque;
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

            Vector3 toLight = trafficLight.transform.position - transform.position;
            toLight.y = 0;
            float angle = Vector3.Angle(transform.forward, toLight);

            if (angle > 45f) continue;

            float dist = Vector3.Distance(transform.position, trafficLight.transform.position);

            if (dist > trafficLightStopDistance) continue;

            TrafficLightBase.State lightState = trafficLight.GetState();

            if (lightState == TrafficLightBase.State.Stop || lightState == TrafficLightBase.State.PrepareToStop)
            {
                Debug.Log("Traffic light ahead! State: " + lightState + " | dist=" + dist + " | stopping");
                return true;
            }
        }

        return false;
    }
}
