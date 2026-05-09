using UnityEngine;

public class NPCFollower : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform[] waypoints;
    public bool loop = true;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float stopDistance = 0.2f;
    public float waitTimeAtWaypoint = 1f;
    
    [Header("Animation Settings")]
    public string speedParameterName = "Speed";
    public bool useAnimator = true;

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private Animator animator;
    private int animIDSpeed;

    void Start()
    {
        if (useAnimator)
        {
            animator = GetComponent<Animator>();
            if (animator != null)
            {
                animIDSpeed = Animator.StringToHash(speedParameterName);
            }
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"NPC {gameObject.name} has no waypoints assigned!");
        }
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0 || currentWaypointIndex >= waypoints.Length) return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            UpdateAnimation(0f);
            if (waitTimer <= 0)
            {
                isWaiting = false;
                GoToNextWaypoint();
            }
            return;
        }

        MoveTowardsWaypoint();
    }

    void MoveTowardsWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];
        if (target == null) return;

        Vector3 direction = (target.position - transform.position);
        direction.y = 0; // Keep movement on the ground plane

        float distance = direction.magnitude;

        if (distance < stopDistance)
        {
            isWaiting = true;
            waitTimer = waitTimeAtWaypoint;
            return;
        }

        // Rotation
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Movement
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.position.x, transform.position.y, target.position.z), moveSpeed * Time.deltaTime);
        
        UpdateAnimation(moveSpeed);
    }

    void GoToNextWaypoint()
    {
        currentWaypointIndex++;
        if (currentWaypointIndex >= waypoints.Length)
        {
            if (loop)
            {
                currentWaypointIndex = 0;
            }
        }
    }

    void UpdateAnimation(float currentSpeed)
    {
        if (useAnimator && animator != null && HasParameter(speedParameterName, animator))
        {
            animator.SetFloat(animIDSpeed, currentSpeed);
        }
    }

    // Helper to check if animator has a specific parameter
    public static bool HasParameter(string paramName, Animator anim)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawSphere(waypoints[i].position, 0.2f);

            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
            else if (loop && waypoints[0] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
            }
        }
    }
}
