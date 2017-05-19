using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class RainFall : NetworkBehaviour {

	// Use this for initialization
	void Start ()
    {
        Destroy(gameObject,3f);
	}
	

}
