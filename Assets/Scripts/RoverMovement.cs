using UnityEngine;
using System.Collections;

public class RoverMovement : MonoBehaviour {
	private HingeJoint[] lWheels;
	private HingeJoint[] rWheels;
	
	private const float MAX_SPEED = 200.0f;
	private const int MOTOR_FORCE = 100;
	
	// Use this for initialization
	void Start () {
		HingeJoint[] wheels = GetComponentsInChildren<HingeJoint>();
		
		for (int i = 0; i < wheels.Length; i++) {
			JointMotor motor = wheels[i].motor;
			motor.force = MOTOR_FORCE;
		
			wheels[i].useMotor = true;
			wheels[i].motor = motor;
		}
		
		int halfWheels = wheels.Length / 2;
		
		lWheels = new HingeJoint[halfWheels];
		rWheels = new HingeJoint[halfWheels];
		
		for (int i = 0; i < wheels.Length / 2; i++) {
			lWheels[i] = wheels[i];
			rWheels[i] = wheels[i + halfWheels];
		}
	}
	
	public void SetMotors(int powerL, int powerR) {
		for (int i = 0; i < lWheels.Length; i++) {
			JointMotor motor = lWheels[i].motor;
			motor.targetVelocity = -PowerToSpeed(powerL);
			lWheels[i].motor = motor;
		}
		
		for (int i = 0; i < rWheels.Length; i++) {
			JointMotor motor = rWheels[i].motor;
			motor.targetVelocity = PowerToSpeed(powerR);
			rWheels[i].motor = motor;
		}
	}
	
	float PowerToSpeed(int power) {
		if (power > 255 || power < -255) return 0.0f;
		return ((float) power / 255.0f) * MAX_SPEED;
	}
}
