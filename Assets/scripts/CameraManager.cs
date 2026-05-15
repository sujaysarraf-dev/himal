using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraShot
    {
        public Camera cam;
        public float showTime = 2f;
        public AudioClip music;
    }

    public CameraShot[] cameras;

    private int currentIndex = 0;
    private float timer = 0f;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].cam != null)
                cameras[i].cam.gameObject.SetActive(false);
        }

        if (cameras.Length > 0 && cameras[0].cam != null)
        {
            cameras[0].cam.gameObject.SetActive(true);
            timer = cameras[0].showTime;
            PlayMusic(cameras[0].music);
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

    void PlayMusic(AudioClip clip)
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
        audioSource.clip = clip;
        if (clip != null)
            audioSource.Play();
    }

    void SwitchToNextCamera()
    {
        if (cameras[currentIndex].cam != null)
            cameras[currentIndex].cam.gameObject.SetActive(false);

        currentIndex++;

        if (currentIndex >= cameras.Length)
            currentIndex = 0;

        if (cameras[currentIndex].cam != null)
        {
            cameras[currentIndex].cam.gameObject.SetActive(true);
            timer = cameras[currentIndex].showTime;
            PlayMusic(cameras[currentIndex].music);
        }
    }
}