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
        //MethodInfo mi;
        //Type classType;
        //object classInstance;
        //MethodInfo initMi;
        //MethodInfo undoMi;
        //MethodInfo execMi;
        //MethodInfo skipMi;


        public InGameFunction(float startTick, float endTick, string functionName, List<Tuple<string,string,string>> paramNames) :
            base(startTick, endTick)
        {
            //Check that tuple list is okay, then...
           tupleList = paramNames;


            //find the functionName script.


           // classType = Type.GetType(functionName);
            // functionInstance = Activator.CreateInstance(classType);
             
             //classType = Type.GetType(functionName);
             //classInstance = Activator.CreateInstance(classType);

             ingamescript = gameController.GetComponent(functionName);
             //Debug.Log(ingamescript.ToString());


             //initMi = classType.GetMethod("Init");
             //undoMi = classType.GetMethod("Undo");
             //execMi = classType.GetMethod("Execute");
             //skipMi = classType.GetMethod("Skip");
             
            //mi = this.GetType().GetMethod(functionName);
        }

        public override bool Init()
        {
          //  mi.Invoke(this,tupleList);
           // Activator.CreateInstance(classType);

           // functionInstance.InvokeMember("Init", null);
           // functionType.Invoke(Init, tupleList);

            //initMi.Invoke(classInstance, new object[] { tupleList });
            Debug.Log("WHEN");
            ingamescript.SendMessage("Init", tupleList);
            return true;
        }

        public override void Undo()
		{
            ingamescript.SendMessage("Undo");
            //undoMi.Invoke(classInstance, null);
		}

        public override void Skip()
        {
            ingamescript.SendMessage("Skip");
            //skipMi.Invoke(classInstance, null);
        }

        public override void Execute(float currentTime) 
        {
            ingamescript.SendMessage("Execute");
           // execMi.Invoke(classInstance, null);
	    }

        public override void Stop()
        {
        }
    }
}