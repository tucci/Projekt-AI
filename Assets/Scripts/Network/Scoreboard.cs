using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard instance;
    public static int killsToWin = 5;

	public struct Playerdata
	{
		public string name;
		public int kills;
		public int deaths;
	}

	public List<Playerdata> listofPlayers;

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start () 
	{
        listofPlayers = new List<Playerdata>();
		listofPlayers = GetScoreBoard ();
        DontDestroyOnLoad(gameObject.transform);
	}

    public List<Playerdata> GetScoreBoard()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
        // Clear so we dont duplicate
        listofPlayers.Clear();
		if (players.Length > 0) 
		{
			foreach (GameObject player in players) 
			{
				if (player != null)
				{
					PlayerClientController pc = player.GetComponent<PlayerClientController> ();
					Playerdata pd;
					pd.name = pc.playerName;
					pd.kills = pc.kills;
					pd.deaths = pc.deaths;
					listofPlayers.Add (pd);

				}
			}
			return listofPlayers;
		} else {
			print ("No players in the scene");
			return listofPlayers;
		}
	}
}