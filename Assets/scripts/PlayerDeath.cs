using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [Header("Ambulance Settings")]
    public GameObject ambulancePrefab;
    public Transform ambulanceSpawnPoint;

    void OnCollisionEnter(Collision collision)
    {
        // Debug line to see what we are hitting
        Debug.Log($"Player touched: {collision.gameObject.name}");

        // Detect hit by car
        if (collision.gameObject.name == "main" || collision.gameObject.GetComponent<WaypointFollower>() != null ||
            collision.gameObject.name.ToLower().Contains("car"))
        {
            Debug.Log("Player HIT by car! Die!");
            
            // Instead of disabling immediately, we trigger the ambulance first
            SpawnAmbulance();
            
            // Now hide the player (or play death animation if you have one later)
            gameObject.SetActive(false);
        }
    }

    void SpawnAmbulance()
    {
        if (ambulancePrefab != null && ambulanceSpawnPoint != null)
        {
            GameObject ambulance = Instantiate(ambulancePrefab, ambulanceSpawnPoint.position, ambulanceSpawnPoint.rotation);
            AmbulanceFollower ambulanceScript = ambulance.GetComponent<AmbulanceFollower>();
            if (ambulanceScript != null)
            {
                // Note: Even if player is disabled, the ambulance can still track this Transform
                // or you could create a temporary target point here.
                ambulanceScript.SetTarget(this.transform);
            }
            Debug.Log("Ambulance dispatched to player location!");
        }
        else
        {
            Debug.LogWarning("Ambulance prefab or spawn point not assigned on Player!");
        }
    }
}