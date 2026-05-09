using UnityEngine;

public class ZebraCrossing : MonoBehaviour
{
    [Header("UI Feedback")]
    public GameObject crossingUI;
    
    // Keeping the name the same so we don't break the car scripts, 
    // but now it includes both Players and NPCs
    public bool isPlayerOnZebra => entityCount > 0;

    private int entityCount = 0;

    void Start()
    {
        if (crossingUI != null) crossingUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check for both Player and NPC tags
        if (other.CompareTag("Player") || other.CompareTag("NPC"))
        {
            entityCount++;
            if (crossingUI != null) crossingUI.SetActive(true);
            Debug.Log($"{other.tag} entered zebra: {gameObject.name} (Total: {entityCount})");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("NPC"))
        {
            entityCount--;
            if (entityCount <= 0)
            {
                entityCount = 0;
                if (crossingUI != null) crossingUI.SetActive(false);
            }
            Debug.Log($"{other.tag} exited zebra: {gameObject.name} (Total: {entityCount})");
        }
    }

    // Optional: Reset count on Disable to prevent ghost counts
    void OnDisable()
    {
        entityCount = 0;
    }
}