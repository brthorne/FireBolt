﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using UnityEditor;

namespace Assets.scripts
{
    public class Create : IActorAction
    {
        float startTick;
        string actorName,modelName;
        Vector3 position;

        public static bool ValidForConstruction(string actorName, string modelName)
        {
            if (string.IsNullOrEmpty(actorName) || string.IsNullOrEmpty(modelName))
                return false;
            return true;
        }

        public Create(float startTick, string actorName, string modelName, Vector3 position) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.modelName = modelName;
            this.position = position;
        }

        public void Init()
        {
            GameObject actor = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Models/" + modelName), position, Quaternion.identity) as GameObject;
            actor.name = actorName;
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

        public float? EndTick()
        {
            return null;
        }
    }
}
