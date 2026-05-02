using UnityEngine;

public class ZebraCrossing : MonoBehaviour
{
    private int characterCount = 0;
    public bool isPlayerOnZebra => characterCount > 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("NPC"))
        {
            characterCount++;
            Debug.Log(other.tag + " entered zebra: " + gameObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("NPC"))
        {
            characterCount--;
            if (characterCount < 0) characterCount = 0;
            Debug.Log(other.tag + " exited zebra: " + gameObject.name);
        }
    }

    void Update()
    {
        Debug.Log("Zebra: " + gameObject.name + " | CharacterOn=" + isPlayerOnZebra);
    }
}