using UnityEngine;
using HealthbarGames;

public class CarController : MonoBehaviour
{
    public Transform[] wheelMeshes;
    public float speed = 10f;

    private Rigidbody rb;

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
}