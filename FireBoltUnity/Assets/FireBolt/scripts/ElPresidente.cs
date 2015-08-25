﻿using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.IO;
using System.Collections;
using Assets.scripts;
using System.Collections.Generic;
using System;
using Impulse.v_1_336;
using UintT = Impulse.v_1_336.Interval<Impulse.v_1_336.Constants.ValueConstant<uint>, uint>;
using UintV = Impulse.v_1_336.Constants.ValueConstant<uint>;


public class ElPresidente : MonoBehaviour {

    FireBoltActionList actorActionList;
    FireBoltActionList cameraActionList;
    FireBoltActionList executingActions;
    private float lastTickLogged;
    private float totalTime;
    public Text debugText;
	public float myTime;
    public Slider whereWeAt;
    public static readonly ushort MILLIS_PER_FRAME = 5;
    private AStory<UintV, UintT, IIntervalSet<UintV, UintT>> story;

    public static ElPresidente Instance;

    private AssetBundle actorsAndAnimations = null;
    private AssetBundle terrain = null;
    private bool initialized = false;

    /// <summary>
    /// FireBolt point of truth for time.  updated with but independent of time.deltaTime
    /// expressed in milliseconds
    /// </summary>
    public static float currentTime;

    public void Init(float a)
    {
        Init();
    }

/// <summary>
/// 
/// </summary>
/// <param name="newStoryPlanPath"></param>
/// <param name="newCameraPlanPath"></param>
/// <param name="newCinematicModelPath"></param>
/// <param name="newActorAndAnimationBundlePath"></param>
    public void Init(string storyPlanPath = "Assets/storyPlans/defaultStory.xml", string cameraPlanPath = "Assets/cameraPlans/defaultCamera.xml", 
                     string cinematicModelPath = "Assets/cinematicModels/defaultModel.xml", 
                     string actorsAndAnimationsBundlePath = "AssetBundles/actorsAndAnimations", string terrainBundlePath = "AssetBundles/terrain")
    {      

        executingActions = new FireBoltActionList(new ActionTypeComparer());
        loadStructuredImpulsePlan(storyPlanPath);
        actorActionList = ActorActionFactory.CreateStoryActions(story, cinematicModelPath);
        cameraActionList = CameraActionFactory.CreateCameraActions(story, cameraPlanPath);
        currentTime = 0;
        //find total time for execution. not sure how to easily find this without searching a lot of actions
        totalTime = 0;
        if (actorActionList.Count > 0)
            totalTime = actorActionList[actorActionList.Count - 1].EndTick() - actorActionList[0].StartTick();

        Instance = this;
        actorsAndAnimations = AssetBundle.CreateFromFile(actorsAndAnimationsBundlePath);
        terrain = AssetBundle.CreateFromFile(terrainBundlePath);
        initialized = true;
    }

    private void loadStructuredImpulsePlan(string storyPlanPath)
    {
        Debug.Log("begin story plan xml load");
        var xml = Impulse.v_1_336.Xml.Story.LoadFromFile(storyPlanPath);
        Debug.Log("end story plan xml load");
        var factory = Impulse.v_1_336.StoryParsingFactories.GetUnsignedIntergerIntervalFactory();
        Debug.Log("begin story plan parse");
        story = factory.ParseStory(xml, false);//TODO true! get crackin with that validation, colin!
        Debug.Log("end story plan parse");
    }

    public AssetBundle GetActiveAssetBundle()
    {
        if (actorsAndAnimations == null)
        {
            Debug.Log("attempting to load from asset bundle before it is set. " +
                      "use ElPresidente.SetActiveAssetBundle() to load an asset bundle");
            return null;
        }
        return actorsAndAnimations;
    }

    public void togglePause()
    {
        if (Time.timeScale < float.Epsilon)
            Time.timeScale = 1f;
        else
            Time.timeScale = 0f;
    }

    public void speedToggle()
    {
        Time.timeScale = (Time.timeScale + 1f) % 4;        
    }

    public void setTime(float targetPercentComplete)
    {        
        if (Mathf.Abs(targetPercentComplete * totalTime - currentTime) > MILLIS_PER_FRAME)
            goTo (targetPercentComplete * totalTime);
    }

    void Update()
    {
        if (!initialized)
            return;
        currentTime += Time.deltaTime * 1000;
        if(debugText != null)
            debugText.text = currentTime.ToString();
        if (whereWeAt && currentTime < totalTime)
            whereWeAt.value = currentTime / totalTime;
		myTime = currentTime;  
        logTicks();

        updateFireBoltActions(actorActionList);
        updateFireBoltActions(cameraActionList);
    }

    void updateFireBoltActions(FireBoltActionList actions)
    {
        List<IFireBoltAction> removeList = new List<IFireBoltAction>();
        foreach (IFireBoltAction actorAction in executingActions)
        {
            if (actorActionComplete(actorAction) || actorAction.StartTick() > currentTime)
            {
                actorAction.Stop();
                removeList.Add(actorAction);
            }
        }
        foreach (IFireBoltAction action in removeList)
        {
            executingActions.Remove(action);
        }
        while (actions.NextActionIndex < actions.Count && actions[actions.NextActionIndex].StartTick() <= currentTime) //TODO should probably encapsulate some more of this stuff in the list class
        {
            IFireBoltAction action = actions[actions.NextActionIndex];
            actions.NextActionIndex++;
            if (action.Init())
			{
                if (actorActionComplete(action))
                    action.Skip();                
                else
                    executingActions.Add(action);                     
			}
        }
    }

    void rewindFireBoltActions(FireBoltActionList actions)
    {
        int currentIndex = actions.NextActionIndex-1;//next action was pointed to...next action!
        actions.NextActionIndex = 0;
        while (actions.NextActionIndex < actions.Count &&
               actions[actions.NextActionIndex].EndTick() < currentTime)
        {
            actions.NextActionIndex++;
        }

        while (currentIndex >= actions.NextActionIndex)
        {
            actions[currentIndex].Undo();
            currentIndex--;
        }
        Debug.Log ("rewind to " + actions.NextActionIndex + ": " + actions[actions.NextActionIndex]);
    }

    void fastForwardFireBoltActions(FireBoltActionList actions)
    {
        List<IFireBoltAction> removeList = new List<IFireBoltAction>();
        foreach (IFireBoltAction actorAction in executingActions)
        {
            if (actorActionComplete(actorAction))
            {
                actorAction.Skip();
                removeList.Add(actorAction);
            }
        }
        foreach (IFireBoltAction action in removeList)
        {
            executingActions.Remove(action);
        }
        while (actions.NextActionIndex < actions.Count && actions[actions.NextActionIndex].StartTick() <= currentTime) //TODO should probably encapsulate some more of this stuff in the list class
        {
            IFireBoltAction action = actions[actions.NextActionIndex];
            actions.NextActionIndex++;
            if (action.Init())
            {
                if (!actorActionComplete(action))
                    executingActions.Add(action);
                else
                    action.Skip();
            }
        }
    }

    void LateUpdate()
    {
        if (!initialized)
            return;
        foreach (IFireBoltAction actorAction in executingActions)
        {
            actorAction.Execute();
        }
    }

    public float getCurrentTime()
    {
        return currentTime;
    }

	public void goTo(float time)
	{
        if (time < 0)
            time = 0;
        Debug.Log ("goto " + time);
        lastTickLogged = time;
		if (time < currentTime)
        {
            currentTime = time;
            rewindFireBoltActions(actorActionList);
            rewindFireBoltActions(cameraActionList);
        }
        else
        {
            currentTime = time;
            fastForwardFireBoltActions(actorActionList);
            fastForwardFireBoltActions(cameraActionList);
        }
		currentTime = time;
	}

    public void scaleTime(float scale)
    {
        Time.timeScale = scale;
    }

	public void goToRel(float time)
	{
		goTo(currentTime + time);
	}

    void logTicks()
    {
        if (currentTime - lastTickLogged > 1000)
        {
            Debug.Log(currentTime);
            lastTickLogged = currentTime;
        }
    }

    bool actorActionComplete(IFireBoltAction iaa)
    {
        return iaa.EndTick() < currentTime;
    }
}