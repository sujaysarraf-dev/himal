using UnityEngine;

public class NPCDeath : MonoBehaviour
{
    [Header("Death Settings")]
    public string deathTriggerName = "Die";
    public AudioClip crashSound;
    
    [Header("Ambulance Settings")]
    public GameObject ambulancePrefab;
    public Transform ambulanceSpawnPoint;

    private bool isDead = false;
    private Animator animator;
    private NPCFollower followerScript;

    void Start()
    {
        animator = GetComponent<Animator>();
        followerScript = GetComponent<NPCFollower>();
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"NPC touched: {collision.gameObject.name}");

        if (isDead) return;

        if (collision.gameObject.GetComponent<WaypointFollower>() != null || 
            collision.gameObject.name.ToLower().Contains("car"))
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("NPC DIED! Waiting for ambulance...");

        // Play crash sound
        if (crashSound != null)
        {
            AudioSource.PlayClipAtPoint(crashSound, transform.position);
        }

        if (followerScript != null)
        {
            followerScript.enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger(deathTriggerName);
        }

        if (ambulancePrefab != null && ambulanceSpawnPoint != null)
        {
            GameObject ambulance = Instantiate(ambulancePrefab, ambulanceSpawnPoint.position, ambulanceSpawnPoint.rotation);
            AmbulanceFollower ambulanceScript = ambulance.GetComponent<AmbulanceFollower>();
            if (ambulanceScript != null)
            {
                ambulanceScript.SetTarget(this.transform);
            }
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; 
        }
    }

    // This is now called by the Ambulance script when it arrives!
    public void TriggerDisappear()
    {
        Debug.Log("Ambulance arrived! NPC disappearing...");
        
        // Hide the root object (entire NPC)
        transform.root.gameObject.SetActive(false);
    }
}
