﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;

namespace Assets.scripts
{
    public class Translate : IActorAction
    {
        float lastUpdateTime;
        float startTick, endTick;
        string actorName;
        Vector3 destination;
        GameObject actor;
        Vector3 requiredVelocity;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Translate(float startTick, float endTick, string actorName, Vector3 destination) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.endTick = endTick;
            this.destination = destination;
        }

        public bool Init()
        {
            actor = GameObject.Find(actorName);
            if(actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot move");
                return false;
            }
            Vector3 direction = (destination - actor.transform.position);
            float moveDuration = endTick - startTick;
            requiredVelocity = new Vector3(direction.x/moveDuration, direction.y/moveDuration, direction.z/moveDuration);
            lastUpdateTime = Time.time * 1000;
            return true;
        }

        public void Execute()
        {
            //move enough to get where we're going before endTick
            float moveTimeElapsed = Time.time * 1000 - lastUpdateTime;
            Vector3 newPosition = new Vector3();
            try
            {
                newPosition = new Vector3(requiredVelocity.x * moveTimeElapsed, 
                                              requiredVelocity.y * moveTimeElapsed, 
                                              requiredVelocity.z * moveTimeElapsed) + actor.transform.position;
                actor.transform.position = newPosition;
            }
            catch (Exception ex)
            {
                Debug.LogError(newPosition.ToString());
            }
            lastUpdateTime = Time.time * 1000;
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
            return endTick;
        }
    }
}
