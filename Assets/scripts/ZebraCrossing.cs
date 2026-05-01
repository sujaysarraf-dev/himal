using UnityEngine;

public class ZebraCrossing : MonoBehaviour
{
    private int playerCount = 0;
    public bool isPlayerOnZebra => playerCount > 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount++;
            Debug.Log("Player entered zebra: " + gameObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount--;
            if (playerCount < 0) playerCount = 0;
            Debug.Log("Player exited zebra: " + gameObject.name);
        }
    }

    void Update()
    {
        Debug.Log("Zebra: " + gameObject.name + " | PlayerOn=" + isPlayerOnZebra);
    }
}