using UnityEngine;
using System.Collections;

public class activate_FireBolt : MonoBehaviour {
    public GameObject fireBolt;
    public ElPresidente activate_FireBolt_script;
	// Use this for initialization
	void Start () {
        //fireBolt = GameObject.FindGameObjectWithTag("FireBolt");
        activate_FireBolt_script = fireBolt.GetComponent<ElPresidente>();
        activate_FireBolt_script.Init(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
