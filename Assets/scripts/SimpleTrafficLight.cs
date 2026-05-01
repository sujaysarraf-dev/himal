using UnityEngine;
using HealthbarGames;

public class SimpleTrafficLight : MonoBehaviour
{
    public HaloTrafficLight haloTrafficLight;

    [Header("Timing (seconds) - FAST for testing")]
    public float redDuration = 3f;
    public float yellowDuration = 1f;
    public float greenDuration = 3f;

    private enum LightState { Red, Yellow, Green }
    private LightState currentState;
    private float timer;

    void Start()
    {
        if (haloTrafficLight == null)
            haloTrafficLight = GetComponent<HaloTrafficLight>();

        Debug.Log("TrafficLight " + gameObject.name + " started - Initializing as RED");
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
                    Debug.Log("TrafficLight " + gameObject.name + " -> GREEN");
                    SetState(LightState.Green);
                    break;
                case LightState.Green:
                    Debug.Log("TrafficLight " + gameObject.name + " -> YELLOW");
                    SetState(LightState.Yellow);
                    break;
                case LightState.Yellow:
                    Debug.Log("TrafficLight " + gameObject.name + " -> RED");
                    SetState(LightState.Red);
                    break;
            }
        }
    }

    void SetState(LightState state)
    {
        currentState = state;

        switch (state)
        {
            case LightState.Red:
                haloTrafficLight?.ChangeLightState(true, false, false, null);
                timer = redDuration;
                break;
            case LightState.Green:
                haloTrafficLight?.ChangeLightState(false, false, true, null);
                timer = greenDuration;
                break;
            case LightState.Yellow:
                haloTrafficLight?.ChangeLightState(false, true, false, null);
                timer = yellowDuration;
                break;
        }
    }

    public bool IsRed()
    {
        return currentState == LightState.Red;
    }

    public bool IsGreen()
    {
        return currentState == LightState.Green;
    }

    public string GetState()
    {
        return currentState.ToString();
    }
}
