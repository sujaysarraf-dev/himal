using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [System.Serializable]
    public class CameraShot
    {
        public Camera cam;
        public float showTime = 5f;
    }

    public CameraShot[] cameras;
    public bool loopCameras = true;

    [Header("UI Settings")]
    public Text currentCameraNameText;
    public Text timerText;

    private int currentIndex = 0;
    private float timer = 0f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (cameras == null || cameras.Length == 0) return;

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].cam != null)
                cameras[i].cam.gameObject.SetActive(false);
        }

        ActivateCamera(0);
    }

    void Update()
    {
        if (cameras == null || cameras.Length == 0) return;

        timer -= Time.deltaTime;

        CameraShot current = cameras[currentIndex];
        if (currentCameraNameText != null && current.cam != null)
        {
            currentCameraNameText.text = current.cam.name;
        }

        if (timerText != null)
        {
            timerText.text = Mathf.Max(0, timer).ToString("F1") + "s";
        }

        if (timer <= 0f)
        {
            NextCamera();
        }
    }

    public void NextCamera()
    {
        if (cameras == null || cameras.Length == 0) return;

        int nextIndex = currentIndex + 1;

        if (nextIndex >= cameras.Length)
        {
            if (loopCameras)
                nextIndex = 0;
            else
                return;
        }

        ActivateCamera(nextIndex);
        currentIndex = nextIndex;
    }

    public void PreviousCamera()
    {
        if (cameras == null || cameras.Length == 0) return;

        int prevIndex = currentIndex - 1;

        if (prevIndex < 0)
            prevIndex = loopCameras ? cameras.Length - 1 : 0;

        ActivateCamera(prevIndex);
        currentIndex = prevIndex;
    }

    public void GoToCamera(int index)
    {
        if (cameras == null || index < 0 || index >= cameras.Length) return;

        ActivateCamera(index);
        currentIndex = index;
    }

    void ActivateCamera(int index)
    {
        if (cameras == null || index < 0 || index >= cameras.Length) return;

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].cam != null)
                cameras[i].cam.gameObject.SetActive(i == index);
        }

        timer = cameras[index].showTime;
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public Camera GetCurrentCamera()
    {
        if (cameras != null && currentIndex >= 0 && currentIndex < cameras.Length)
            return cameras[currentIndex].cam;
        return null;
    }

    public int GetCameraCount()
    {
        return cameras != null ? cameras.Length : 0;
    }
}

public class CameraController : MonoBehaviour
{
    public string cameraName = "Camera";
    public Transform target;

    void Start()
    {
        gameObject.SetActive(false);
    }
}

public class CameraListItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Text nameText;
    public Text indexText;
    public Image backgroundImage;
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f);
    public Color dragColor = new Color(0.4f, 0.4f, 0.6f);

    private CameraController camera;
    private int index;
    private CameraManager manager;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Transform originalParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Setup(CameraController cam, int idx, CameraManager mgr)
    {
        camera = cam;
        index = idx;
        manager = mgr;

        if (nameText != null) nameText.text = cam.cameraName;
        if (indexText != null) indexText.text = (idx + 1).ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        transform.SetParent(transform.parent.parent);
        if (backgroundImage != null) backgroundImage.color = dragColor;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        float scale = canvas != null ? canvas.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / scale;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
        if (backgroundImage != null) backgroundImage.color = normalColor;
    }
}