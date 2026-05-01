using UnityEngine;

public class SimpleTrafficLight : MonoBehaviour
{
    [Header("Lights")]
    public GameObject redLight;
    public GameObject yellowLight;
    public GameObject greenLight;

    [Header("Timing (seconds)")]
    public float redDuration = 3f;
    public float yellowDuration = 1f;
    public float greenDuration = 3f;

    private enum LightState { Red, Yellow, Green }
    private LightState currentState;
    private float timer;

    void Start()
    {
        Debug.Log("TrafficLight " + gameObject.name + " starting - RED");
        SetState(LightState.Red);
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            switch (currentState)
            {
                case LightState.Red:
                    SetState(LightState.Green);
                    break;
                case LightState.Green:
                    SetState(LightState.Yellow);
                    break;
                case LightState.Yellow:
                    SetState(LightState.Red);
                    break;
            }
        }
    }

    void SetState(LightState state)
    {
        currentState = state;
        timer = GetDuration(state);

        if (redLight) redLight.SetActive(state == LightState.Red);
        if (yellowLight) yellowLight.SetActive(state == LightState.Yellow);
        if (greenLight) greenLight.SetActive(state == LightState.Green);
    }

    float GetDuration(LightState state)
    {
        switch (state)
        {
            case LightState.Red: return redDuration;
            case LightState.Yellow: return yellowDuration;
            case LightState.Green: return greenDuration;
            default: return 3f;
        }
    }

    public bool IsRed() => currentState == LightState.Red;
    public bool IsYellow() => currentState == LightState.Yellow;
    public bool IsGreen() => currentState == LightState.Green;
    public string GetState() => currentState.ToString();
}