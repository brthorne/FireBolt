using UnityEngine;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace Assets.scripts
{
    

    public class FrameSaver : MonoBehaviour
    {
        [DllImport("opengl32")]
        public static extern int glReadPixels(int x, int y, int width, int height, int format, int type, IntPtr buffer);
        private const int GL_UNSIGNED_BYTE = 0x1401;
        private const int GL_UNSIGNED_INT_8_8_8_8_REV = 33639;
        private const int GL_RGBA = 0x1908;
        private const int GL_BGRA = 32993;

        byte[] pixels;
        bool init = false;

        static System.Diagnostics.Process ffmpeg;
        BinaryWriter framewriter;

        void Start()
        {
            //add post-render hook only when generating video
            if (ElPresidente.Instance.GenerateVideoFrames)
            {               
                pixels = new byte[Screen.width * Screen.height * 4];
                Camera.onPostRender += PostRender;
                ffmpeg = initializeFFMPEGCommand();
            }

        }

        private System.Diagnostics.Process initializeFFMPEGCommand()
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo("ffmpeg.exe",string.Format("-y -r {0} -f rawvideo -s {1}x{2} -pix_fmt rgba " +
                                                                                             "-i - -pix_fmt yuv420p -crf {0} -vf vflip {3}",
                                                                                             30, Screen.width, Screen.height, "output.mp4"));
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardError = false;
            return p;
        }

        public void PostRender(Camera camera)
        {

            if (!init)
            {
                ffmpeg.Start();
                framewriter = new BinaryWriter(ffmpeg.StandardInput.BaseStream);
                init = true;
            }

            unsafe
            {
                fixed (void* ptr = &(pixels[0]))
                {
                    IntPtr buffer = (IntPtr)ptr;
                    glReadPixels(0, 0, Screen.width, Screen.height, GL_RGBA, GL_UNSIGNED_BYTE, buffer);
                }
            }
            framewriter.Write(pixels, 0, pixels.Length);
        }

        public static void StopCapture()
        {
            ffmpeg.StandardInput.WriteLine('q');
        }

    }
}
