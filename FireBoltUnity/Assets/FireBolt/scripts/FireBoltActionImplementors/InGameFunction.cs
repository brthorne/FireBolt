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

        GameObject hostObject;
        List<Tuple<string,string,string>> tupleList;
        Component ingamescript;
        String objectName;


        public InGameFunction(float startTick, float endTick, string functionName, List<Tuple<string,string,string>> paramNames) :
            base(startTick, endTick)
        {
            objectName = startTick.ToString() + " " + endTick.ToString() + " " +  functionName;
            if (!ElPresidente.createdGameObjects.TryGet(objectName, out hostObject))
            {
                hostObject = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FireBolt/Resources/" + functionName + "_prefab.prefab")) as GameObject;
                //Debug.Log(hostObject.name);
                GameObject fireBolt;
                if (ElPresidente.createdGameObjects.TryGet("FireBolt", out fireBolt))
                {
                    hostObject.transform.SetParent(fireBolt.transform);
                }
                ElPresidente.createdGameObjects.Add(objectName, hostObject);
                tupleList = paramNames;
                ingamescript = hostObject.GetComponent(functionName);
            }
            

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