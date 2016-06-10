using UnityEngine;
using System.Collections.Generic;
using CM = CinematicModel;

namespace Assets.scripts
{
    public class AnimateMecanim : FireBoltAction
    {
        private string actorName;
        private GameObject actor;
        private string animName;
        private string stateName;
        private Animator animator;
        private AnimationClip animation;
        private AnimationClip state;
        AnimatorOverrideController overrideController;
        private int stopTriggerHash;
        private bool loop;
        private static readonly string animationToOverride = "_87_a_U1_M_P_idle_Neutral__Fb_p0_No_1";
        private static readonly string stateToOverride = "state";
        bool assignEndState = false;

        private static Dictionary<string, AnimationClip> cachedAnimationClips = new Dictionary<string, AnimationClip>();

        public static bool ValidForConstruction(string actorName, CM.Animation animation)
        {
            if (string.IsNullOrEmpty(actorName) || animation == null || string.IsNullOrEmpty(animation.FileName))
                return false;
            return true;
        }


        public AnimateMecanim(float startTick, float endTick, string actorName, string animName, bool loop, string endingName) :
            base(startTick, endTick)
        {
            this.actorName = actorName;
            this.animName = animName;
			this.loop = loop;
            this.assignEndState = !string.IsNullOrEmpty(endingName);
            this.stateName = endingName; 
            stopTriggerHash = Animator.StringToHash("stop");
        }

        public override bool Init()
        {
            
            //short circuit if this has clearly been initialized before
            if((animator && animator.isInitialized && overrideController && animation && 
                (!assignEndState ||(assignEndState && state))))
            {
                assignAnimations();
                animator.runtimeAnimatorController = overrideController;
                actor.SetActive(true);
                return true;
            }

            if (!findAnimations()) return false;
            animation.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            //get the actor this animate action is supposed to affect
            if (actor == null &&
               !getActorByName(actorName, out actor))
            {
                Debug.LogError("actor[" + actorName + "] not found.  cannot animate");
                return false;
            }

            //get the actor's current animator if it exists
            animator = actor.GetComponent<Animator>();
            if (animator == null)
            {
                animator = actor.AddComponent<Animator>();                
            }          

            //find or make an override controller
            if (animator.runtimeAnimatorController is AnimatorOverrideController)
            {
                overrideController = (AnimatorOverrideController) animator.runtimeAnimatorController;
            }
            else
            {
                overrideController = new AnimatorOverrideController();
                overrideController.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("AnimatorControllers/Generic");
                animator.runtimeAnimatorController = overrideController;
            }

            assignAnimations();
            animator.applyRootMotion = false;
            return true;
        }

        private void assignAnimations()
        {
            overrideController[animationToOverride] = animation;
            if (assignEndState)
                overrideController[stateToOverride] = state;
        }

        private bool findAnimations()
        {            
            AnimationClip animationClip;
            if(!lookupAnimation(animName, out animationClip))
            {
                return false;
            }
            animation = animationClip;

            //using different variables to keep from reassigning references in the cache....this is super sketchy
            AnimationClip stateClip;
            if (!string.IsNullOrEmpty(stateName))
            {                
                if(!lookupAnimation(stateName, out stateClip))
                {
                    return false;
                }
                state = stateClip;
            }            
            return true;
        }

        private bool lookupAnimation(string animationName, out AnimationClip clip)
        {
            clip = null;
            if (cachedAnimationClips.ContainsKey(animationName))
            {
                Profiler.BeginSample("cached animation lookup");
                clip = cachedAnimationClips[animationName];
                Profiler.EndSample();
            }
            else if (ElPresidente.Instance.GetActiveAssetBundle().Contains(animationName))
            {
                Profiler.BeginSample("bundle animation lookup");
                clip = ElPresidente.Instance.GetActiveAssetBundle().LoadAsset<AnimationClip>(animationName);
                Profiler.EndSample();
                if (clip == null)
                {
                    Debug.LogError(string.Format("unable to find animation [{0}] in asset bundle[{1}]", animationName, ElPresidente.Instance.GetActiveAssetBundle().name));
                    return false;
                }
                cachedAnimationClips.Add(animationName, clip);            
            }
            else
            {
                Extensions.Log("asset bundle [{0}] does not contain animation[{1}]", ElPresidente.Instance.GetActiveAssetBundle().name, animationName);
                return false;
            }
            return true;
        }

        public override void Undo()
		{
		}

        public override void Skip()
        {
            //if (animator)
            //{
            //    animator.SetTrigger(stopTriggerHash);
            //}                
        }

        public override void Execute(float currentTime) 
        {
            Profiler.BeginSample("exec animate");
            float at = Mathf.Repeat ((currentTime - startTick)/1000, animation.length);
            animator.CrossFade( "animating", 0, 0, at/animation.length);
            Profiler.EndSample();
	    }

        public override void Stop()
        {
            //if (!animator.isInitialized)
            //{
            //    animator.Rebind();
            //}
            //animator.SetTrigger(stopTriggerHash);
        }
    }
}