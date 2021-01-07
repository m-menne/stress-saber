using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.IO;

// Handles the connection to the heartrate server.
public class NetworkManager : MonoBehaviour {  		
	private TcpClient socketConnection; 	// TCP client which sets the socket connection.
	private Thread clientReceiveThread;     // Thread which listens for new data.
	private ScoreBoard scoreBoard;          // The scoreboard script.
	private int heart_rate;					// Current heart rate.	

	// IP address and port of the server socket.
	private string ipAddress = "192.168.1.84";
	private int port = 1111;

	// Use this for initialization 	
	void Start () {
		ConnectToTcpServer();   // Setup TCP server.
		scoreBoard = GameObject.Find("ScoreBoard").GetComponent<ScoreBoard>();  // Set reference of the scoreboard script.
	}  	
	// Update is called once per frame
	void Update () {         
		scoreBoard.Heart_rate = heart_rate;		// Update heart rate in scoreBoard here, since ListenForData runs in clientReceiveThread!
	}  	
	
	// Setup socket connection. 		
	private void ConnectToTcpServer () { 		
		try {  			
			clientReceiveThread = new Thread (new ThreadStart(ListenForData));  // Setup new thread which listens for new data.
			clientReceiveThread.IsBackground = true;                            // Let thread run in background.
			clientReceiveThread.Start();  		                                // Start the thread.
		} 		
		catch (Exception e) {   // Catch and log any exceptions.
			Debug.Log("On client connect exception " + e); 		
		} 	
	}  	
	
	// Runs in background clientReceiveThread; Listens for incomming data. 
	private void ListenForData() { 		
		try { 			
			socketConnection = new TcpClient(ipAddress, port);     // Starts socket connection to heartrate server.
			Byte[] bytes = new Byte[1024];                              // Array for storing the incomming data.
			while (true) { 				
				// Create a stream object for reading incomming data.
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length];
						// Convert received data to heart rate.
						heart_rate = BitConverter.ToInt32(bytes, 0);
					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	}  	
}
