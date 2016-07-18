using UnityEngine;
using System.Collections;

namespace Talis.Highlight
{    
    [ExecuteInEditMode]
    public class HighlightCamera : MonoBehaviour
    {
        public Material outlineMaterial;
        public Shader replacementShader;
        public Camera cam;

        void Start()
        {
            cam = GetComponent<Camera>();
            if (replacementShader)
                cam.SetReplacementShader(replacementShader, null);
            cam.depthTextureMode = DepthTextureMode.Depth;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, outlineMaterial);
        }
    }
}