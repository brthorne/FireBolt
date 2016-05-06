using UnityEngine;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

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

        public void Initialize(VideoInputSet videoInputSet)
        {
            pixels = new byte[Screen.width * Screen.height * 4];
            Camera.onPostRender += PostRender;
            ffmpeg = initializeFFMPEGCommand(videoInputSet);
        }

        private System.Diagnostics.Process initializeFFMPEGCommand(VideoInputSet videoInputSet)
        {
            var p = new System.Diagnostics.Process();
            StringBuilder videoOutputs = new StringBuilder();
            foreach(var encoding in videoInputSet.Encodings)
            {
                videoOutputs.Append(string.Format(" {0} {1}.{2} ",
                                                encoding.Value, videoInputSet.OutputPath, encoding.Key));
            }
            string ffmpegArgs = string.Format("-y -s {1}x{2} -f rawvideo -pix_fmt rgba -i - -framerate {0}  ", 
                                              videoInputSet.FrameRate, Screen.width, Screen.height) + videoOutputs.ToString();
            Debug.LogWarning("ffmpegArgs = " + ffmpegArgs);
            p.StartInfo = new System.Diagnostics.ProcessStartInfo(videoInputSet.FFMPEGPath, ffmpegArgs);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardError = false;
            return p;
        }

        public void PostRender(Camera camera)
        {
            if (ElPresidente.Instance.GenerateVideoFrames)
            {
                if (!init)
                {
                    ffmpeg.Start();
                    framewriter = new BinaryWriter(ffmpeg.StandardInput.BaseStream);
                    init = true;
                }
#if VIDEO_GEN
                unsafe
                {
                    fixed (void* ptr = &(pixels[0]))
                    {
                        IntPtr buffer = (IntPtr)ptr;
                        glReadPixels(0, 0, Screen.width, Screen.height, GL_RGBA, GL_UNSIGNED_BYTE, buffer);
                        framewriter.Write(pixels, 0, pixels.Length);
                    }
                }               
#endif
            }
        }

        public void StopCapture()
        {
            ffmpeg.StandardInput.WriteLine('q');
            ffmpeg.WaitForExit(2500);
            if (!ffmpeg.HasExited)
            {
                ffmpeg.Kill();
            }            
        }

    }
}
