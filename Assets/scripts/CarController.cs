using UnityEngine;
using HealthbarGames;

public class CarController : MonoBehaviour
{
    public Transform[] wheelMeshes;
    public float speed = 10f;
    public float stopDistance = 15f;

    private Rigidbody rb;
    private ZebraCrossing detectedZebra;
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
        DetectZebraCrossing();

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

    void DetectZebraCrossing()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        Debug.Log("Raycasting forward from " + gameObject.name + " at position " + transform.position);

        if (Physics.Raycast(rayOrigin, transform.forward, out hit, stopDistance))
        {
            Debug.Log("Ray hit: " + hit.collider.name + " tag: " + hit.collider.tag);

            ZebraCrossing zebra = hit.collider.GetComponent<ZebraCrossing>();
            if (zebra == null) zebra = hit.collider.GetComponentInParent<ZebraCrossing>();

            Debug.Log("ZebraCrossing found: " + (zebra != null));

            if (zebra != null)
            {
                if (detectedZebra != zebra)
                {
                    detectedZebra = zebra;
                    Debug.Log("Car " + gameObject.name + " detected zebra: " + zebra.gameObject.name);
                }

                Debug.Log("isPedestrianCrossing: " + zebra.isPedestrianCrossing);

                if (zebra.isPedestrianCrossing)
                {
                    if (!isStopped)
                    {
                        isStopped = true;
                        Debug.Log("Car " + gameObject.name + " STOPPED");
                    }
                }
                else
                {
                    if (isStopped)
                    {
                        isStopped = false;
                        Debug.Log("Car " + gameObject.name + " GOING");
                    }
                }
            }
        }
        else
        {
            Debug.Log("Ray hit nothing");
            if (isStopped)
            {
                isStopped = false;
                Debug.Log("Car " + gameObject.name + " GOING - no zebra");
            }
            detectedZebra = null;
        }

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