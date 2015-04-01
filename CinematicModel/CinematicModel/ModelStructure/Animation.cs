﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class Animation
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public int Duration { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }
    }
}