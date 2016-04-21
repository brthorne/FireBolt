using UnityEngine;
using System.Collections;
using System;

public class FireBolt_Initialization : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
    {
        QualitySettings.vSyncCount = 0;
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-d" && args[i + 1].Equals("debug", StringComparison.OrdinalIgnoreCase))
            {
                ElPresidente.Instance.LogDebugStatements = true;
            }
            
        }
        ElPresidente elPresidente = this.GetComponent<ElPresidente>();
        elPresidente.Init(null,40,false,false,true,false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
