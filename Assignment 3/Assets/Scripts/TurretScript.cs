using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

public class TurretScript : MonoBehaviour
{

    public GameObject Bullet;
    public Transform target;
    public Transform startingPoint;
    public BossStateMachine bossStateMachine;

    public float launchForce = 700f;
    public float bulletTime = 100;
    //message to fire bullet
    public bool fireOrder;

    private void Awake()
    {
        fireOrder = false;
    }

    void Update()
    {
        // Rotate the camera every frame so it keeps looking at the target
        transform.LookAt(target);

        if (fireOrder == true)
        {
            Shoot();
            fireOrder = false;
        }
    }

    public void Shoot()
    {
        //instantiate the bullet
        GameObject bullet = Instantiate(Bullet, startingPoint.position, startingPoint.rotation);

        Vector3 direction = (target.position - startingPoint.position).normalized;

        Rigidbody bulletRig = bullet.GetComponent<Rigidbody>();
        bulletRig.AddForce(direction * launchForce);

    }
}
