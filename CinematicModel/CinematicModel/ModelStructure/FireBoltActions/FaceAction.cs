﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    //The FaceAction is intended to provide the means for controlling times in a single clip corresponding to the dimension of the face.
    //TODO: delineate which clip/animationLayer of Generic animator controller to animate. Only supporting emotion for now so no delineation needed.
    public class FaceAction : FireBoltAction
    {

        [XmlAttribute("emotion")]
        public string Emotion { get; set; }
    }
}