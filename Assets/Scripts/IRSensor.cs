using UnityEngine;
using System.Collections;

public class IRSensor : MonoBehaviour {
	
	float currentSensorVal = 0.0f;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//print (currentSensorVal);
	}
	
	public float GetCurrentSensorVal() {
		return currentSensorVal;
	}
	
	void FixedUpdate() {
		RaycastHit hit;
		Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit);
		Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.red, Time.deltaTime);
		
		currentSensorVal = hit.distance;
	}
}
