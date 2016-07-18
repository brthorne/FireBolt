using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{
    [RequireComponent(typeof(ElPresidente))]
    public class MiniMapController : MonoBehaviour
    {
        Camera miniMapCamera;
        CameraBody mainCameraBody;
        int miniMapLayerId;
        int terrainLayerId;
        int cameraFrustumLayerId;
        Vector3 markerScale = new Vector3(5, 5, 5);
        float markerHeight = 5;
        float cameraMarkerScale = 2.0f;

        List<string> unmarkedActors;

        public Material markerMaterial;

        public void InitializeMiniMap(List<string> actors)
        {
            unmarkedActors = actors;

            miniMapLayerId = LayerMask.NameToLayer("MiniMap");
            terrainLayerId = LayerMask.NameToLayer("Terrain");
            cameraFrustumLayerId = LayerMask.NameToLayer("CameraFrustum");

            GameObject proCam;
            if(!ElPresidente.createdGameObjects.TryGet("Pro Cam", out proCam))
            {
                Debug.LogError("Pro Cam not found for minimap render");

            }
            else
            {
                mainCameraBody = proCam.GetComponent<CameraBody>();
            }
            
            createMiniMapArtifacts();
            miniMapCamera = initializeCamera();
        }

        private void createMiniMapArtifacts()
        {
            GameObject rig;
            if(ElPresidente.createdGameObjects.TryGet("Rig", out rig))
            {
                var cameraMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cameraMarker.transform.localScale = new Vector3(cameraMarkerScale, cameraMarkerScale, cameraMarkerScale);
                cameraMarker.layer = miniMapLayerId;
                cameraMarker.transform.position = rig.transform.position + Vector3.up * 5;
                cameraMarker.transform.SetParent(rig.transform);
            }

            markUnmarkedActors();            
        }

        void markUnmarkedActors()
        {
            if(unmarkedActors == null || unmarkedActors.Count == 0)
            {
                return;
            }

            var removeList = new List<string>();

            foreach (var actorName in unmarkedActors)
            {
                GameObject actor;
                if (ElPresidente.createdGameObjects.TryGet(actorName, out actor))
                {
                    var actorMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    actorMarker.transform.localScale = markerScale;
                    actorMarker.layer = miniMapLayerId;
                    actorMarker.transform.position = actor.transform.position + Vector3.up * markerHeight;
                    actorMarker.transform.SetParent(actor.transform);
                    actorMarker.GetComponent<MeshRenderer>().material = markerMaterial;

                    removeList.Add(actorName);
                }
            }

            foreach(var actorToRemove in removeList)
            {
                unmarkedActors.Remove(actorToRemove);
            }            
        }

        void LateUpdate()
        {
            markUnmarkedActors();
            if (mainCameraBody != null)
            {
                mainCameraBody.drawFrustum(Color.white, Color.yellow, Color.cyan);
            }            
        }
        
        private Camera initializeCamera()
        {
            var obj = new GameObject();
            //set camera at the origin
            obj.transform.position = Vector3.up * 100;
            //point it at the ground
            obj.transform.rotation = Quaternion.LookRotation(Vector3.down);


            Camera  camera =  obj.AddComponent<Camera>();
            camera.depth = 2;
            camera.clearFlags = CameraClearFlags.Depth;
            camera.orthographic = true;            
            //make minimap bottom right, one third of height, square
            camera.rect = new Rect(1.0f - (3.0f / 16.0f), 0f, (3.0f / 16.0f), 0.33f);
            camera.aspect = 1.0f;

            camera.cullingMask = 1 << terrainLayerId | 1 << miniMapLayerId | 1 << cameraFrustumLayerId;
            
            camera.enabled = false;
            return camera;
        }

        public void ToggleMiniMap()
        {
            if (miniMapCamera.isActiveAndEnabled)
            {
                miniMapCamera.enabled = false;
            }
            else
            {
                miniMapCamera.orthographicSize = Terrain.activeTerrain.terrainData.size.x / 2.0f;
                miniMapCamera.enabled = true;
            }
        }
        
    }
}
