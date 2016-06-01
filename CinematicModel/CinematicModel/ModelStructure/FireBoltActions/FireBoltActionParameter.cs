using System.Xml.Serialization;

namespace CinematicModel
{
    public enum ParameterAbstraction
    {
        /// <summary>
        /// associated value is the name of a parameter in the domain action
        /// whose value should be looked up in the impulse plan and used in 
        /// the generated FireBolt action
        /// </summary>
        Abstract,
        /// <summary>
        /// associated value should be used directly in the FireBoltAction parameter.
        /// particularly handy for actions that always spawn the same thing implicitly
        /// or actions with a static time offset
        /// </summary>
        Absolute,

    }

    public enum ParameterAttribute
    {
        /// <summary>
        /// do not attempt to address a subcomponent of the associated value
        /// </summary>
        Root,
        /// <summary>        
        /// treat the associated parameter's value as an actor and capture its position
        /// as the parameter of the FireBolt action
        /// </summary>
        PositionOf,
        /// <summary>
        /// treat the associated parameter's value as an actor and capture its position
        /// as the parameter of the FireBolt action
        /// </summary>
        OrientationOf,
    }

    public class FireBoltActionParameter
    {
        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("abstraction")]
        public ParameterAbstraction Abstraction { get; set; }

        [XmlAttribute("attribute")]
        public ParameterAttribute Attribute { get; set; }

        //for absolute params, how do we decide type information?  
        //we could just support one default and try to dodge the problem
        //or we could try to do the same sort of typing system that occurs in impulse...which seems horrendously complicated
        //(perhaps i should look at the code more and see if it actually is)
    }
}
