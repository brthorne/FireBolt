using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities;

namespace Assets.scripts
{
    public class Rotate : FireBoltAction
    {
        string actorName;
        protected GameObject actor;

        //where was the actor facing at start
		protected Vector3 startOrientation;

        //where should the actor be facing when this is done
        Vector3Nullable targetOrientation;

        //should be added to startOrientation to achieve targetOrientation
        Vector3 rotationChangeRequired;

        public static bool ValidForConstruction(string actorName, Vector3Nullable targetRotation, Vector2? targetPoint)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            if ((targetRotation.X.HasValue || targetRotation.Y.HasValue || targetRotation.Z.HasValue) && //can't define an angle and a point
                targetPoint.HasValue)
                return false;
            return true;
        }

        public override string ToString ()
        {
            return "Rotate " + actorName + " from " + startOrientation + " to " + targetOrientation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <param name="actorName"></param>
        /// <param name="targetRotation"></param>
        /// <param name="targetPoint">x,z</param>
        public Rotate(float startTick, float endTick, string actorName, Vector3Nullable targetRotation, Vector2? targetPoint) :
            base(startTick, endTick)
        {
            this.actorName = actorName;
            
            //TODO this smells.  but generality in all dimensions will take too long to figure out
            //if the targetPoint is specified, prefer to use it over a bare rotation value
            if (targetPoint.HasValue)
                this.targetOrientation = new Vector3Nullable(null, Mathf.Atan2(targetPoint.Value.x, targetPoint.Value.y) * Mathf.Rad2Deg, null);
            else
                this.targetOrientation = targetRotation;           
        }

        public override bool Init()
        {
            if (actor != null)
            {
                startOrientation = actor.transform.rotation.eulerAngles;
                return true;
            }
            
            if(actor == null &&
               !getActorByName(actorName, out actor))
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot rotate");
                return false;
            }

			startOrientation = actor.transform.rotation.eulerAngles;
        
            // i must away to el Presidente and discover what happens when I scrub back to the middle of an action
            //the verdict: all actions that have not yet completed at the target time will be undone and then reinitialized and executed

            //let's figure out how far we need to go
            //first we need to convert all the rotation values we have into a standard form.
            //reduce to less than a full rotation
            rotationChangeRequired = new Vector3(targetOrientation.X.HasValue ? (targetOrientation.X.Value - startOrientation.x).BindToSemiCircle() : 0,
                                                 targetOrientation.Y.HasValue ? (targetOrientation.Y.Value - startOrientation.y).BindToSemiCircle() : 0,
                                                 targetOrientation.Z.HasValue ? (targetOrientation.Z.Value - startOrientation.z).BindToSemiCircle() : 0);
            Debug.Log(this.ToString());
            return true;
        }

        public override void Execute(float currentTime)
        {
            if (endTick - startTick < 1)
                return;
            //every execute cycle we will get the actor rotation
            Vector3 currentRotation = actor.transform.rotation.eulerAngles;
            
            //if this rotate had something specified on a given axis, we set its position that we are going to add onto to the start position
            //otherwise we will preserve any changes done to actor's orientation subsequent to init of this rotate
            Vector3 newRotation = new Vector3(targetOrientation.X.HasValue ? startOrientation.x : currentRotation.x,
                                              targetOrientation.Y.HasValue ? startOrientation.y : currentRotation.y,
                                              targetOrientation.Z.HasValue ? startOrientation.z : currentRotation.z);

            //how much of our rotate duration has elapsed?
            float percentCompleted = (currentTime - startTick) / (endTick - startTick);
            
            Vector3 currentRotationAmount = rotationChangeRequired * percentCompleted;

            //add our scaled rotation onto the start/current rotations.  those axes using start also had their total amount set to 0 in the init
            newRotation = newRotation + currentRotationAmount;

            actor.transform.rotation = Quaternion.Euler(newRotation);
            //Debug.DrawRay(actor.transform.position + Vector3.up, actor.transform.forward,Color.magenta);
        }

        public override void Undo()
		{
			if (actor != null)
            {
                actor.transform.rotation = Quaternion.Euler(startOrientation);
            }
		}

        public override void Skip()
        {
            Vector3 actorTransformEulerAngles = actor.transform.rotation.eulerAngles;
            //for any unspecified values in targetRotation, set that axis angle to current value
            actor.transform.rotation = Quaternion.Euler(new Vector3(targetOrientation.X ?? actorTransformEulerAngles.x,
                                                                    targetOrientation.Y ?? actorTransformEulerAngles.y,
                                                                    targetOrientation.Z ?? actorTransformEulerAngles.z));
        }

        public override void Stop()
        {
            //nothing to stop
        }
    }
}
