using LN.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Text;

namespace CinematicModel
{


    public class FunctionAction : FireBoltAction
    {
        public FunctionAction()
        {
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlArray("args")]
        [XmlArrayItem("arg")]
        public List<Tuple<string, string, string>> tupleList { get; set; }
    }
}
