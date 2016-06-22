﻿using UnityEngine;
using System.Linq;
using System;

namespace Assets.scripts
{
    public class Attach : FireBoltAction
    {
        string actorName,parentName;
		GameObject actor, parent;
        bool attach;

        public static bool ValidForConstruction(string actorName, string parentName)
        {
            if (string.IsNullOrEmpty(actorName) || string.IsNullOrEmpty(parentName))
                return false;
            return true;
        }

        public override string ToString ()
        {
            return string.Format ("Attach[{2}] actor[{0}] to parent[{1}]", actorName, parentName, attach);
        }

        public Attach(float startTick, string actorName, string parentName, bool attach) :
            base(startTick, startTick)
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.parentName = parentName;
            this.attach = attach;
        }

        public override bool Init()
        {
            Debug.Log("init " + ToString().AppendTimestamps());
            if (actor == null &&
                !getActorByName(actorName, out actor))
            {
                Extensions.Log("attach failed to find actor[{0}] to attach", actorName);
                return false;                              
            }

            if (parent == null &&
                !getActorByName(parentName, out parent))
            {
                Extensions.Log("attach failed to find parent[{0}] to attach to", parentName);
                return false;

            }

            performAttach(attach);
            actor.SetActive(true);
            return true;
        }

        private void performAttach(bool a)
        {
            if (a)
            {
                actor.transform.SetParent(parent.transform);
            }
            else
            {
                actor.transform.SetParent(GameObject.Find("InstantiatedObjects").transform);
            }
        }

        public override void Undo()
		{
            Extensions.Log("Undo " + ToString());
            performAttach(!attach);
		}

        public override void Skip()
        {
            // nothing to skip
        }

        public override void Execute(float currentTime)
        {
            //nothing to do
        }

        public override void Stop()
        {
            //nothing to stop
        }

        public override string GetMainActorName()
        {
            return actorName;
        }
    }
}
