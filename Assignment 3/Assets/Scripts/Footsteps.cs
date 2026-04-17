using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioSource Footstep;
    public float minPitch = 0.8f;
    public float maxPitch = 1.1f;
    float lastTime = 0;

    public Transform character;

    public Transform lastKnownSpot;
    //public Transform LastKnownSpot;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            Footstep.enabled = true;

        }
        else
        {
            Footstep.enabled = false;

        }
        if (Footstep.time < lastTime)
        {
            Footstep.pitch = Random.Range(minPitch, maxPitch);
            Debug.Log("Pitch change");
            RelocateSpot();
        }
        lastTime = Footstep.time;

    }

    void RelocateSpot()
    {
        //Each footstep sound moves to Spot to the place where the footstep sound was made
        transform.position = character.position;
    }
}