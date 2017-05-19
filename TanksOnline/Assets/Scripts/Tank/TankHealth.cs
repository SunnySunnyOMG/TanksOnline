using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class TankHealth : NetworkBehaviour
{
    public float m_StartingHealth = 100f;
    public Slider m_Slider;
    public Image m_FillImage;
    public Color m_FullHealthColor = Color.green;
    public Color m_ZeroHealthColor = Color.red;
    public GameObject m_ExplosionPrefab;
    [HideInInspector]
    public bool m_Dead;
    //   public Text scoreText;

    [SyncVar]
    public int deathNum;

    [SerializeField]
    private GameObject infoPanel;
    private AudioSource m_ExplosionAudio;
    private ParticleSystem m_ExplosionParticles;
    private TankLoad tankLoad;


    [SyncVar(hook = "WhenChangeHealth")]
    private float m_CurrentHealth;



    private void Start()
    {
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

        m_ExplosionParticles.gameObject.SetActive(false);
        tankLoad = GetComponent<TankLoad>();
        if (!isLocalPlayer)
            infoPanel.SetActive(false);
    }


    //run when be enabled
    private void OnEnable()
    {
        // local value
        m_CurrentHealth = m_StartingHealth;      
        // sync to server
            CmdSyncHealth(m_CurrentHealth);
        Debug.Log("current health: " + m_CurrentHealth);

        SetHealthUI(m_CurrentHealth);
    }

    [Command]
    void CmdSyncHealth(float hh)
    {
        m_CurrentHealth = hh;
        m_Dead = false;
    }

    public void TakeDamage(float amount)
    {
        if (!isServer)
            return;  
        // Adjust the tank's current health, update the UI based on the new health and check whether or not the tank is dead.
        m_CurrentHealth -= amount;
        Debug.Log("Health after damage: " + m_CurrentHealth);
        if (m_CurrentHealth <= 0f && !m_Dead)
        {
            Debug.Log("Health when die: " + m_CurrentHealth);
            CmdOnDeath();   
        } 
        Debug.Log("Dead? after damage: " + m_Dead);
    }


    private void SetHealthUI(float health)
    {
        // Adjust the value and colour of the slider.
        m_Slider.value = health;
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, health / m_StartingHealth);
    }

    private void WhenChangeHealth(float m_CurrentHealth)
    {
        SetHealthUI(m_CurrentHealth);
    }

    [Command]
    private void CmdOnDeath()
    {
        RpcOnDeath();
        m_Dead = true;
        //deathNum++;
    }

    [ClientRpc]
    private void RpcOnDeath()
    {
        // Play the effects for the death of the tank and deactivate it.
        //       scoreText.text = "Death: " + deathNum;
        
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);
        m_ExplosionParticles.Play();
        m_ExplosionAudio.Play();

        //gameObject.SetActive(false);

        tankLoad.AfterDefeat();

    }
}