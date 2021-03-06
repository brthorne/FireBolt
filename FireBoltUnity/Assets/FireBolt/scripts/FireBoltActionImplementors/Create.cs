﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
//using UnityEditor;

namespace Assets.scripts
{
    public class Create : FireBoltAction
    {
        string actorName,modelName;
        Vector3 position;
        Vector3? orientation;
		GameObject actor;
        bool defaultedCreate;
        CinematicModelMetaData metaData;

        private static Dictionary<string, GameObject> cachedModels = new Dictionary<string, GameObject>();

        public static bool ValidForConstruction(string actorName, string modelName)
        {
            if (string.IsNullOrEmpty(actorName) || string.IsNullOrEmpty(modelName))
                return false;
            return true;
        }

        public override string ToString ()
        {
            return string.Format ("Create " + actorName);
        }

        public Create(float startTick, string actorName, string modelName, Vector3 position,
                      CinematicModelMetaData metaData, Vector3? orientation=null, bool defaultedCreate=false) :
            base(startTick, startTick)
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.modelName = modelName;
            this.position = position;
            this.metaData = metaData;
            this.orientation = orientation;
            this.defaultedCreate = defaultedCreate;
			this.actor = null;
        }

        public override bool Init()
        {            
            Extensions.Log("init create model[{0}] for actor [{1}]",modelName, actorName);
            if (getActorByName(actorName, out actor))
            {
                if (defaultedCreate)
                    actor.SetActive(false);
                else
                    actor.SetActive(true);

                actor.transform.position = position;
                if(orientation.HasValue)
                    actor.transform.rotation = Quaternion.Euler(orientation.Value);
                return true;
            }
            Profiler.BeginSample("init create actor");
            GameObject model = null;
            if (cachedModels.ContainsKey(modelName))
            {
                Profiler.BeginSample("cached model lookup");
                model = cachedModels[modelName];
                Profiler.EndSample();
            }
            else if (ElPresidente.Instance.GetActiveAssetBundle().Contains(modelName))
            {
                Profiler.BeginSample("bundle model lookup");
                model = ElPresidente.Instance.GetActiveAssetBundle().LoadAsset<GameObject>(modelName);
                Profiler.EndSample();
                cachedModels.Add(modelName, model);
            }

            if (model == null)
            {
                Debug.LogError(string.Format("could not load asset[{0}] from assetbundle[{1}]", 
                                             modelName, ElPresidente.Instance.GetActiveAssetBundle().name));
                return false;
            }

            Quaternion actorOrientation = orientation.HasValue ?Quaternion.Euler(orientation.Value) : model.transform.rotation;
            actor = GameObject.Instantiate(model, position, actorOrientation) as GameObject;
            actor.name = actorName;

            GameObject instanceContainer;
            if (ElPresidente.createdGameObjects.TryGet("InstantiatedObjects", out instanceContainer))
            {               
                actor.transform.SetParent(instanceContainer.transform, true);
            }
            else
            {
                Extensions.Log("could not find InstantiatedObjects in createdGameObjects registry.  cannot add [{0}] in the hierarchy", actor);
            }
            //add actor to the main registry for quicker lookups
            ElPresidente.createdGameObjects.Add(actor.name, actor);

            actor.layer = LayerMask.NameToLayer("Actor");

            //add a collider so we can raycast against this thing
            if (actor.GetComponent<BoxCollider>() == null)
            {
                BoxCollider collider = actor.AddComponent<BoxCollider>();
                Bounds bounds = getBounds(actor);
                collider.center = new Vector3(0,0.75f,0); //TODO un-hack and find proper center of model                
                collider.size = bounds.max - bounds.min;
            }

            //staple the cinematic model metadata onto it
            var newData = actor.AddComponent<CinematicModelMetaDataComponent>();
            newData.LoadFromStruct(metaData);

            if (defaultedCreate)
            {
                actor.SetActive(false);
            }
            Profiler.EndSample();
            return true;
        }

        private Bounds getBounds(GameObject gameObject)
        {
            Bounds bounds;
            var renderer = gameObject.GetComponent<Renderer>();

            if (renderer != null)
            {
                bounds = renderer.bounds;
            }
            //if the model does not directly have a renderer, accumulate from child bounds
            else
            {
                bounds = new Bounds(gameObject.transform.position, Vector3.zero);
                foreach (var r in gameObject.GetComponentsInChildren<Renderer>())
                {
                    bounds.Encapsulate(r.bounds);
                }
            }
            return bounds;
        }

        public override void Undo()
		{
            Extensions.Log("Undo create" + ToString());
			if (actor != null)
            {
                actor.SetActive(false);
            }
            if (defaultedCreate)
            {
                actor.transform.position = position;
            }
			    
		}

        public override void Skip()
        {
            // nothing to skip
        }

        public override void Execute(float currentTime)
        {
            //nothing to do
        }

        public override void Stop()
        {
            //nothing to stop
        }

        public override string GetMainActorName()
        {
            return actorName;
        }
    }
}
