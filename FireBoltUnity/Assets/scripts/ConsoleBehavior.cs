﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConsoleBehavior : MonoBehaviour {

	public string command;
	public string[] log;
	public ElPresidente castro;

	// Use this for initialization
	void Start () {
		log = new string[4];

	}
	
	// Update is called once per frame
	void Update () {

		if (Input.inputString != "") {
			if (Input.inputString == "\b")
			{
				if (command != "")
				{
				    command = command.Substring(0, command.Length - 1);
				}
			}
			else
			{
			    command += Input.inputString;
			}
			log[3] = command;

			if (command.IndexOf("\n") != -1 || command.IndexOf("\r") != -1)
			{
				command = command.Trim();
				if (command.StartsWith("goto "))
				{
					string[] tokens = command.Split(new char[] {' '});
					float value = 0.0f;
					if (tokens.GetLength(0) == 2 && float.TryParse(tokens[1], out value))
					{
						castro.goTo(value);
					}
				}
				if (command.StartsWith("+ "))
				{
					string[] tokens = command.Split(new char[] {' '});
					float value = 0.0f;
					if (tokens.GetLength(0) == 2 && float.TryParse(tokens[1], out value))
					{
						castro.goToRel(value);
					}
				}
				for (int i = 0; i < 3; ++i)
				{
					log[i] = log[i+1];
				}
				log[3] = "";
				command = "";
			}
			gameObject.GetComponent<Text>().text = string.Join("\n", log);
		}

	}
}
