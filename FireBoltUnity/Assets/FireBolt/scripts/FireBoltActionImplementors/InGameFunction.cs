using UnityEngine;
using System.Collections;
using CM = CinematicModel;
using UnityEditor;
using LN.Utilities;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace Assets.scripts
{
    public class InGameFunction : FireBoltAction
    {

        GameObject gameController = GameObject.FindGameObjectWithTag("GameController");
        List<Tuple<string,string,string>> tupleList;
        Component ingamescript;



        public InGameFunction(float startTick, float endTick, string functionName, List<Tuple<string,string,string>> paramNames) :
            base(startTick, endTick)
        {
            //Check that tuple list is okay, then...
           tupleList = paramNames;
           ingamescript = gameController.GetComponent(functionName);

        }

        public override bool Init()
        {
            ingamescript.SendMessage("Init", tupleList);
            return true;
        }

        public override void Undo()
		{
            ingamescript.SendMessage("Undo");
		}

        public override void Skip()
        {
            ingamescript.SendMessage("Skip");
        }

        public override void Execute(float currentTime) 
        {
            ingamescript.SendMessage("Execute");
	    }

        public override void Stop()
        {
        }
    }
}