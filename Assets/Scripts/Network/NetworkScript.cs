using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using PlayerController = UnityEngine.Networking.PlayerController;
using System;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public enum GameState {LOBBY= 0, INGAME = 1, GAMEOVER = 2};

public class NetworkScript : NetworkManager
{

	public float gameTimeInMinutes = 5.0f;
	public static int MAX_PLAYERS = 4;
	public int playersToStartGame = 2;
	public GameState gamestate;
	float currentime;

	[SerializeField]
	private InputField playerTextBox;
	string name;

	public void SetPlayerName(){
		name = playerTextBox.text;

	}


    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("Server start");
    }

	void Start(){
		gamestate = GameState.LOBBY;
		currentime = gameTimeInMinutes * 60;
	}

	void Update()
	{
		if (gamestate == GameState.INGAME) 
		{
			currentime -= Time.deltaTime;
			gameTimeInMinutes = currentime / 60.0f;
			string gametimenow = getFormattedTime (gameTimeInMinutes);
			if (currentime <= 0) {
				gamestate = GameState.GAMEOVER;
			}
		}
		if (gamestate == GameState.GAMEOVER) {
			// TODO: have some sort of ui show game over
		}
	}

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
		Debug.Log("On client connect");
		ClientScene.AddPlayer(conn, 0);


    }

	public string getFormattedTime(float timeInMinutes)
	{
		TimeSpan timeSpan = TimeSpan.FromMinutes(timeInMinutes);
		string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		print (timeText);
		return timeText;
	}


    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
		Debug.Log("on server add player");
		if (singleton.numPlayers > playersToStartGame) 
		{
			gamestate = GameState.INGAME;

		}
		GameObject obj = GameObject.Instantiate(playerPrefab) as GameObject;
		if (obj != null) {
			Debug.Log("Changing player name to " + name);

			obj.GetComponent<PlayerClientController> ().playerName = name;
		}
       
        NetworkServer.AddPlayerForConnection(conn, obj, playerControllerId);
    }
}