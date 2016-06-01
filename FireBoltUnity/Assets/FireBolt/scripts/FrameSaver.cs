using UnityEngine;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO.Pipes;
using System.ComponentModel;

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
                videoOutputs.Append(string.Format(" {0} -r {1} {2}.{3} ",
                                                encoding.Value, videoInputSet.FrameRate, videoInputSet.OutputPath, encoding.Key));
            }
            string ffmpegArgs = string.Format("-y -framerate {0} -s {1}x{2} -f rawvideo -pix_fmt rgba -i - ",            
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
                    Debug.LogWarning("starting ffmpeg");
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
            Camera.onPostRender -= PostRender;
            ffmpeg.StandardInput.Close();
            while (!ffmpeg.HasExited)
            {
                ffmpeg.WaitForExit(500);
            }
        }

    }
}

//leaving named pipes alone for the time being...no useful performance gains to be had in encoding

//string ffmpegArgs = string.Format(@"-y -framerate {0} -s {1}x{2} -f rawvideo -pix_fmt rgba -i  \\.\pipe\{3}",

//pipeName = "framePipe" + System.Diagnostics.Process.GetCurrentProcess().Id +"-"+ DateTime.Now.Ticks;
//Debug.LogWarning("creating pipe: " + pipeName);
//try
//{
//    framePipe = new NamedPipeServerStream(pipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte,
//                                          PipeOptions.None, 1024, int.MaxValue);//TODO pid based name
//}
//catch ( Win32Exception stupidException)
//{
//    if(!stupidException.Message.Contains("The operation completed successfully."))//exceptional success
//    {
//        throw new IOException("failed in creating named pipe", stupidException);
//    }
//    else
//    {
//        Debug.LogError("failure : succesful encountered");   
//    }
//}

//if(framePipe == null)
//{
//    Debug.LogError("framePipe is null after create");
//}
