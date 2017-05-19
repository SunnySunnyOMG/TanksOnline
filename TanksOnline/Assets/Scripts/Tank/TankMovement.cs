using UnityEngine;
using UnityEngine.Networking;

public class TankMovement : NetworkBehaviour
{
    public int m_PlayerNumber = 1;
    public float m_Speed = 12f;            
    public float m_TurnSpeed = 180f;       
    public AudioSource m_MovementAudio;    
    public AudioClip m_EngineIdling;       
    public AudioClip m_EngineDriving;      
    public float m_PitchRange = 0.2f;
    public float moveSpd = 1f;
    public float rotationSpd = 10f;
    public Transform playerCamera;
    public Transform tankNeck;


    private float lastNeckRot;
//    private string m_MovementAxisName;     
//    private string m_TurnAxisName;         
    private Rigidbody m_Rigidbody;         
    private float m_MovementInputValue;    
//    private float m_TurnInputValue;        
    private float m_OriginalPitch;
    private Vector3 moveFwd;
    private Quaternion lookRot;
   

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    private void OnEnable ()
    {
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
//        m_TurnInputValue = 0f;
    }


    private void OnDisable ()
    {
        m_Rigidbody.isKinematic = true;
    }


    private void Start()
    {
//        m_MovementAxisName = "Vertical" ;
//        m_TurnAxisName = "Horizontal";

        m_OriginalPitch = m_MovementAudio.pitch;
    }


    private void Update()
    {
        if (!isLocalPlayer)
            return;
        // Store the player's input and make sure the audio for the engine is playing.
        moveFwd = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        moveFwd = playerCamera.rotation * moveFwd;
        moveFwd.y = 0f;
        moveFwd.Normalize();
        m_MovementInputValue = Mathf.Max(Mathf.Abs(moveFwd.x), Mathf.Abs(moveFwd.z));

       
        EngineAudio();
        
    }


    private void EngineAudio()
    {
        // Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
        if (Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_MovementInputValue) < 0.1f)
        {
            if(m_MovementAudio.clip == m_EngineDriving)
                {
                     m_MovementAudio.clip = m_EngineIdling;
                     m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                     m_MovementAudio.Play();
                }
        }
        else
        {
            if(m_MovementAudio.clip == m_EngineIdling)
                {
                    m_MovementAudio.clip = m_EngineDriving;
                    m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play();
                }
        }
    }


    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;
        // Move and turn the tank.
        Move();
        Turn();

    }


    private void Move()
    {
        // Adjust the position of the tank based on the player's input.
        m_Rigidbody.MovePosition(m_Rigidbody.position + moveFwd * moveSpd * Time.fixedDeltaTime);
    }


    private void Turn()
    {
        // Adjust the rotation of the tank based on the player's input.
        if (m_MovementInputValue > 0.1)
        {
            lookRot = Quaternion.LookRotation(moveFwd);
            lookRot = Quaternion.Slerp(transform.rotation, lookRot, rotationSpd * Time.fixedDeltaTime);
            //   Debug.Log(lookRot.eulerAngles);
            m_Rigidbody.MoveRotation(lookRot);
        }
        // nect and gun always look in the same direction of camera
        var neckRot = playerCamera.rotation.eulerAngles;

        //******could give limitation to achieve shooting adjustment in Y axis of screen
        neckRot.x = 0f;
        neckRot.z = 0f;
        //*******could give a damp to slow down the rotation speed
        //  (do something)

            tankNeck.rotation = Quaternion.Euler(neckRot);

        // to save network resource, only update in sever and client when neck rotation > 1 degree => have BUG!!!!

        //if (Mathf.Abs(lastNeckRot-neckRot.y)>1f)
//       {
            //Debug.Log("nect rotation difference: " + (lastNeckRot - neckRot.y));
            //tell sever i need turn neck
            CmdNectRot(neckRot.y);//this rotation is automaically attached with a network identity
 //       }

 //       lastNeckRot = neckRot.y;
        
    }
    //tell sever to broadcast the rotation of the tank
    [Command]
    void CmdNectRot(float neckRotY)
    {
        RpcNeckRot(neckRotY);
    }
    
    //all clients (exept the local player) synchronize their neck rotation
    [ClientRpc]
    void RpcNeckRot(float neckRotY)
    {
        if (!isLocalPlayer)
        {
            tankNeck.rotation = Quaternion.Euler(0f, neckRotY, 0f);
        }
    }
}