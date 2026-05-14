using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [Header("Ambulance Settings")]
    public GameObject ambulancePrefab;
    public Transform ambulanceSpawnPoint;

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Player touched: {collision.gameObject.name}");

        if (collision.gameObject.name == "main" || collision.gameObject.GetComponent<WaypointFollower>() != null ||
            collision.gameObject.name.ToLower().Contains("car"))
        {
            Debug.Log("Player HIT by car! Die!");

            SpawnAmbulance();

            Invoke(nameof(LoadZebraScene), 2f);
        }
    }

    void LoadZebraScene()
    {
        SceneManager.LoadScene("zebra");
    }

    void SpawnAmbulance()
    {
        if (ambulancePrefab != null && ambulanceSpawnPoint != null)
        {
            GameObject ambulance = Instantiate(ambulancePrefab, ambulanceSpawnPoint.position, ambulanceSpawnPoint.rotation);
            AmbulanceFollower ambulanceScript = ambulance.GetComponent<AmbulanceFollower>();
            if (ambulanceScript != null)
            {
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