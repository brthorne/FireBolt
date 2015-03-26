﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public abstract class ActionDecorator : IActorAction
    {
        private IActorAction nestedAction;
        private float startTick;
        private float? endTick;

        protected ActionDecorator(float startTick, float? endTick, IActorAction nestedAction)
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.nestedAction = nestedAction;
        }

        public IActorAction NestedAction { 
            get
            {
                return nestedAction;
            } 
            set
            {
                //should i log or throw exceptions when we try to assign
                //to an already existing action and fail?
                if (nestedAction == null)
                {
                    nestedAction = value;
                }
            }
        }
                
        public float StartTick()
        {
            return startTick;
        }

        public float? EndTick()
        {
            return endTick;
        }

        public void Execute()
        {
            if (nestedAction != null)
            {
                nestedAction.Execute();
            }
            execute();
        }

        private abstract void execute();

        public void Stop()
        {
            if (nestedAction != null)
            {
                nestedAction.Stop();
            }
            stop();
        }

        private abstract void stop();

        public void Init()
        {
            if (nestedAction != null)
            {
                nestedAction.Init();
            }
            init();
        }

        private abstract void init();
    }
}
