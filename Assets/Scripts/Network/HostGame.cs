using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections.Generic;

public class HostGame : MonoBehaviour
{
	[SerializeField]
	private uint roomSize = 4;

	private string roomName;
	private NetworkManager networkmanager;

	void Start()
	{
		networkmanager = NetworkManager.singleton;
		if (networkmanager.matchMaker == null) 
		{
			networkmanager.StartMatchMaker ();
		}
	}

	public void SetRoomName(string name){
		roomName = name;
	}
		

	public void CreateRoom()
	{
		Debug.Log ("Creating room: " + roomName + " for max: " + roomSize + " players as a player");
		Debug.Log (roomName);
		if (roomName != "" && roomName != null) 
		{
			networkmanager.matchMaker.CreateMatch(roomName, roomSize, true, "", "", "", 0, 0, networkmanager.OnMatchCreate);
		}
	}

	public void CreateServerRoom()
	{
		Debug.Log ("Creating room: " + roomName + " for max: " + roomSize + " players as a server" );
		Debug.Log (roomName);
		if (roomName != "" && roomName != null) 
		{
			networkmanager.matchMaker.CreateMatch(roomName, roomSize, true, "", "", "", 0, 0, OnMatchCreateSever);
		}
	}

	public void OnMatchCreateSever(bool success, string extendedInfo, MatchInfo matchInfo)
	{
		if (success)
		{
			Debug.Log("Sever Match Created");
			print (matchInfo.networkId);
			MatchInfo hostInfo = matchInfo;
			NetworkServer.Listen(hostInfo, 9999);
			NetworkManager.singleton.StartServer(hostInfo);

			OnConnect();
		}
		else
		{
			Debug.Log("ERROR : Match Create Failure");
		}
	}
	void OnConnect ()
	{
		if (Network.isServer)
		{
			Debug.Log("You are Server");
		}
		else if (Network.isClient)
		{
			Debug.Log("You are Client");
		}
		else
		{
			Debug.Log("ERROR : MatchMaking Failed, Peer type is neither Client nor Server");
		}
	}
		
}