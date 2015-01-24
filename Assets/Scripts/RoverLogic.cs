using UnityEngine;
using System.Collections;

using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;

public class RoverLogic : MonoBehaviour {
	public GameObject sensors;
	private RoverMovement rover;
	private IRSensor irSensor;
	
	private TcpListener server = new TcpListener(IPAddress.Any, 3141);
	private Socket socket = null;
	
	enum RUN_MODES {
		TELEOP, FULL_AUTON, NETWORK, NONE
	};
	
	void Start () {
		server.Start();
		rover = GetComponentInParent<RoverMovement>();
		irSensor = sensors.GetComponentInChildren<IRSensor>();
	}
	
	//Socket Communication
	
	//Logic Start
	
	const float MIN_RANGE = 0.25f;
	RUN_MODES currentRunMode = RUN_MODES.NONE;
	
	int networkL = 0;
	int networkR = 0;
	
	int count = 1024;
	
	StateObject state = null;
	
	void NetworkUpdate() {
		if (server.Pending()) {
			if (socket != null) {
				socket.Disconnect(false);
				socket = null;
			}
			
			if (socket == null) {
				socket = server.AcceptSocket();
				print ("Accepted Socket: " + socket.ToString());
				state = new StateObject(count, socket);
				socket.BeginReceive(state.sBuffer, 0, count, SocketFlags.None, new System.AsyncCallback(ReceiveCallback), state);
			}
		}
	}
	
	bool SocketConnected(Socket s)
	{
		bool part1 = s.Poll(1000, SelectMode.SelectRead);
		bool part2 = (s.Available == 0);
		if (part1 && part2)
			return false;
		else
			return true;
	}
	
	public void ReceiveCallback(System.IAsyncResult asyncReceive) {
		StateObject stateObject = (StateObject)asyncReceive.AsyncState;
		string message = Encoding.ASCII.GetString(stateObject.sBuffer);
		
		if (message != "") {
			string[] messages = message.Split (';');
			
			int state = int.Parse(messages[0].Split(':')[1]);
			
			switch (state) {
			case 1:
				currentRunMode = RUN_MODES.TELEOP;
				break;
			case 2:
				currentRunMode = RUN_MODES.FULL_AUTON;
				break;
			case 3:
				currentRunMode = RUN_MODES.NETWORK;
				break;
			default:
				currentRunMode = RUN_MODES.NONE;
				break;
			}
			
			networkL = int.Parse(messages[1].Split (':')[1]);
			networkR = int.Parse(messages[2].Split (':')[1]);
			
			print (networkL.ToString() + ", " + networkR.ToString());
		}
		stateObject.sBuffer = new byte[count];
		
		stateObject.sSocket.BeginReceive(stateObject.sBuffer, 0, count, SocketFlags.None, new System.AsyncCallback(ReceiveCallback), stateObject);
	}
	
	void Update() {
		NetworkUpdate();
	}
	
	void FixedUpdate() {
		if (Input.GetKeyDown(KeyCode.Alpha0)) currentRunMode = RUN_MODES.NETWORK;
		if (Input.GetKeyDown(KeyCode.Alpha1)) currentRunMode = RUN_MODES.TELEOP;
		if (Input.GetKeyDown(KeyCode.Alpha2)) currentRunMode = RUN_MODES.FULL_AUTON;
	
		if (currentRunMode == RUN_MODES.TELEOP) {
			float aH = Input.GetAxis("Horizontal");
			float aV = Input.GetAxis("Vertical");
			
			int speedL = (int) (aV * 255.0f);
			int speedR = (int) (aH * 255.0f);
			
			rover.SetMotors(speedL, speedR);
		} else if (currentRunMode == RUN_MODES.FULL_AUTON) {
			if (irSensor.GetCurrentSensorVal() > MIN_RANGE) {
				rover.SetMotors(255, 255);
			} else {
				rover.SetMotors(255, -255);
			}
		} else if (currentRunMode == RUN_MODES.NETWORK) {
			rover.SetMotors(networkL, networkR);
		} else {
			rover.SetMotors(0, 0);
		}
	}
}

class StateObject {
	internal byte[] sBuffer;
	internal Socket sSocket;
	
	internal StateObject(int size, Socket sock) {
		sBuffer = new byte[size];
		sSocket = sock;
	}
}