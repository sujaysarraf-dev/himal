using UnityEngine;
using System.Collections;

namespace HealthbarGames
{
    public class HaloTrafficLight : TrafficLightBase
    {
        // 🔴 RED
        public Renderer RedRenderer;
        public GameObject RedHalo;
        public Material RedOnMat;
        public Material RedOffMat;

        // 🟡 YELLOW
        public Renderer YellowRenderer;
        public GameObject YellowHalo;
        public Material YellowOnMat;
        public Material YellowOffMat;

        // 🟢 GREEN
        public Renderer GreenRenderer;
        public GameObject GreenHalo;
        public Material GreenOnMat;
        public Material GreenOffMat;

        private bool mInitialized = false;
        private bool isRedOn = false;
        private bool isYellowOn = false;
        private bool isGreenOn = false;

        void Awake()
        {
            if ((RedRenderer != null || RedHalo != null) &&
                (YellowRenderer != null || YellowHalo != null) &&
                (GreenRenderer != null || GreenHalo != null))
            {
                mInitialized = true;
            }
            else
            {
                mInitialized = false;
                Debug.LogError("Some variables haven't been assigned correctly for HaloTrafficLight script.", this);
            }
        }

        public override void OnLightStateChanged(bool redLightState, bool yellowLightState, bool greenLightState)
        {
            if (!mInitialized)
                return;

            isRedOn = redLightState;
            isYellowOn = yellowLightState;
            isGreenOn = greenLightState;

            if (RedHalo != null)
                RedHalo.SetActive(redLightState);

            if (RedRenderer != null)
                RedRenderer.material = redLightState ? RedOnMat : RedOffMat;

            if (YellowHalo != null)
                YellowHalo.SetActive(yellowLightState);

            if (YellowRenderer != null)
                YellowRenderer.material = yellowLightState ? YellowOnMat : YellowOffMat;

            if (GreenHalo != null)
                GreenHalo.SetActive(greenLightState);

            if (GreenRenderer != null)
                GreenRenderer.material = greenLightState ? GreenOnMat : GreenOffMat;
        }

        public bool IsRedState()
        {
            return isRedOn;
        }

        public bool IsYellowState()
        {
            return isYellowOn;
        }

        public bool IsGreenState()
        {
            return isGreenOn;
        }
    }
}