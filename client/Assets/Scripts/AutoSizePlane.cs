using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSizePlane : MonoBehaviour {

	[SerializeField]
	private Camera camera;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		transform.localScale = new Vector3(camera.aspect, 1.0f, 1.0f);
	}
}
