using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public GameObject ambulancePrefab;
    public Transform ambulanceSpawnPoint;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "main" || collision.gameObject.GetComponent<WaypointFollower>() != null ||
            collision.gameObject.name.Contains("Car"))
        {
            Debug.Log("Player HIT by car! Die!");
            gameObject.SetActive(false);
            SpawnAmbulance();
        }
    }

    void SpawnAmbulance()
    {
        if (ambulancePrefab != null && ambulanceSpawnPoint != null)
        {
            Instantiate(ambulancePrefab, ambulanceSpawnPoint.position, ambulanceSpawnPoint.rotation);
            Debug.Log("Ambulance spawned!");
        }
        else
        {
            Debug.LogWarning("Ambulance prefab or spawn point not assigned!");
        }
    }
}