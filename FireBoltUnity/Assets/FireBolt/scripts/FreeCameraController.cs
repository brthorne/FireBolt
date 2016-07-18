using UnityEngine;
using Talis.Highlight;
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

        //highlight things
        Shader outlineShader;
        HighlightCamera highlightCameraController;
        GameObject highlit;
        int actorLayer;
        int highlightLayer;

        void Start()
        {
            freeCamera = gameObject.AddComponent<Camera>();
            freeCamera.tag = "MainCamera";

            //layers
            highlightLayer = LayerMask.NameToLayer("Highlight");
            actorLayer = LayerMask.NameToLayer("Actor");

            //attach a highlight camera
            //var highlightCameraRig = new GameObject("HighlightCamera");
            //highlightCameraRig.transform.SetParent(gameObject.transform);
            //highlightCameraRig.transform.localPosition = Vector3.zero;
            //highlightCameraRig.transform.localRotation = Quaternion.identity;
            //var highlightCamera = highlightCameraRig.AddComponent<Camera>();
            //highlightCameraController = highlightCameraRig.AddComponent<HighlightCamera>();
            //highlightCameraController.replacementShader = Shader.Find("Custom/SolidColor");
            //highlightCameraController.outlineMaterial = Resources.Load<Material>("Materials/outlineposteff");
            //highlightCamera.cullingMask = highlightLayer;
            //highlightCamera.clearFlags = CameraClearFlags.Depth;
            //highlightCamera.depth = 1;

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

        void moveMouseOverToHighlight()
        {
            if(highlit!=null)
            {
                highlit.layer = actorLayer;
            }
            RaycastHit hit;
            if (Physics.Raycast(freeCamera.ScreenPointToRay(Input.mousePosition), out hit, 1000f, 1<<actorLayer))
            {
                highlit = hit.transform.gameObject;
                highlit.layer = highlightLayer;
            }
        }

        void Update()
        {
            //moveMouseOverToHighlight();
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