using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TankLoad : NetworkBehaviour
{

    [SyncVar]
    public Color color;
    [SyncVar]
    public string playerName;
    public int rebornTimeInSec = 10;
    public Color disabledColor;
    public AudioSource audioSource;
    
    

    // mesh renders for the tank
    private MeshRenderer[] renderers;
    private Color enableColor;
//    private TankHealth tankHealth;
    private string coloredPlayerNameText;
    private WaitForSeconds waitOneSec;
    private NetworkStartPosition[] playerSpawnPoint;
//    private GameObject worldCam;
    private Text screenText;
    private TankMovement scriptMovement;
    private TankShooting scriptShooting;


    // Use this for initialization
    void Start ()
    {
        if (isLocalPlayer)    //if  client
        {
            Debug.Log("Finding reference...");
            playerSpawnPoint = FindObjectsOfType<NetworkStartPosition>();
         //   worldCam = GameObject.FindGameObjectWithTag("WorldCam");
            screenText = FindObjectOfType<Text>(); 
            
            scriptMovement = GetComponent<TankMovement>();
            scriptShooting = GetComponent<TankShooting>();

            if (screenText != null)
                screenText.text = "";
            else Debug.Log("cannot find the screen text");

            enableColor = color;

        }




        renderers = GetComponentsInChildren<MeshRenderer>();
//        tankHealth = GetComponent<TankHealth>();
        ChangeColor(color);
        //get the player text display with color
        coloredPlayerNameText = "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + playerName + "</color>";
        waitOneSec = new WaitForSeconds(1f);
    }




    // to reset tank and camera when a tank health is 0
    public void AfterDefeat()
    {

        ChangeColor(disabledColor);
        DisableControl();

        StartCoroutine(TimeCountDown(rebornTimeInSec));
        

    }


     IEnumerator TimeCountDown(int Sec)
    {
        if (isLocalPlayer)
        {
            for (int i = Sec; i > 0; i--)
            {

                Debug.Log("time: " + i + " Sencond");
                screenText.text = "You need " + i + " seconds to reborn";
                yield return waitOneSec;
            }
            ResetPlayer();
        }
            
    } 


    
    void RespawnPlayer()
    {
        if (playerSpawnPoint != null)
        {
            transform.position = playerSpawnPoint[Random.Range(0, playerSpawnPoint.Length)].transform.position;

        }
        else Debug.Log("not set the references of reborn points");
    }


    void ResetPlayer()
    {
        
        EnableControl();
        RespawnPlayer();
        screenText.text = "";
        
        gameObject.SetActive(false);
        gameObject.SetActive(true);
        CmdChangeColor(color);
    }
    [Command]
    void CmdChangeColor(Color cc)
    {
        RpcChangeColor(cc);
    }

    [ClientRpc]
    void RpcChangeColor(Color cc)
    {
        ChangeColor(cc);
    }
    void ChangeColor(Color cc)
    {
  //      CmdWhenChangeColor(cc);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = cc;
        }   
    }

  /*  [Command]
    void CmdWhenChangeColor(Color cc)
    {
        color = cc;
    }
    */
    void DisableControl()
    {
        if (!isLocalPlayer)
            return;
            scriptMovement.enabled = false;
            scriptShooting.enabled = false;

            audioSource.enabled = false;
        
    }

    public void EnableControl()
    {
        if (!isLocalPlayer)
            return;
        scriptMovement.enabled = true;
        scriptShooting.enabled = true;

        audioSource.enabled = true;
    }

}
