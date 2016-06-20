using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Assets.scripts
{
    [RequireComponent(typeof(ElPresidente))]
    public class DebugInputManager : MonoBehaviour
    {

        public float fixedTimeStep = 33f;
        public float fixedTimeStepIncrement = 1f;
        public Text fixedTimeStepDisplayText;
        public MiniMapController MinimapController;

        //using the unity event system requires that we send event data...which we don't care about
        UnityEngine.EventSystems.BaseEventData garbageEventData;
        bool freelooking = false;
        FreeCameraController freeCameraController;

        // Use this for initialization
        void Start()
        {
            garbageEventData = new UnityEngine.EventSystems.BaseEventData(UnityEngine.EventSystems.EventSystem.current);
            fixedTimeStepDisplayText.text = fixedTimeStep.ToString();
            GameObject go = new GameObject("freeCamera");
            freeCameraController = go.AddComponent<FreeCameraController>();
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Q))
            {
                ElPresidente.Instance.ToggleCameraExecuting();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                ElPresidente.Instance.ToggleStoryExecuting();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ElPresidente.Instance.PauseToggle();
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                fixedTimeStep += fixedTimeStepIncrement;
                fixedTimeStepDisplayText.text = fixedTimeStep.ToString();
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                fixedTimeStep -= fixedTimeStepIncrement;
                if (fixedTimeStep < ElPresidente.MILLIS_PER_FRAME)
                {
                    fixedTimeStep = ElPresidente.MILLIS_PER_FRAME;
                }
                fixedTimeStepDisplayText.text = fixedTimeStep.ToString();
            }

            //back up in dicourse time
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ElPresidente.Instance.SuspendTimeUpdate(garbageEventData);
                ElPresidente.Instance.setTime(getTargetPercentComplete(-fixedTimeStep));
                ElPresidente.Instance.ResumeTimeUpdate(garbageEventData);
            }

            //skip forward in discourse time
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ElPresidente.Instance.SuspendTimeUpdate(garbageEventData);
                ElPresidente.Instance.setTime(getTargetPercentComplete(fixedTimeStep));
                ElPresidente.Instance.ResumeTimeUpdate(garbageEventData);
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                //toggle minimap
                if (MinimapController != null)
                {
                    MinimapController.ToggleMiniMap();
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (freelooking)
                {
                    freeCameraController.StopFreeLook();
                    freelooking = false;
                }
                else
                {
                    if (Time.timeScale != 0f)
                    {
                        ElPresidente.Instance.PauseToggle();
                    }
                    //lookup speed shouldn't really matter here...though it should really probably pull created objects registry
                    freeCameraController.StartFreeLook(GameObject.Find("Pro Cam").GetComponent<Camera>(), GameObject.Find("Rig"));
                    freelooking = true;
                }
                
            }
        }

        /// <summary>
        /// find the percent complete for dicourse time based on current
        /// discourse time and a provided offset
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>value from 0 to 1 for discourse time percent completion</returns>
        private float getTargetPercentComplete(float offset)
        {
            return (ElPresidente.currentDiscourseTime + offset) / ElPresidente.Instance.EndDiscourseTime;
        }
    }
}