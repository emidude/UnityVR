using System;
using UnityEngine;
using System.IO;


public class CrashReporter: MonoBehaviour
{
	private void Awake()
	{
		Application.logMessageReceived += OnLogMessageReceived;
	}

	private void OnLogMessageReceived (string condition, string stackTrace, LogType type)
	{
		if(type == LogType.Error || type == LogType.Exception)
		{
			SaveCrash(condition, stackTrace);
		}
	}

	private void SaveCrash (string condition, string stackTrace)
	{
		string logPath = Application.persistentDataPath + "/crash.txt";
		string log = condition + "\n" + stackTrace;
		Debug.Log("saving crash to " + logPath);
		File.AppendAllText(logPath, log);
	}
}


