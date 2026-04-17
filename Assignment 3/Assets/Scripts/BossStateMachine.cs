using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using System.Diagnostics.Tracing;

public class BossStateMachine : MonoBehaviour
{

    public enum State { Resting, TurretShooting, Melee, MortarMode, MortarFiring}

    [Header("Scene References")]
    public Transform character;
    public Transform LastKnownSpot;
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI fireRateText;
    public TextMeshProUGUI mortarTimerText;
    public TextMeshProUGUI mortarAmmoText;

    [Header("Config")]
    public float mortarDelayThreshold = 3.0f;
    public float fireTimeThreshhold = 5.0f;
    public float launchForce = 700f;
    public float mortarLaunchForce = 700f;
    public int ammo = 4;
    public int mortarAmmo = 3;
    private float timer = 1.0f;
    private float mortarTimer = 0.0f;
    private float mortarFireRate = 0.5f;


    [Header("Vision Settings")]
    public float viewRadius = 120f;
    [Range(0, 360)]
    public float meleeRadius = 5f;
    public float viewAngle = 90f;

    //I couldn't get a lobbing projectile working for the mortar
    
    

    //layermask makes it so that the vison cone isn't blocked by projectiles
    public LayerMask BossLayerMask;

    public GameObject Bullet;
    public GameObject Mortar;
    public GameObject MortarSpawner;
    public Transform target;
    public Transform startingPoint;
    public Transform mortarStartingPoint;
    public Transform mortarTarget;


    //animator for the melee attack
    [SerializeField] private Animator meleeAnimator;
    

    State state;
    float mortarDelayTime;
    float fireTime;
    float shotRate;
    bool canSeePlayer;
    bool canHearPlayer;

    private float bulletTime;
    

    private void Awake()
    {
        state = State.Resting;
        

    }

    
    void Update()
    {
        switch (state)
        {
            case State.Resting:
                Resting();
                break;
            case State.TurretShooting:
                TurretShooting();
                break;
            case State.Melee:
                Melee();
                break;
            case State.MortarMode:
                MortarMode();
                break;
            case State.MortarFiring:
                MortarFiring();
                break;

        }
        stateText.text = $"State: {state}";
        
    }

    void Resting()
    {
        //bulletTime -= Time.deltaTime;
        
        ammo = 4;
        mortarAmmo = 3;
        //meleeAnimator.Play("MeleeCrush", -1, 0f);
        
        
        canSeePlayer = IsInViewCone();
        mortarTimer += Time.deltaTime;
        mortarTimerText.text = $"Time until mortar: {mortarTimer}";
        if (canSeePlayer && ammo == 4)
        {
            Debug.Log(state);
            mortarTimer = 0.0f;
            state = State.TurretShooting;
            
        }
        //if enough time has passed without seeing the player, enter mortar mode
        if (mortarTimer >= mortarDelayThreshold && mortarAmmo == 3)
        {
            Debug.Log(state);
            state = State.MortarMode;
        }
    }

    void TurretShooting()
    {
        //float fireTimeElapsed = Time.time - fireTime;
        ammoText.text = $"Boss ammo: {ammo}";
        fireRateText.text = $"Fire Rate Timer: {bulletTime}";

        if (ammo > 0)
        {
            Debug.Log("fire");
            Shoot();
            //fireTimeElapsed = 0;
            
        }
        //reload
        else if(ammo <= 0)
        {
            state = State.Resting;
        }
        canSeePlayer = IsInViewCone();
        if (!canSeePlayer)
        {
            Debug.Log(state);
            state = State.Resting;
        }
        
        
    }
    void Melee()
    {
        //meleeAnimator = GetComponent<Animator>();
        
        meleeAnimator.SetBool("CrushOn", true);
        Debug.Log(state);
        state = State.Resting;
    }

    void MortarMode()
    {
        //start displaying ammo
        mortarAmmoText.text = $"MortarAmmo: {mortarAmmo}";

        if (mortarAmmo > 0)
        {
            Debug.Log("fire Mortar");
            state = State.MortarFiring;

        }
        //reload
        else if (mortarAmmo <= 0)
        {
            Debug.Log("Mortar reload");
            state = State.Resting;
        }
        canSeePlayer = IsInViewCone();
        if (canSeePlayer)
        {
            Debug.Log(state);
            state = State.Resting;
        }
    }

    void MortarFiring()
    {
        //regulate fire rate
        mortarFireRate -= Time.deltaTime;
        if (mortarFireRate > 0) return;

        mortarFireRate = mortarTimer;
        mortarAmmo = mortarAmmo - 1;

        //instantiate the mortar
        GameObject mortar = Instantiate(Mortar, mortarStartingPoint.position, mortarStartingPoint.rotation);

        //Mortar starting point is always directly above the target point
        Vector3 direction = (mortarTarget.position - mortarStartingPoint.position).normalized;

        Rigidbody mortarRig = mortar.GetComponent<Rigidbody>();
        mortarRig.AddForce(direction * mortarLaunchForce);

        state = State.MortarMode;

    }

    bool IsInViewCone()
    {
        Vector3 toPlayer = character.position - transform.position;
        float distToPlayer = toPlayer.magnitude;

        // 1. Distance check
        if (distToPlayer > viewRadius) return false;

        // 2. Angle check
        Vector3 dirToPlayer = toPlayer.normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle > viewAngle * 0.5f) return false;

        // 3. Raycast
        if (Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, viewRadius, BossLayerMask))
        {
            return hit.transform == character.transform;
        }
        return false;
    }

    public void Shoot()
    {

        //regulate fire rate
        bulletTime -= Time.deltaTime;
        if (bulletTime > 0) return;

        bulletTime = timer;
        ammo = ammo - 1;

        //instantiate the bullet
        GameObject bullet = Instantiate(Bullet, startingPoint.position, startingPoint.rotation);

        Vector3 direction = (target.position - startingPoint.position).normalized;

        Rigidbody bulletRig = bullet.GetComponent<Rigidbody>();
        bulletRig.AddForce(direction * launchForce);

    }

    private void OnTriggerEnter(Collider other)
    {
        //Check if the player is in the Shooting state 
        if (state == State.TurretShooting)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log(state);
                state = State.Melee;
                Debug.Log("MELEE ATTACK");
            }

        }

    }
    private void OnTriggerExit(Collider other)
    {
        //check if the player is in the melee state
        if (state == State.Melee)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log(state);
                meleeAnimator.SetBool("CrushOn", false);
                state = State.Resting;
                Debug.Log("RESETMELEE");
            }

        }
    }



    private void OnDrawGizmos()
    {
        Handles.color = new Color(0f, 1f, 1f, 0.25f);
        if (canSeePlayer) Handles.color = new Color(1f, 0f, 0f, 0.25f);

        Vector3 forward = transform.forward;
        Handles.DrawSolidArc(transform.position, Vector3.up, forward, viewAngle / 2f, viewRadius);
        Handles.DrawSolidArc(transform.position, Vector3.up, forward, -viewAngle / 2f, viewRadius);
    }
}
