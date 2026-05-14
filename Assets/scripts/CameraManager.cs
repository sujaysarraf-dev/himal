using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraShot
    {
        public Camera cam;
        public float showTime = 2f;
    }

    public CameraShot[] cameras;

    private int currentIndex = 0;
    private float timer = 0f;

    void Start()
    {
        // Disable all cameras first
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].cam != null)
                cameras[i].cam.gameObject.SetActive(false);
        }

        // Enable first camera
        if (cameras.Length > 0 && cameras[0].cam != null)
        {
            cameras[0].cam.gameObject.SetActive(true);
            timer = cameras[0].showTime;
        }
    }

    void Update()
    {
        if (cameras.Length == 0)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SwitchToNextCamera();
        }
    }

    void SwitchToNextCamera()
    {
        // Disable current camera
        if (cameras[currentIndex].cam != null)
            cameras[currentIndex].cam.gameObject.SetActive(false);

        // Move to next
        currentIndex++;

        // Loop back to first
        if (currentIndex >= cameras.Length)
            currentIndex = 0;

        // Enable next camera
        if (cameras[currentIndex].cam != null)
        {
            cameras[currentIndex].cam.gameObject.SetActive(true);
            timer = cameras[currentIndex].showTime;
        }
    }
}