using System;
using System.Linq;

using UnityEngine;

using LN.Utilities;

using Impulse.v_1_336;
using Impulse.v_1_336.Sentences;
using Impulse.v_1_336.Constants;
using UintT = Impulse.v_1_336.Intervals.Interval<Impulse.v_1_336.Constants.ValueConstant<uint>, uint>;
using UintV = Impulse.v_1_336.Constants.ValueConstant<uint>;

using CM = CinematicModel;

namespace Assets.scripts
{
    public class ActorActionFactory
    {
        //TODO concatenate looping subsequent animations on the same character into a single action?  would require even longer preprocessing...        
        private static CM.CinematicModel cm;
        private static string[] orderedObjectSets;
        //private static string[] orderedActionTypes;
        private static Story<UintV, UintT, IIntervalSet<UintV, UintT>> story;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storyPlanPath">path to the story plan to load</param>
        /// <param name="cinematicModelPath">path to the cinematic model to load</param>
        /// <returns></returns>
        public static FireBoltActionList CreateStoryActions(Story<UintV, UintT, IIntervalSet<UintV, UintT>> story, CM.CinematicModel cm)
        {
            ActorActionFactory.cm = cm;
            FireBoltActionList aaq = new FireBoltActionList();            
            ActorActionFactory.story = story;
            orderedObjectSets = story.ObjectSetGraph.ReverseTopologicalSort().ToArray();
            //orderedActionTypes = story.ActionTypeGraph.ReverseTopologicalSort().ToArray();

            buildInitialState(aaq);

            //generate FireBolt actions for the steps
            foreach (IStoryAction<UintT> storyAction in story.Actions.Values)
            {
                CM.DomainAction domainAction = getStoryDomainAction(storyAction);
                if (domainAction == null) continue;

                CM.Animation effectingAnimation = getEffectingAnimation(storyAction, domainAction);

                enqueueCreateActions(storyAction, domainAction, effectingAnimation, aaq);
                enqueueAnimateActions(storyAction, domainAction, effectingAnimation, aaq);
                enqueueDestroyActions(storyAction, domainAction, effectingAnimation, aaq);
                enqueuetranslateActions(storyAction, domainAction, effectingAnimation, aaq);
                enqueueRotateActions(storyAction, domainAction, effectingAnimation, aaq);
                enqueueAttachActions(storyAction, domainAction, effectingAnimation, aaq);
            }            
            return aaq;
        }

        private static void buildInitialState(FireBoltActionList aaq) //TODO actor model defaulting a la create actions
        {
            var interval = new UintT(new UintV(0), new UintV(1));
            var initialPositions = from sentence in story.Sentences
                                   where sentence is Predicate
                                   let p = (Predicate)sentence
                                   where p.Temporal &&      
                                         p.Name == "at" &&
                                         p.Time is UintT &&
                                         p.Terms[0] is IConstant &&
                                         p.Terms[1] is IConstant<Coordinate2D> &&
                                         story.IntervalSet.IncludesOrMeetsStartOf<UintV, UintT>((UintT)p.Time, interval) 
                                   select new { Actor = p.Terms[0].Name, Location = (p.Terms[1] as IConstant<Coordinate2D>).Value };

            Debug.Log("building init state creation actions");
            foreach (var initPos in initialPositions)
            {
                Debug.Log(initPos.Actor + ", " + initPos.Location.ToString());
                CM.Actor actor;
                if (!cm.TryGetActor(initPos.Actor,out actor))
                {
                    Debug.Log("actor [" + initPos.Actor + "] not found in cinematic model.");
                    continue;
                }

                string modelFileName = actor.Model;
                if (string.IsNullOrEmpty(modelFileName))
                {
                    Debug.Log("model name for actor[" + initPos.Actor + "] not found in cinematic model.");
                    continue;
                }
                aaq.Add(new Create(0, initPos.Actor, modelFileName, initPos.Location.ToVector3()));
            }
        }

        private static void enqueueRotateActions(IStoryAction<UintT> storyAction, CM.DomainAction domainAction, 
                                                 CM.Animation effectingAnimation, FireBoltActionList aaq)
        {
            foreach (CM.RotateAction ra in domainAction.RotateActions)
            {
                float startTick = 0;
                float endTick = 0;
                string actorName = null;
                float? targetDegrees = null;
                Vector2? targetPoint = null;
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == ra.ActorNameParamName)
                    {
                        if (!getActionParameterValue(storyAction, domainActionParameter, out actorName))
                        {
                            break;
                        }
                    }
                    else if (domainActionParameter.Name == ra.DestinationParamName)
                    {
                        IActionProperty targetOrientation;
                        if (storyAction.TryGetProperty(domainActionParameter.Name, out targetOrientation))
                        {
                            if (targetOrientation.Value.Value is float)
                            {
                                targetDegrees = (float)targetOrientation.Value.Value;
                                if (targetOrientation.Range.Name == "x+degrees")
                                    targetDegrees = targetDegrees.Value.convertSourceEngineToUnityRotation();
                            }
                            else if (targetOrientation.Value.Value is Coordinate2D)
                            {
                                targetPoint = new Vector2((float)((Coordinate2D)targetOrientation.Value.Value).X,
                                                          (float)((Coordinate2D)targetOrientation.Value.Value).Y);
                            }
                        }
                        else
                        {
                            Debug.LogError("orientation not set for stepId[" + storyAction.Name + "]");
                        }
                    }
                }
                startTick = getStartTick(storyAction, ra, effectingAnimation);
                endTick = getEndTick(storyAction, ra, effectingAnimation, startTick);
                var targetRotation = new Vector3Nullable(null, targetDegrees, null);
                if (Rotate.ValidForConstruction(actorName, targetRotation, targetPoint))
                {                    
                    aaq.Add(new Rotate(startTick, endTick, actorName, targetRotation, targetPoint));
                }                
            }
        }

        private static void enqueuetranslateActions(IStoryAction<UintT> storyAction, CM.DomainAction domainAction, 
                                               CM.Animation effectingAnimation, FireBoltActionList aaq)
        {
            foreach (CM.TranslateAction ta in domainAction.TranslateActions)
            {
                float startTick = 0;
                float endTick = 0;
                string actorName = null;
                Vector3Nullable destination = new Vector3Nullable(null, null, null);
                Vector3 origin = Vector3.zero;
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == ta.OriginParamName)
                    {
                        IActionProperty coord;
                        if (storyAction.TryGetProperty(domainActionParameter.Name, out coord))
                        {
                            if (coord.Value.Value is Coordinate2D)
                                origin = ((Coordinate2D)coord.Value.Value).ToVector3(cm.DomainDistancePerEngineDistance);
                            else if (coord.Value.Value is Coordinate3D)
                                origin = ((Coordinate3D)coord.Value.Value).ToVector3(cm.DomainDistancePerEngineDistance);
                        }
                        else
                        {
                            Debug.LogError("origin not set for stepId[" + storyAction.Name + "]");
                        }
                    }
                    else if (domainActionParameter.Name == ta.DestinationParamName)
                    {
                        IActionProperty coord;
                        if (storyAction.TryGetProperty(domainActionParameter.Name, out coord))
                        {
                            if(coord.Value.Value is Coordinate2D)
                                destination = ((Coordinate2D)coord.Value.Value).ToVector3Nullable(cm.DomainDistancePerEngineDistance);
                            else if (coord.Value.Value is Coordinate3D)
                                destination = ((Coordinate3D)coord.Value.Value).ToVector3Nullable(cm.DomainDistancePerEngineDistance);                            
                        }
                        else
                        {
                            Debug.LogError("destination not set for stepId[" + storyAction.Name + "]");
                        }
                    }
                    else if (domainActionParameter.Name == ta.ActorNameParamName)
                    {
                        if (!getActionParameterValue(storyAction, domainActionParameter, out actorName))
                        {
                            break;
                        }
                    }
                }
                startTick = getStartTick(storyAction,ta,effectingAnimation);
                endTick = getEndTick(storyAction, ta, effectingAnimation, startTick);
                if (Translate.ValidForConstruction(actorName))
                {
                    aaq.Add(new Translate(startTick, endTick, actorName, origin, destination));
                }
            }
        }

        private static CM.Animation getEffectingAnimation(IStoryAction<UintT> storyAction, CM.DomainAction domainAction)
        {
            //find effector if any
            CM.AnimateAction effectorAnimateAction = domainAction.AnimateActions.Find(x => x.Effector);
            //didn't find an effector for this domain action...move along; nothing to see here
            if (effectorAnimateAction == null) return null;

            string effectorActorName = null;
            CM.AnimationMapping effectorAnimationMapping = null;
            CM.Animation effectingAnimation = null;
            foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
            {
                if (domainActionParameter.Name == effectorAnimateAction.ActorNameParamName)
                {
                    IActionProperty actorNameProperty;
                    if(storyAction.TryGetProperty(domainActionParameter.Name, out actorNameProperty))
                    {
                        effectorActorName = actorNameProperty.Value.Name; 
                    }
                    if (effectorActorName == null)
                    {
                        Debug.LogError("actorName not set for stepId[" + storyAction.Name + "]");
                        return null;
                    }
                    CM.Actor effectorActor;
                    if (!cm.TryGetActor(effectorActorName,out effectorActor))
                    {
                        Debug.Log(string.Format("effector actor [{0}] undefined for step[{1}]",effectorActorName,storyAction.Name));
                        return null;
                    }
                    if(!effectorActor.TryGetAnimationMapping(effectorAnimateAction.Name, out effectorAnimationMapping))
                    {
                        Debug.Log("cinematic model animation instance undefined for actor[" +
                            effectorActorName + "] action[" + domainAction.Name + "] paramName[" + domainActionParameter.Name + "]");
                        return null;
                    }
                    effectingAnimation = cm.FindAnimation(effectorAnimationMapping.AnimationName);
                    if (effectingAnimation == null)
                    {
                        Debug.LogError(string.Format("animation name [{0}] undefined.", effectingAnimation));
                    }
                }
            }
            return effectingAnimation;
        }

        private static void enqueueAnimateActions(IStoryAction<UintT> storyAction, CM.DomainAction domainAction, 
                                                  CM.Animation effectingAnimation, FireBoltActionList aaq)
        {            
            foreach(CM.AnimateAction animateAction in domainAction.AnimateActions)
            {
                string actorName = null;
                float startTick = 0;
                float endTick = 0;
                CM.AnimationMapping animMapping = null;
                CM.AnimationMapping stateMapping = null;
                CM.Animation animation = null;

                string endName = !string.IsNullOrEmpty(animateAction.End) ? animateAction.End : string.Empty;
                string animateActionName = animateAction.Name;             

                //PURPOSE: if domain action parameter is the name of an animateAction for that domain action as defined in the cinematic model,
                //then use the parameter value as the animateAction name.
                // Used when a domain action has a variable for accepting an action to play, handy for spawn actions that require an initial state
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    endName = string.Empty;
                    //  Debug.Log("beforeset: " + animateAction.Name + " " +  domainActionParameter.Name);
                    if (domainActionParameter.Name.Equals(animateAction.Name))
                    {
                        getActionParameterValue(storyAction, domainActionParameter, out animateActionName);
                        endName = animateActionName;                        
                        break;
                    }
                }

                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {

                    if (domainActionParameter.Name == animateAction.ActorNameParamName)
                    {
                        if (getActionParameterValue(storyAction, domainActionParameter, out actorName))
                        {
                            int objectSetIndex = 0;
                            int actorHierarchyStepLevel = 1;
                            string abstractActorName = actorName;
                            getAnimationMapping(abstractActorName, animateActionName, out animMapping);

                            //TODO extract method
                            //abstract actor lookup if we didn't find an exact actor name match
                            while (objectSetIndex < orderedObjectSets.Length &&
                                  actorHierarchyStepLevel < cm.SmartModelSettings.ActorMaxSearchDepth &&
                                  animMapping == null)
                            {

                                if (story.ObjectSets[orderedObjectSets[objectSetIndex]].
                                    Contains(new ClassConstant<string>(actorName)))
                                {
                                    actorHierarchyStepLevel++;
                                    abstractActorName = orderedObjectSets[objectSetIndex];
                                    if (getAnimationMapping(abstractActorName, animateActionName, out animMapping))
                                    {
                                        break;
                                        //found our mapping
                                    }
                                }
                                objectSetIndex++;
                            }


                            if (animMapping == null)
                            {
                                Debug.Log("cinematic model animation instance undefined for actor[" +
                                    abstractActorName + "] animateAction[" + animateActionName + "]");
                                break;
                            }
                            //we have a valid mapping, let's use it to find an animation for this actor 
                            animation = cm.FindAnimation(animMapping.AnimationName);
                            if (animation == null)
                            {
                                Debug.Log(string.Format("animation name [{0}] undefined", animMapping.AnimationName));
                                break;
                            }

                            //end name is optional, we don't have to do this for the animation to be valid
                            if (!string.IsNullOrEmpty(endName))
                            {
                                getAnimationMapping(actorName, endName, out stateMapping);
                                if (!(stateMapping == null))
                                {
                                    endName = cm.FindAnimation(stateMapping.AnimationName).FileName;
                                    //Debug.Log("end name: " + endName);
                                }
                            }

                        }
                    }
                }
                startTick = getStartTick(storyAction, animateAction, effectingAnimation);
                endTick = getEndTick(storyAction, animateAction, effectingAnimation, startTick);

                if (AnimateMecanim.ValidForConstruction(actorName, animation))
                {
                 //   Debug.Log("actor: " + actorName + " animMappingName: " + animMapping.AnimationName + " animateActionName: " + animMapping.AnimateActionName + " loop: " + animMapping.LoopAnimation);
                    aaq.Add(new AnimateMecanim(startTick, endTick, actorName, animation.FileName, animMapping.LoopAnimation, endName));
                }
            }
        }

        private static float getStartTick(IStoryAction<UintT> storyAction, CM.FireBoltAction fireBoltAction, CM.Animation effectingAnimation)
        {
            float startTick = 0;
            startTick = storyAction.Time.Start.ToMillis(cm.MillisPerTick);
            startTick += getEffectorAnimationOffset(effectingAnimation, fireBoltAction);
            return startTick;
        }

        private static float getEndTick(IStoryAction<UintT> storyAction, CM.FireBoltAction fireBoltAction, CM.Animation effectingAnimation, float startTick)
        {
            float endTick = storyAction.Time.End.ToMillis(cm.MillisPerTick);
            if (fireBoltAction.MaxDuration.HasValue &&
                fireBoltAction.MaxDuration.Value < storyAction.Time.End - storyAction.Time.Start)
            {
                endTick = startTick + fireBoltAction.MaxDuration.Value;
            }
            return endTick;
        }

        private static float getEffectorAnimationOffset(CM.Animation effectingAnimation, CM.FireBoltAction fireBoltAction)
        {
            float offset = 0;
            if (effectingAnimation != null)
            {
                CM.AnimationIndex effectingIndex = effectingAnimation.AnimationIndices.Find(x => x.Name == fireBoltAction.EffectorOffsetIndexName);
                if (effectingIndex != null)
                {
                    offset = effectingIndex.TimeOffset;
                }
            }
            return offset;
        }

        private static bool getActionParameterValue(IStoryAction<UintT> storyAction, CM.DomainActionParameter domainActionParameter, out string parameterValueName)
        {
            parameterValueName = null;
            IActionProperty ParamNameProperty;
            if (storyAction.TryGetProperty(domainActionParameter.Name, out ParamNameProperty))
            {
                parameterValueName = ParamNameProperty.Value.Name;
                return true;
            }
            Debug.Log(domainActionParameter.Name + " not set for stepId[" + storyAction.Name + "]");
            return false;
        }

        private static bool getActorModel(string actorName, out string modelFileName)
        {
            modelFileName = null;
            CM.Actor actor;
            if (!cm.TryGetActor(actorName, out actor))
            {
                Debug.Log("actor[" + actorName + "] not found in cinematic model");
            }
            else
            {                
                if (actor.Model != null)
                {                    
                    modelFileName = actor.Model;
                    return true;
                }
                else
                {
                    Debug.Log("model name for actor[" + actorName + "] not found in cinematic model.");
                }
            }
            return false;
        }

        private static bool getAnimationMapping(string actorName, string animateActionName, out CM.AnimationMapping animationMapping)
        {
            animationMapping = null;
            CM.Actor actor = null;
            if (cm.TryGetActor(actorName, out actor) &&
                actor.TryGetAnimationMapping(animateActionName, out animationMapping))
            {
                return true;
            }
            return false;
        }

        private static void enqueueCreateActions(IStoryAction<UintT> storyAction, CM.DomainAction domainAction, CM.Animation effectingAnimation, FireBoltActionList aaq )
        {
            foreach (CM.CreateAction ca in domainAction.CreateActions)
            {
                float startTick = 0;
                string actorName = null;
                string modelName = null;
                Vector3 destination = new Vector3();
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == ca.ActorNameParamName)
                    {
                        if (getActionParameterValue(storyAction, domainActionParameter, out actorName))//actorName is defined, we can look up a model
                        {
                            getActorModel(actorName, out modelName);
                            int objectSetIndex = 0;
                            int actorHierarchyStepLevel = 1;                           
                            while(string.IsNullOrEmpty(modelName) &&
                                  objectSetIndex < orderedObjectSets.Length &&
                                  actorHierarchyStepLevel <= cm.SmartModelSettings.ActorMaxSearchDepth)
                            {
                                if (story.ObjectSets[orderedObjectSets[objectSetIndex]].
                                        Contains(new ClassConstant<string>(actorName)) )
                                {
                                    actorHierarchyStepLevel++;
                                    if (getActorModel(orderedObjectSets[objectSetIndex], out modelName))
                                    {
                                        break;//quit looking up the hierarchy.  we found a more generic actor
                                    }
                                }
                                objectSetIndex++;
                            }
                            if (string.IsNullOrEmpty(modelName))
                            {
                                break;//didn't find actor definition.  give up on this create action and move to the next one
                            }
                        }
                    }
                    else if (domainActionParameter.Name == ca.OriginParamName)
                    {
                        IActionProperty coord;
                        if(storyAction.TryGetProperty(domainActionParameter.Name, out coord))
                        {
                            destination = ((Coordinate2D)coord.Value.Value).ToVector3();
                        }
                        else
                        {
                            Debug.LogError("origin not set for stepId[" + storyAction.Name + "]");
                        }                        
                    }
                }
                startTick = getStartTick(storyAction, ca, effectingAnimation);                
                if(Create.ValidForConstruction(actorName,modelName))
                {
                    aaq.Add(new Create(startTick, actorName, modelName, destination));
                }                
            }
        }

        private static void enqueueDestroyActions(IStoryAction<UintT> storyAction, CM.DomainAction domainAction, 
                                                    CM.Animation effectingAnimation, FireBoltActionList aaq)
        {
            foreach (CM.DestroyAction da in domainAction.DestroyActions)
            {
                float startTick = 0;
                string actorName = null;
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == da.ActorNameParamName)
                    {
                        if (!getActionParameterValue(storyAction, domainActionParameter, out actorName))
                        {
                            break;
                        }
                    }
                }
                startTick = getStartTick(storyAction,da,effectingAnimation);
                if (Destroy.ValidForConstruction(actorName))
                {
                    aaq.Add(new Destroy(startTick, actorName));
                }
            }
        }

        private static void enqueueAttachActions(IStoryAction<UintT> storyAction, CM.DomainAction domainAction, CM.Animation effectingAnimation, FireBoltActionList aaq)
        {
            foreach (CM.AttachAction aa in domainAction.AttachActions)
            {
                float startTick = 0;
                string actorName = null;
                string parentName = null;
                bool attach = false;                
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == aa.ActorNameParamName)
                    {
                        getActionParameterValue(storyAction, domainActionParameter, out actorName);
                        //TODO fail gracefully if we don't find actor param value                       
                    }
                    else if (domainActionParameter.Name == aa.ParentParamName)
                    {
                        getActionParameterValue(storyAction, domainActionParameter, out parentName);                       
                        //TODO fail gracefully if we don't find parent param value
                    }                    
                }
                attach = aa.Attach;
                startTick = getStartTick(storyAction, aa, effectingAnimation);
                if (Create.ValidForConstruction(actorName, parentName))
                {
                    aaq.Add(new Attach(startTick, actorName, parentName, attach));
                }
            }
        }

        private static CM.DomainAction getStoryDomainAction(IStoryAction action)
        {
            //check if the step action is in the domain of cinematic model
            foreach(CM.DomainAction domainAction in cm.DomainActions)
            {
                if(string.Equals(domainAction.Name,action.ActionType.Name,StringComparison.OrdinalIgnoreCase))
                {
                    return domainAction;
                }
            }
            return null;
        }
    }
}
