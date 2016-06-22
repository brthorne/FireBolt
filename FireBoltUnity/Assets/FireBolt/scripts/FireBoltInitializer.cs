using UnityEngine;
using System.Collections;
using System;
using Mono;

namespace Assets.scripts
{
    [RequireComponent(typeof(ElPresidente))]
    public class FireBoltInitializer : MonoBehaviour
    {
        private static readonly char encodingDelimiter = ':';
        // Use this for initialization
        void Start()
        {
            bool logDebug = false;
            bool logStatistics = false;
            string statFile = "stats.txt";
            InputSet inputFiles = new InputSet();
            VideoInputSet videoInputSet = null;
#if !UNITY_EDITOR
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-d":
                        switch (args[i + 1])
                        {
                            case "debug":
                                logDebug = true;
                                break;
                        }
                        break;
                    case "-stats":
                        logStatistics = true;    
                        statFile = args[i+1];                    
                        break;
                    case "-v": //video generation
                        QualitySettings.vSyncCount = 0;
                        videoInputSet = new VideoInputSet();
                        if (i + 1 < args.Length)
                        {
                            foreach(var encoding in args[i + 1].Split(new char[] { encodingDelimiter }))
                            {
                                videoInputSet.AddEncoding(encoding);
                            }
                        }
                        break;
                    case "-fr": //framerate
                        if (videoInputSet == null)
                        {
                            Console.WriteLine("-fr framerate option only valid for video mode; must follow -v");
                            Debug.LogError("-fr framerate option only valid for video mode; must follow -v");
                            break;
                        }
                        videoInputSet.FrameRate = uint.Parse(args[i + 1]);
                        break;
                    case "-ffmpeg"://ffmpeg location
                        if (videoInputSet == null)
                        {
                            Console.WriteLine("-ffmpeg ffmpeg path only valid for video mode; must follow -v");
                            Debug.LogError("-ffmpeg ffmpeg path only valid for video mode; must follow -v");
                            break;
                        }
                        videoInputSet.FFMPEGPath = args[i + 1];
                        break;
                    case "-vo": //output file
                        if (videoInputSet == null)
                        {
                            Console.WriteLine("-vo video output path only valid for video mode; must follow -v");
                            Debug.LogError("-vo video output path only valid for video mode; must follow -v");
                            break;
                        }
                        videoInputSet.OutputPath = args[i + 1];
                        break;
                    case "-story":
                        inputFiles.StoryPlanPath = args[i + 1];                        
                        break;
                    case "-model":
                        inputFiles.CinematicModelPath = args[i + 1];
                        break;
                    case "-camera":
                        inputFiles.CameraPlanPath = args[i + 1];
                        break;
                    default:
                        break;
                }

            }
            ElPresidente.Instance.Init(logDebug, inputFiles, videoInputSet, false, false, false, logStatistics, statFile);
#else
            Profiler.maxNumberOfSamplesPerFrame = -1;
            inputFiles.CameraPlanPath = "tests/longCamera.xml";
            //inputFiles.CinematicModelPath = "tests/lowAngle/defaultModel.xml";
            inputFiles.CinematicModelPath = "cinematicModels/defaultModel.xml";
            inputFiles.StoryPlanPath = "tests/longStory.xml";

            ElPresidente.Instance.Init(logDebug, inputFiles, videoInputSet, false, false, false, false, "statfile.txt");
#endif
        }
    }
}