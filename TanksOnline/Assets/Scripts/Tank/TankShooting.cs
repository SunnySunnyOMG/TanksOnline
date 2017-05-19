using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class TankShooting : NetworkBehaviour
{
    public int m_PlayerNumber = 1;       
    public GameObject m_Shell;            
    public Transform m_FireTransform;    
    public Slider m_AimSlider;           
    public AudioSource m_ShootingAudio;  
    public AudioClip m_ChargingClip;     
    public AudioClip m_FireClip;         
    public float m_MinLaunchForce = 15f; 
    public float m_MaxLaunchForce = 30f; 
    public float m_MaxChargeTime = 0.75f;

    public GameObject rainFall;
    public bool useRainFall = true;
    public float rainFallDis = 15f;


    private string m_FireButton;         
    private float m_CurrentLaunchForce;  
    private float m_ChargeSpeed;         
    private bool m_Fired;                


    private void OnEnable()
    {
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }


    private void Start()
    {
        m_FireButton = "Fire1" ;

        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
    }
    

    private void Update()
    {
        if (!isLocalPlayer)
            return;
        // Track the current state of the fire button and make decisions based on the current launch force.
        if (m_CurrentLaunchForce>=m_MaxLaunchForce&&!m_Fired)
        {
            //at max charge, not fired
            m_CurrentLaunchForce = m_MaxLaunchForce;
            m_ShootingAudio.clip = m_FireClip;
            Fire();
            useRainFall = true;
        }
        else if (Input.GetButtonDown(m_FireButton))
        {
            //just press down firebutton
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play();
        }
        else if (Input.GetButton(m_FireButton) && !m_Fired)
        {
            //holding the button
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
            m_AimSlider.value = m_CurrentLaunchForce;
        }
        else if (Input.GetButtonUp(m_FireButton)&&!m_Fired)
        {
            //release the button, not fired
            m_ShootingAudio.clip = m_FireClip;
            Fire();
            
        }
        if (useRainFall)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                CmdSpawnRainFall();
                useRainFall = false;
            }
        }
    }


    private void Fire()
    {
       m_Fired = true ;
        // Instantiate and launch the shell in local client; this is to ruduce the visual latterncy 
        SpawnBullet(m_CurrentLaunchForce);
        // Instantiate and launch the shell in sever and then to client
        CmdSpawnBullet(m_CurrentLaunchForce);

        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_CurrentLaunchForce;
    }


    [Command]
    void CmdSpawnBullet(float Force)
    {
        RpcSpawnBullet(Force);
    }

    [ClientRpc]
    void RpcSpawnBullet(float Force)
    {
        // local player has already generate a shell, so here we dont need to generate it again
        if (isLocalPlayer)
            return;
        SpawnBullet(Force);
    }

    private void SpawnBullet(float Force)
    {

        GameObject shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as GameObject;
        shellInstance.GetComponent<Rigidbody>().velocity = Force * m_FireTransform.forward;

        m_ShootingAudio.Play();
    }

    [Command]
    void CmdSpawnRainFall()
    {
        GameObject rainInstance = Instantiate(rainFall, m_FireTransform.position+m_FireTransform.forward*rainFallDis, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(rainInstance);
    }

}