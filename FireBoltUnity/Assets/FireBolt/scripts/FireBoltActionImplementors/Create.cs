﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
//using UnityEditor;

namespace Assets.scripts
{
    public class Create : IFireBoltAction
    {
        float startTick;
        string actorName,modelName;
        Vector3 position;
		GameObject actor;

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

        public Create(float startTick, string actorName, string modelName, Vector3 position) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.modelName = modelName;
            this.position = position;
			this.actor = null;
        }

        public bool Init()
        {
            Debug.Log(string.Format("init create model[{0}] for actor [{1}]",modelName, actorName));
            if (actor != null)
            {
                actor.SetActive(true);
                actor.transform.position = position;
                //actor.transform.rotation = Quaternion.identity;
                return true;
            }
            GameObject model = null;
            if (ElPresidente.Instance.GetActiveAssetBundle().Contains(modelName))
            {
                model = ElPresidente.Instance.GetActiveAssetBundle().LoadAsset<GameObject>(modelName);
            }

            if (model == null)
            {
                Debug.LogError(string.Format("could not load asset[{0}] from assetbundle[{1}]", 
                                             modelName, ElPresidente.Instance.GetActiveAssetBundle().name));
                return false;
            }
            actor = GameObject.Instantiate(model, position, model.transform.rotation) as GameObject;
            actor.name = actorName;
            actor.transform.SetParent((GameObject.Find("InstantiatedObjects") as GameObject).transform, true);

            //add a collider so we can raycast against this thing
            if (actor.GetComponent<BoxCollider>() == null)
            {
                BoxCollider collider = actor.AddComponent<BoxCollider>();
                Bounds bounds = getBounds(actor);
                collider.center = new Vector3(0,0.75f,0); //TODO un-hack and find proper center of model                
                collider.size = bounds.max - bounds.min;
            }
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

		public void Undo()
		{
            Debug.Log ("Undo create");
			if (actor != null)
			    actor.SetActive (false);
		}

        public void Skip()
        {
            // nothing to skip
        }

        public void Execute()
        {
            //nothing to do
        }

        public void Stop()
        {
            //nothing to stop
        }

        public float StartTick()
        {
            return startTick;
        }

        public float EndTick()
        {
            return startTick;
        }
    }
}
