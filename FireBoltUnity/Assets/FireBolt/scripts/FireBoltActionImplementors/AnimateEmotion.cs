using UnityEngine;
using System.Collections;
using CM = CinematicModel;

namespace Assets.scripts
{
    public class AnimateEmotion : FireBoltAction
    {
        private string actorName;
        private GameObject actor;
        private float emotionTime;
        private Animator animator;
        private int stopTriggerHash;
        private int animatingHash;

        public static bool ValidForConstruction(string actorName, float emotionTime)
        {
            if (string.IsNullOrEmpty(actorName) || emotionTime >62 || emotionTime < 0)
                return false;
            return true;
        }


        public AnimateEmotion(float startTick, float endTick, string actorName, float emotionTime) :
            base(startTick, endTick)
        {
            this.actorName = actorName;
            this.emotionTime = emotionTime;
            stopTriggerHash = Animator.StringToHash("stopEmotion");
            animatingHash = Animator.StringToHash("animating");
            
        }

        public override bool Init()
        {
            //short circuit if this has clearly been initialized before
            if(animator)
            {
                actor.SetActive(true);
                return true;
            }

            //get the actor this animate action is supposed to affect
            if(actor == null &&
               !getActorByName(actorName, out actor))
            {
                Debug.LogError("actor[" + actorName + "] not found.  cannot animate");
                return false;
            }

            //get the actor's current animator if it exists
            animator = actor.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("This actor[" + actorName + "] needs an animator controller first, not supported to put one on");
                return false;
            }
            Debug.Log("Initialized AnimateEmotion");
            
            return true;
        }

        public override void Undo()
		{
		}

        public override void Skip()
        {
            animator.SetTrigger(stopTriggerHash);
        }

        public override void Execute(float currentTime) 
        {
            animator.Play(animatingHash, 1, emotionTime);
            Debug.Log("Played AnimateEmotion");
	    }

        public override void Stop()
        {
            animator.SetTrigger(stopTriggerHash);
        }
    }
}