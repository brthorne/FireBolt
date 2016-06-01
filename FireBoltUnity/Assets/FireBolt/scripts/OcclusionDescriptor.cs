using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{
    public enum Opacity
    {
        None,
        Low,
        Medium,
        High
    }

    public class OcclusionDescriptor : MonoBehaviour
    {
        public Opacity colliderOpacity;
    }
}
