using UnityEngine;
using System.Collections;

namespace Assets.scripts
{
    public class FreeCameraController : MonoBehaviour
    {
        float translateSpeed = 0.2f;
        float rotateSpeed = 0.8f;
        float scrollThreshold = 0.05f;
        float fieldOfViewMax = 160f;
        float fieldOfViewMin = 3f;
        Camera freeCamera;
        Camera baseCamera;
        Vector3 previousMousePosition;

        void Start()
        {
            freeCamera = gameObject.AddComponent<Camera>();
            freeCamera.tag = "MainCamera";
            freeCamera.enabled = false;
            enabled = false;
        }

        public void StopFreeLook()
        {
            freeCamera.enabled = false;
            enabled = false;

            baseCamera.enabled = true;
        }

        public void StartFreeLook(Camera baseCamera, GameObject baseRig)
        {
            this.baseCamera = baseCamera;
            baseCamera.enabled = false;
            gameObject.transform.position = baseRig.transform.position;
            gameObject.transform.rotation = baseRig.transform.rotation;

            freeCamera.CopyFrom(baseCamera);
            freeCamera.cullingMask = ~(1 << LayerMask.NameToLayer("MiniMap"));

            freeCamera.enabled = true;
            enabled = true;

        }

        void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                gameObject.transform.position += gameObject.transform.forward * translateSpeed;
            }
            if (Input.GetKey(KeyCode.A))
            {
                gameObject.transform.position -= gameObject.transform.right * translateSpeed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                gameObject.transform.position += gameObject.transform.right * translateSpeed;
            }
            if (Input.GetKey(KeyCode.S))
            {
                gameObject.transform.position -= gameObject.transform.forward * translateSpeed;
            }

            if (Input.GetMouseButton(1))
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
                if (!Input.GetMouseButtonDown(1))
                { 
                    var rotation = gameObject.transform.rotation.eulerAngles;
                    rotation.y += Input.GetAxis("Mouse X") * rotateSpeed;
                    rotation.x -= Input.GetAxis("Mouse Y") * rotateSpeed;
                    gameObject.transform.rotation = Quaternion.Euler(rotation);
                }
                previousMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(1))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            float scrollAmount = Input.GetAxis("Mouse ScrollWheel");

            if (scrollAmount > scrollThreshold)
            {
                freeCamera.fieldOfView += getFieldOfViewIncrement(freeCamera.fieldOfView);
                if(freeCamera.fieldOfView > fieldOfViewMax)
                {
                    freeCamera.fieldOfView = fieldOfViewMax;
                }
            }
            else if(scrollAmount < -scrollThreshold)
            {
                freeCamera.fieldOfView -= getFieldOfViewIncrement(freeCamera.fieldOfView);
                if(freeCamera.fieldOfView < fieldOfViewMin)
                {
                    freeCamera.fieldOfView = fieldOfViewMin;
                }
            }

        }

        private float getFieldOfViewIncrement(float fov)
        {
            return 0.1f * fov;
        }
    }
}