using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public class VideoInputSet
    {
        public static readonly string FFMPEG_PATH_DEFAULT = "ffmpeg.exe";
        public static readonly string VIDEO_OUTPUT_PATH_DEFAULT = "output";
        public static readonly uint FRAME_RATE_DEFAULT = 30;

        public static readonly string ENCODING_MP4 = "mp4";
        public static readonly string ENCODING_OPTIONS_MP4 = "-c:v libx264 -pix_fmt yuv420p -vf vflip";

        public static readonly string ENCODING_OGV = "ogv";
        public static readonly string ENCODING_OPTIONS_OGV = "-c:v libtheora -pix_fmt yuv420p -b:v 1200k -vf vflip";

        public static readonly string ENCODING_WEBM = "webm";
        public static readonly string ENCODING_OPTIONS_WEBM = "-c:v libvpx-vp9 -b:v 1200k -speed 4 -vf vflip";

        public static readonly string ENCODING_DEFAULT = ENCODING_MP4;
        public static readonly string ENCODING_OPTIONS_DEFAULT = ENCODING_OPTIONS_MP4;

        public static readonly Dictionary<string, string> ENCODINGS_POSSIBLE = new Dictionary<string, string>()
        {
            {ENCODING_MP4, ENCODING_OPTIONS_MP4},
            {ENCODING_OGV, ENCODING_OPTIONS_OGV},
            {ENCODING_WEBM, ENCODING_OPTIONS_WEBM}
        };



        public VideoInputSet()
        {
            this.FFMPEGPath = FFMPEG_PATH_DEFAULT;
            this.OutputPath = VIDEO_OUTPUT_PATH_DEFAULT;
            this.FrameRate = FRAME_RATE_DEFAULT;
            this.encodings = new Dictionary<string, string>();

        }

        public string FFMPEGPath { get; set; }
        public string OutputPath { get; set; }
        public uint FrameRate { get; set; }

        private Dictionary<string, string> encodings;
        public Dictionary<string, string> Encodings
        {
            get
            {
                if(encodings.Keys.Count == 0)
                {
                    return new Dictionary<string, string>() { { ENCODING_DEFAULT, ENCODING_OPTIONS_DEFAULT } };
                }
                return encodings;
            }
        }

        public void AddEncoding(string encodingName)
        {
            if (ENCODINGS_POSSIBLE.ContainsKey(encodingName))
            {
                encodings.Add(encodingName, ENCODINGS_POSSIBLE[encodingName]);
            }
        }

    }
}
