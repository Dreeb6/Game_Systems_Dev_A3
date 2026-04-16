using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Events;
using System.Threading;

public class BossStateMachine : MonoBehaviour
{

    public enum State { Resting, TurretShooting, Melee, MortarMode, MortarFiring}

    [Header("Scene References")]
    public Transform character;
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI fireRateText;
    public TextMeshProUGUI mortarTimerText;

    [Header("Config")]
    public float mortarDelayThreshold = 3.0f;
    public float fireTimeThreshhold = 5.0f;
    public float launchForce = 700f;
    public int ammo = 4;
    private float timer = 1.0f;
    private float mortarTimer = 0.0f;


    [Header("Vision Settings")]
    public float viewRadius = 50f;
    [Range(0, 360)]
    public float meleeRadius = 5f;
    public float viewAngle = 90f;

    [Header("Events")]
    public UnityEvent<bool> fireBullet;

    //public TurretScript turretScript;

    //layermask makes it so that the vison cone isn't blocked by projectiles
    public LayerMask BossLayerMask;

    public GameObject Bullet;
    public Transform target;
    public Transform startingPoint;
    

    

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
        canSeePlayer = IsInViewCone();
        mortarTimer += Time.deltaTime;
        mortarTimerText.text = $"Time until mortar: {mortarTimer}";
        if (canSeePlayer && ammo == 4)
        {
            Debug.Log(state);
            mortarTimer = 0.0f;
            
            state = State.TurretShooting;
            
        }

        if (mortarTimer >= mortarDelayThreshold)
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

    }

    void MortarMode()
    {

    }

    void MortarFiring()
    {

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
    private void OnDrawGizmos()
    {
        Handles.color = new Color(0f, 1f, 1f, 0.25f);
        if (canSeePlayer) Handles.color = new Color(1f, 0f, 0f, 0.25f);

        Vector3 forward = transform.forward;
        Handles.DrawSolidArc(transform.position, Vector3.up, forward, viewAngle / 2f, viewRadius);
        Handles.DrawSolidArc(transform.position, Vector3.up, forward, -viewAngle / 2f, viewRadius);
    }
}
