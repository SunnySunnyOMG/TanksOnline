using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CharacrerCamControl : NetworkBehaviour {

    public Camera mainCam;

    public AudioListener audioLisener;
    // Use this for initialization
	void Start()
    {

        if (isLocalPlayer)
        {
            mainCam.enabled = true;
            audioLisener.enabled = true;
        }
        else
        {
            mainCam.enabled = false;
            audioLisener.enabled = false;
        }
	}
	

}
