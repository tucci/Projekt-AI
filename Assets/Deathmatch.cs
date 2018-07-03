using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Deathmatch : NetworkBehaviour {

    public float gameTimeInMinutes = 5.0f;
    
    

    public float currentime;


    [SerializeField]
    private Text gameTimerText;
    public GameState gamestate;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    

    // Use this for initialization
        void Start () {
        currentime = gameTimeInMinutes * 60;
        gamestate = GameState.LOBBY;
    }
	
	// Update is called once per frame
	void Update () {
        if (gamestate == GameState.INGAME)
        {
            currentime -= Time.deltaTime;
            gameTimeInMinutes = currentime / 60.0f;


            string gametimenow = getFormattedTime(gameTimeInMinutes);

            SetTime(gametimenow);
            if (currentime <= 0)
            {
                gamestate = GameState.GAMEOVER;
            }
        }
        if (gamestate == GameState.GAMEOVER)
        {
            // TODO: have some sort of ui show game over
        }

    }

    public void StartGame()
    {
        gamestate = GameState.INGAME;
    }

    

    public void SetTime(string time)
    {
        if (isServer)
        {
            RpcSetTime(time);
        }
        
    }

    [ClientRpc]
    void RpcSetTime(string time)
    {
        gameTimerText.text = time;
    }

    public string getFormattedTime(float timeInMinutes)
    {
        TimeSpan timeSpan = TimeSpan.FromMinutes(timeInMinutes);
        string timeText = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        print(timeText);
        return timeText;
    }


}
