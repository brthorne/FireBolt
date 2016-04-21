using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public class FrameSaver : MonoBehaviour
    {
        Texture2D screenShot;
        uint videoFrameNumber;
        void Start()
        {         
            screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            Camera.onPostRender += PostRender;
        }

        public void PostRender(Camera camera)
        {            
            if (ElPresidente.Instance.GenerateVideoFrames)
            {
                // Read the rendered texture into the texture 2D.
                screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0,false);

                // Save the texture 2D as a PNG.
                byte[] bytes = screenShot.EncodeToPNG();
                File.WriteAllBytes(@".screens/" + videoFrameNumber + ".png", bytes);
                videoFrameNumber++;

            }
        }
    }
}
