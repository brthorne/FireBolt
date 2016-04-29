using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.scripts
{
   
    public class InputSpecifier : MonoBehaviour
    {
        

        public InputField storyField;
        public InputField modelField;
        public InputField cameraField;
        public InputField actorField;
        public InputField terrainField;

        void Start()
        {
            setNewDefaults();
        }

        public void StartFireBolt()
        {
            InputSet input = new InputSet();
            if (!string.IsNullOrEmpty(storyField.text))
            {
                input.StoryPlanPath = storyField.text;
            }

            if (!string.IsNullOrEmpty(modelField.text))
            {
                input.CinematicModelPath = modelField.text;
            }

            if (!string.IsNullOrEmpty(cameraField.text))
            {
                input.CameraPlanPath = cameraField.text;
            }

            if (!string.IsNullOrEmpty(actorField.text))
            {
                input.ActorsAndAnimationsBundlePath = actorField.text;
            }

            if (!string.IsNullOrEmpty(terrainField.text))
            {
                input.TerrainBundlePath = terrainField.text;
            }
            ElPresidente.Instance.Init(false, input);
        }

        void setNewDefaults()
        {
            //storyField.text = "storyPlans/shakespeare.xml";
            //modelField.text = "cinematicModels/DotaHierarchyModel.xml";
            //cameraField.text = "cameraPlans/dotaCamera.xml";
            //actorField.text = "AssetBundles/actorsandanimations";
            //terrainField.text = "AssetBundles/terrain";
            //storyField.text = "tests/defaultStory.xml";
            //modelField.text = "tests/defaultModel.xml";
            //cameraField.text = "tests/defaultCamera.xml";
            //actorField.text = "AssetBundles/actorsandanimations";
            //terrainField.text = "AssetBundles/terrain";
            //storyField.text = "tests/test004/defaultStory.xml";
            //modelField.text = "tests/test004/defaultModel.xml";
            //cameraField.text = "tests/test004/defaultCamera.xml";
            //storyField.text = "tests/fireblock/MoreComplexStory.xml";
            //modelField.text = "tests/fireblock/MoreComplexCinematic.xml";
            //cameraField.text = "tests/fireblock/MoreComplexCamera.xml";
            //actorField.text = "AssetBundles/actorsandanimations";
            //terrainField.text = "AssetBundles/terrain";
            storyField.text = "MoreComplexStory.xml";
            modelField.text = "MoreComplexCinematic.xml";
            cameraField.text = "MoreComplexCamera.xml";
            actorField.text = "AssetBundles/actorsandanimations";
            terrainField.text = "AssetBundles/terrain";
        }

        
    }
}
