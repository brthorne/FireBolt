using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    public class Shake : FireBoltAction
    {
        float shakeValue;
        string cameraName;
        GameObject actor;
        ShakeCam shakeCam;


        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Shake(float startTick, float endTick, string cameraName, float shakeValue) :
            base(startTick,endTick)
        {
            this.cameraName = cameraName;
            this.shakeValue = shakeValue;
        }

        public override bool Init()
        {

            if (actor == null &&
                !getActorByName(cameraName, out actor))
            {
                Debug.LogError("actor name [" + cameraName + "] not found. cannot shake");
                return false;
            }

            shakeCam = actor.GetComponent<ShakeCam>() as ShakeCam;
            if (shakeCam == null)
            {
                Debug.LogError(string.Format("camera name [{0}] does not have ShakeCam component",cameraName));
                return false;
            }

            Skip();
            return true;
        }

        public override void Execute(float currentTime)
        {
                       
        }

		public override void Undo()
		{
            //intentionally blank
        }

        public override void Skip()
        {
            shakeCam.positionShakeSpeed = shakeValue;
            shakeCam.rotationShakeSpeed = shakeValue; 
        }

        public override void Stop()
        {
            //nothing to stop
        }
    }
}
