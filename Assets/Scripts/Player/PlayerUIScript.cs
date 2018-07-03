using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerUIScript : NetworkBehaviour
{

    private Text killedPlayerText;
    private Image crosshair;
    private Image crosshairHitmarker;
    private Text crosshairText;
    private Text healthText;
    private Text armorText;
    private Text ammoText;

    private List<Image> ammoBoxes;
    private List<Text> ammoTexts;
    private List<Image> ammoImages;

    public float crosshairRaycastDetect = 1.0f;
    public Camera m_Camera;
    private PlayerClientController controllerRef;

    private Color boxColor;

    private List<Color> boxHighlightColors;

    private Canvas inGameCanvas;
    private Canvas respawnCanvas;
    private Canvas pauseCanvas;
    private Button pauseButton;

    private Canvas scoreboardCanvas;
    private Transform contentPanel;
    private List<GameObject> scoreboardItemInstances;
    

    public GameObject scoreboardItemPrefab;




        


    public void Init()
    {
        if (!isLocalPlayer) { return; }
        inGameCanvas = GameObject.FindGameObjectWithTag("InGameCanvas").GetComponent<Canvas>();
        respawnCanvas = GameObject.FindGameObjectWithTag("RespawnCanvas").GetComponent<Canvas>();
        pauseCanvas = GameObject.FindGameObjectWithTag("PauseCanvas").GetComponent<Canvas>();
        scoreboardCanvas = GameObject.FindGameObjectWithTag("ScoreboardCanvas").GetComponent<Canvas>();
        contentPanel = GameObject.FindGameObjectWithTag("scoreboardContentPanel").transform;
        scoreboardItemInstances = new List<GameObject>();


        pauseButton = pauseCanvas.GetComponentInChildren<Button>();
        pauseButton.onClick.AddListener(OnDisconnectButtonClick);

        killedPlayerText = GameObject.FindGameObjectsWithTag("KilledPlayerText")[0].GetComponent<Text>();
        crosshair = GameObject.FindGameObjectsWithTag("CrosshairImage")[0].GetComponent<Image>();
        crosshairHitmarker = GameObject.FindGameObjectsWithTag("CrosshairHitmarker")[0].GetComponent<Image>();
        crosshairText = GameObject.FindGameObjectsWithTag("CrosshairText")[0].GetComponent<Text>();
        healthText = GameObject.FindGameObjectsWithTag("PlayerHealthText")[0].GetComponent<Text>();
        armorText = GameObject.FindGameObjectsWithTag("PlayerArmorText")[0].GetComponent<Text>();
        ammoBoxes = new List<Image>();
        ammoTexts = new List<Text>();
        ammoImages = new List<Image>();
        boxHighlightColors = new List<Color>();

        // Since FindGameObjectsWithTag doesnt return in the same order in the hierachy we have to find each one

        ammoBoxes.Add(GameObject.FindGameObjectsWithTag("ammo0")[0].GetComponent<Image>());
        ammoBoxes.Add(GameObject.FindGameObjectsWithTag("ammo1")[0].GetComponent<Image>());
        ammoBoxes.Add(GameObject.FindGameObjectsWithTag("ammo2")[0].GetComponent<Image>());
        ammoBoxes.Add(GameObject.FindGameObjectsWithTag("ammo3")[0].GetComponent<Image>());
        ammoBoxes.Add(GameObject.FindGameObjectsWithTag("ammo4")[0].GetComponent<Image>());


        ammoTexts.Add(GameObject.FindGameObjectsWithTag("ammo0")[0].GetComponentInChildren<Text>());
        ammoTexts.Add(GameObject.FindGameObjectsWithTag("ammo1")[0].GetComponentInChildren<Text>());
        ammoTexts.Add(GameObject.FindGameObjectsWithTag("ammo2")[0].GetComponentInChildren<Text>());
        ammoTexts.Add(GameObject.FindGameObjectsWithTag("ammo3")[0].GetComponentInChildren<Text>());
        ammoTexts.Add(GameObject.FindGameObjectsWithTag("ammo4")[0].GetComponentInChildren<Text>());

        ammoImages.Add(GameObject.FindGameObjectsWithTag("ammoImage0")[0].GetComponent<Image>());
        ammoImages.Add(GameObject.FindGameObjectsWithTag("ammoImage1")[0].GetComponent<Image>());
        ammoImages.Add(GameObject.FindGameObjectsWithTag("ammoImage2")[0].GetComponent<Image>());
        ammoImages.Add(GameObject.FindGameObjectsWithTag("ammoImage3")[0].GetComponent<Image>());
        //ammoImages.Add(GameObject.FindGameObjectsWithTag("ammoImage4")[0].GetComponent<Image>());

        for (int i = 0; i < ammoImages.Count; i++)
        {
            boxHighlightColors.Add(ammoImages[i].color);
        }


        crosshairHitmarker.enabled = false;
        
        // Callback to the player controller, to tell them all the ui elements exist
        controllerRef.CmdUiInitilized();

        boxColor = new Color(0,0,0, 0.5f);

        ShowRespawnUi(false);
        TogglePauseScreen();
        ToggleScoreboard();
        killedPlayerText.enabled = false;




    }

    


    public void SetControllerReference(PlayerClientController controller)
    {
        controllerRef = controller;
    }


    void Awake()
    {

        if (!isLocalPlayer) { return; }



    }

    // Use this for initialization
	void Start () {
        if (!isLocalPlayer) { return; } 
        Init();
    }
	
	// Update is called once per frame
	void Update ()
	{
	    
        if (!isLocalPlayer) { return; }
	    RaycastHit hit;
	    Vector3 fwd = m_Camera.transform.forward;
	    Vector3 rayStartPos = m_Camera.transform.position + (fwd * crosshairRaycastDetect);
        Debug.DrawRay(rayStartPos, fwd);
        if (Physics.Raycast(rayStartPos, fwd, out hit))
	    {
            // TODO(Steven): hardcoding player string like this is bad idea
	        if (hit.transform.tag == "Player" && controllerRef.transform != hit.transform)
	        {
	            SetCrosshairOnEnemny(hit.transform.GetComponent<PlayerClientController>().playerName);
	        }
	        else
	        {
                SetCrosshairNeutral();
            }

	    }
	    else
	    {
            SetCrosshairNeutral();
        }

	}
    

    public void ShowRespawnUi(bool show)
    {
        if (show)
        {
            inGameCanvas.enabled = false;
            respawnCanvas.enabled = true;
        }
        else
        {
            inGameCanvas.enabled = true;
            respawnCanvas.enabled = false;
        }
    }

    public void SetHealth(int health)
    {

        if (!isLocalPlayer) return;
        healthText.text = health.ToString();
        
    }

    public void SetArmor(int armor)
    {
        if (!isLocalPlayer) return;
        armorText.text = armor.ToString();
    }



    public void SetAmmo(SyncList<int> ammos)
    {
        if (!isLocalPlayer) return;
        for (int i = 0; i < ammos.Count; i++)
        {
            if (ammos[i] != null)
            {
                ammoTexts[i].text = ammos[i].ToString();
            }
            
        }
    }



    void SetCrosshairNeutral()
    {
        if (!isLocalPlayer) return;
        crosshair.color = Color.white;
        crosshairText.enabled = false;
        crosshairText.text = "";
    }

    
    void SetCrosshairText(string text)
    {
        if (!isLocalPlayer) return;
        crosshairText.text = text;
    }

    
    void SetCrosshairOnEnemny(string enemenyName)
    {
        if (!isLocalPlayer) return;
        crosshair.color = Color.red;
        crosshairText.enabled = true;
        SetCrosshairText(enemenyName);
    }

    
    void ShowCrosshair()
    {
        if (!isLocalPlayer) return;
        crosshair.enabled = true;
    }

    
    void HideCrosshair()
    {
        if (!isLocalPlayer) return;
        crosshair.enabled = false;
    }


    public void SetHitMarkerEnabled(bool enabled)
    {
        if (!isLocalPlayer) return;
        crosshairHitmarker.enabled = enabled;
    }

    public void SetCurrentGunIndex(int index)
    {
        if (ammoTexts != null)
        {

            for (int i = 0; i < ammoTexts.Count; i++)
            {
                if (ammoBoxes[i] != null)
                {
                    ammoBoxes[i].color = boxColor;
                }

            }
            ammoBoxes[index].color = Color.Lerp(boxHighlightColors[index], Color.black, 0.4f);
        }
    }

    public bool TogglePauseScreen()
    {
        pauseCanvas.enabled = !pauseCanvas.enabled;
        pauseButton.enabled = !pauseButton.enabled;
        return pauseCanvas.enabled;
            
    }

    public bool ToggleScoreboard()
    {

        scoreboardCanvas.enabled = !scoreboardCanvas.enabled;
        if (scoreboardCanvas.enabled)
        {
            List<Scoreboard.Playerdata> data = Scoreboard.instance.GetScoreBoard();
            for (int i = 0; i < data.Count; i++)
            {
                
                GameObject item = Instantiate(scoreboardItemPrefab, scoreboardItemPrefab.transform.position, scoreboardItemPrefab.transform.rotation);
                scoreboardItemInstances.Add(item);
                ScoreboardItem scoreboardItem = item.GetComponent<ScoreboardItem>();
                scoreboardItem.playerName.text = data[i].name;
                scoreboardItem.playerKills.text = data[i].kills.ToString();
                scoreboardItem.playerDeaths.text = data[i].deaths.ToString();
                item.transform.SetParent(contentPanel, false);

            }
        }
        else
        {
            for (int i = 0; i < scoreboardItemInstances.Count; i++)
            {
                // We need to destroy the scoreboard items we instantiated
                Destroy(scoreboardItemInstances[i]);
            }
        }




        return scoreboardCanvas.enabled;
    }

    private void OnDisconnectButtonClick()
    {
        controllerRef.OnPlayerDisconnect();
    }

    public void ShowPlayerKill(string enemnyName)
    {
        StartCoroutine("IShowPlayerKilled", "You killed " + enemnyName);
    }

    public void ShowPlayerSuicide(string text)
    {
        StartCoroutine("IShowPlayerKilled", text);
    }

    private IEnumerator IShowPlayerKilled(string text)
    {
        killedPlayerText.text = text;
        killedPlayerText.enabled = true;
        for (float f = 4f; f >= 0; f -= Time.deltaTime)
        {
            yield return null;
        }


        killedPlayerText.text = "";
        killedPlayerText.enabled = false;
    }


}
