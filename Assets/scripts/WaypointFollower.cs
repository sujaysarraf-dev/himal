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

    [Header("Ambulance Mode")]
    public bool isAmbulance;
    public float overtakeSideDistance = 4f;
    public float overtakeForwardDistance = 12f;

    private int currentWaypointIndex = 0;
    private bool movingForward = true;
    private Rigidbody rb;
    private bool isTurning = false;
    private bool isStoppedForZebra = false;
    private bool isStoppedForLight = false;
    private float currentSpeed;
    private float stuckTimer = 0f;
    private float maxWaitTime = 8f;
    private bool isOvertaking = false;
    private Vector3 overtakeTarget;
    private float overtakeTimer = 0f;
    private float overtakeDuration = 3f;
    private bool ambulanceRotated = false;

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
        bool shouldStopForZebra = !isAmbulance && CheckZebraCrossing();
        bool shouldStopForLight = !isAmbulance && CheckTrafficLights();

        if (shouldStopForZebra || shouldStopForLight)
        {
            stuckTimer += Time.fixedDeltaTime;
            
            if (shouldStopForLight && !isStoppedForLight)
            {
                isStoppedForLight = true;
                Debug.Log("Car STOPPED for traffic light!");
            }
            else if (shouldStopForZebra && !isStoppedForZebra)
            {
                isStoppedForZebra = true;
                Debug.Log("Car STOPPED for zebra crossing!");
            }

            if (stuckTimer > maxWaitTime)
            {
                Debug.LogWarning($"Car {name} stuck too long at stop - forcing move!");
                stuckTimer = 0f;
                isStoppedForZebra = false;
                isStoppedForLight = false;
            }
            else
            {
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                }
                return;
            }
        }
        else
        {
            stuckTimer = 0f;
            if (isStoppedForZebra)
            {
                isStoppedForZebra = false;
                Debug.Log("Car GOING - zebra clear!");
            }
            if (isStoppedForLight)
            {
                isStoppedForLight = false;
                Debug.Log("Car GOING - traffic light clear!");
            }
        }

        if (waypoints == null || waypoints.Length == 0) return;

        // --- Ambulance overtaking ---
        if (isAmbulance)
        {
            HandleAmbulanceOvertaking();
            return;
        }

        // --- Normal car follow ---
        HandleNormalDriving();
    }

    void HandleAmbulanceOvertaking()
    {
        if (!ambulanceRotated)
        {
            transform.Rotate(0, 180, 0);
            ambulanceRotated = true;
            Debug.Log($"{name} Ambulance rotated 180 degrees");
        }

        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = waypoints.Length - 1;
            movingForward = false;
        }
        else if (currentWaypointIndex < 0)
        {
            currentWaypointIndex = 0;
            movingForward = true;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        if (targetWaypoint == null)
        {
            Debug.LogError($"{name} Ambulance waypoint {currentWaypointIndex} is null!");
            return;
        }

        // Check for car in front
        WaypointFollower blocker = GetCarInFront();
        bool hasBlocker = blocker != null;

        if (hasBlocker && !isOvertaking)
        {
            Vector3 toBlocker = blocker.transform.position - transform.position;
            toBlocker.y = 0;
            float blockerDist = toBlocker.magnitude;

            if (blockerDist < carStopDistance * 2f)
            {
                Vector3 leftOffset = -blocker.transform.right * overtakeSideDistance;
                Vector3 forwardOffset = blocker.transform.forward * overtakeForwardDistance;
                overtakeTarget = blocker.transform.position + leftOffset + forwardOffset;
                isOvertaking = true;
                overtakeTimer = 0f;
                Debug.Log($"{name} Ambulance overtaking!");
            }
        }

        if (isOvertaking)
        {
            overtakeTimer += Time.fixedDeltaTime;

            Vector3 dirToOvertake = (overtakeTarget - transform.position);
            dirToOvertake.y = 0;
            float distToOvertake = dirToOvertake.magnitude;

            if (distToOvertake < waypointThreshold || overtakeTimer > overtakeDuration)
            {
                isOvertaking = false;
                Debug.Log($"{name} Ambulance done overtaking, back to waypoints");
            }
            else
            {
                MoveTowards(overtakeTarget, speed);
                return;
            }
        }

        MoveTowards(targetWaypoint.position, speed);
    }

    WaypointFollower GetCarInFront()
    {
        WaypointFollower closest = null;
        float closestDist = float.MaxValue;
        WaypointFollower[] allCars = FindObjectsOfType<WaypointFollower>();

        foreach (WaypointFollower otherCar in allCars)
        {
            if (otherCar == this || otherCar.isAmbulance) continue;

            Vector3 toOther = otherCar.transform.position - transform.position;
            toOther.y = 0;
            float dist = toOther.magnitude;
            if (dist > carStopDistance * 2.5f) continue;

            float dot = Vector3.Dot(transform.forward, toOther.normalized);
            if (dot <= 0.3f) continue;

            float angle = Vector3.Angle(transform.forward, toOther);
            if (angle < 30f && dist < closestDist)
            {
                closestDist = dist;
                closest = otherCar;
            }
        }
        return closest;
    }

    void MoveTowards(Vector3 target, float targetSpeed)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;

        if (direction == Vector3.zero)
        {
            currentWaypointIndex += movingForward ? 1 : -1;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = waypoints.Length - 1;
                movingForward = false;
            }
            else if (currentWaypointIndex < 0)
            {
                currentWaypointIndex = 0;
                movingForward = true;
            }
            return;
        }

        float angleToTarget = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        isTurning = Mathf.Abs(angleToTarget) > 15f;
        float actualRotationSpeed = isTurning ? rotationSpeed * 0.5f : rotationSpeed;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * actualRotationSpeed);

        if (rb != null)
        {
            float currentYVelocity = rb.linearVelocity.y;
            rb.linearVelocity = new Vector3(transform.forward.x * targetSpeed, currentYVelocity, transform.forward.z * targetSpeed);
        }

        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(target.x, 0, target.z));

        if (distance < waypointThreshold)
        {
            currentWaypointIndex += movingForward ? 1 : -1;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = waypoints.Length - 1;
                movingForward = false;
            }
            else if (currentWaypointIndex < 0)
            {
                currentWaypointIndex = 0;
                movingForward = true;
            }
        }
    }

    void HandleNormalDriving()
    {
        float targetSpeed = speed;
        WaypointFollower closestCar = null;
        float closestDist = float.MaxValue;

        WaypointFollower[] allCars = FindObjectsOfType<WaypointFollower>();

        foreach (WaypointFollower otherCar in allCars)
        {
            if (otherCar == this) continue;

            if (tag != otherCar.tag) continue;

            Vector3 toOther = otherCar.transform.position - transform.position;
            toOther.y = 0;
            float dist = toOther.magnitude;
            
            float dotProduct = Vector3.Dot(transform.forward, toOther.normalized);
            
            if (dotProduct <= 0.3f) continue;
            if (dist > carStopDistance * 2.5f) continue;
            
            float angle = Vector3.Angle(transform.forward, toOther);

            if (angle < 30f && dist < closestDist)
            {
                closestDist = dist;
                closestCar = otherCar;
            }
        }

        if (closestCar != null)
        {
            float safeDistance = carStopDistance * 1.5f;
            if (closestDist < safeDistance)
            {
                targetSpeed = 0f;
            }
            else
            {
                float followFactor = (closestDist - carStopDistance) / (safeDistance - carStopDistance);
                targetSpeed = Mathf.Clamp(speed * followFactor, 0f, speed);
            }
        }

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
            currentWaypointIndex += movingForward ? 1 : -1;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = waypoints.Length - 1;
                movingForward = false;
            }
            else if (currentWaypointIndex < 0)
            {
                currentWaypointIndex = 0;
                movingForward = true;
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

            TrafficLightBase.State state = light.GetState();
            
            // ONLY move on GREEN - stop on everything else (red, yellow, prepare to go/stop)
            bool isGreen = (state == TrafficLightBase.State.Go);
            
            // If not green, check if we should stop
            if (!isGreen)
            {
                Vector3 toLight = light.transform.position - transform.position;
                toLight.y = 0;
                float dist = toLight.magnitude;
                float dotProduct = Vector3.Dot(transform.forward, toLight.normalized);
                
                if (dotProduct > 0.3f && dist < lightStopDistance && dist > 2f)
                {
                    Vector3 forward = transform.forward;
                    forward.y = 0;
                    float angle = Vector3.Angle(forward, toLight);
                    
                    if (angle < 60f)
                    {
                        Debug.Log($"Car {name} STOPPED - Light is {state}");
                        return true; // STOP
                    }
                }
            }
        }

        return false; // GO (all relevant lights are green)
    }
}