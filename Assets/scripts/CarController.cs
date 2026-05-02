using UnityEngine;
using HealthbarGames;

public class CarController : MonoBehaviour
{
    public Transform[] wheelMeshes;
    public float speed = 10f;
    public TrafficLightBase[] trafficLights; // Traffic lights that affect this car
    public float trafficLightStopDistance = 20f; // Distance to stop before red light

    private Rigidbody rb;
    private bool isStoppedForTrafficLight = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.centerOfMass = new Vector3(0, -0.5f, 0);
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.useGravity = true;
        }

        if (wheelMeshes == null || wheelMeshes.Length == 0)
        {
            FindWheelMeshes();
        }

        Debug.Log("Car " + gameObject.name + " initialized. Speed: " + speed);
    }

    void FixedUpdate()
    {
        // Check if should stop for traffic light
        bool shouldStopForLight = CheckTrafficLight();

        if (shouldStopForLight)
        {
            if (!isStoppedForTrafficLight)
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
            if (isStoppedForTrafficLight)
            {
                isStoppedForTrafficLight = false;
                Debug.Log("Car GOING - light is green!");
            }
        }

        if (rb != null)
        {
            float currentYVelocity = rb.linearVelocity.y;
            rb.linearVelocity = new Vector3(transform.forward.x * speed, currentYVelocity, transform.forward.z * speed);
        }
        RotateWheels();
    }

    void FindWheelMeshes()
    {
        System.Collections.Generic.List<Transform> wheels = new System.Collections.Generic.List<Transform>();

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child != transform && child.GetComponent<MeshRenderer>() != null)
            {
                string name = child.name.ToLower();
                if (name.Contains("wheel"))
                {
                    wheels.Add(child);
                }
            }
        }

        wheelMeshes = wheels.ToArray();
        Debug.Log("Car " + gameObject.name + " found " + wheelMeshes.Length + " wheel meshes");
    }

    void RotateWheels()
    {
        if (wheelMeshes == null) return;

        float rotationSpeed = speed * 50f;

        foreach (Transform wheel in wheelMeshes)
        {
            if (wheel != null)
            {
                wheel.Rotate(Vector3.right * rotationSpeed * Time.fixedDeltaTime);
            }
        }
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