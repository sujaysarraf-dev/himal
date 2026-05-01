using UnityEngine;

public class CarController : MonoBehaviour
{
    public Transform[] wheelMeshes;
    public float speed = 10f;
    public float stopDistance = 20f;

    private Rigidbody rb;
    private SimpleTrafficLight detectedLight;
    private bool isStopped = false;

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
        DetectTrafficLight();

        if (!isStopped)
        {
            if (rb != null)
            {
                float currentYVelocity = rb.linearVelocity.y;
                rb.linearVelocity = new Vector3(transform.forward.x * speed, currentYVelocity, transform.forward.z * speed);
            }
            RotateWheels();
        }
        else
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
    }

    void DetectTrafficLight()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayOrigin, transform.forward, out hit, stopDistance))
        {
            SimpleTrafficLight light = hit.collider.GetComponentInParent<SimpleTrafficLight>();

            if (light != null)
            {
                if (detectedLight != light)
                {
                    detectedLight = light;
                    Debug.Log("Car " + gameObject.name + " detected traffic light: " + light.gameObject.name + " (State: " + light.GetState() + ")");
                }

                if (light.IsRed())
                {
                    if (!isStopped)
                    {
                        isStopped = true;
                        Debug.Log("Car " + gameObject.name + " STOPPED at RED light");
                    }
                }
                else
                {
                    if (isStopped)
                    {
                        isStopped = false;
                        Debug.Log("Car " + gameObject.name + " GOING - light is " + light.GetState());
                    }
                }
            }
        }
        else
        {
            if (isStopped)
            {
                isStopped = false;
                Debug.Log("Car " + gameObject.name + " GOING - no light detected");
            }
            detectedLight = null;
        }

        // Draw debug ray
        Debug.DrawRay(rayOrigin, transform.forward * stopDistance, isStopped ? Color.red : Color.green);
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
}
