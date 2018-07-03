using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEnd : MonoBehaviour
{

    public Text gameEndText;
    public Button goToMatchMakerButton;

	// Use this for initialization
	void Start ()
	{
        //gameEndText.text = EndGameData.playerWhoWon + " won!";
        gameEndText.text = "Match Over!";
        goToMatchMakerButton.onClick.AddListener(OnMatchMakerClick);
        
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    void OnMatchMakerClick()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
