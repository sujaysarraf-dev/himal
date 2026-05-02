using UnityEngine;

public class CameraSwitcher : MonoBehaviour {
    public Camera[] cameras;
    public float switchTime = 5f;
    
    private int currentIndex = 0;
    private float timer = 0f;
    
    void Start() {
        // Disable all cameras first
        for (int i = 0; i < cameras.Length; i++) {
            cameras[i].gameObject.SetActive(false);
        }
        
        // Enable first camera
        if (cameras.Length > 0) {
            cameras[0].gameObject.SetActive(true);
        }
    }
    
    void Update() {
        timer += Time.deltaTime;
        
        if (timer >= switchTime && currentIndex < cameras.Length - 1) {
            // Switch to next camera
            cameras[currentIndex].gameObject.SetActive(false);
            currentIndex++;
            cameras[currentIndex].gameObject.SetActive(true);
            timer = 0f;
        }
    }
}
