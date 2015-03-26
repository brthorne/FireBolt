﻿using UnityEngine;
using System.Collections;

namespace Assets.scripts {
//concrete
    public class AnimateMecanim : MonoBehaviour, IActorAction{

	    private Animator thisAnim;
	    private int animationHash;
        private long startTick;

	    public AnimateMecanim(GameObject someGameObject, string animName) 
        {
		    thisAnim = someGameObject.GetComponent<Animator> ();
		    animationHash = Animator.StringToHash (animName);
	    }
	
	    public void Execute () {
		    thisAnim.SetTrigger (animationHash);
	    }

        public long StartTick()
        {
            return startTick;
        }



        public long EndTick()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public void Init()
        {
            throw new System.NotImplementedException();
        }
    }

}