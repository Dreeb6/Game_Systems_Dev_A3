using UnityEngine;

public class BulletScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Dangerous")
        Destroy(gameObject);
    }
}
