using UnityEngine;
using System.Collections;

public class FireBolt_Initialization : MonoBehaviour 
{
	// Use this for initialization
	void Awake () 
    {
        ElPresidente elPresidente = this.GetComponent<ElPresidente>();
        elPresidente.Init(null,40,false,false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
