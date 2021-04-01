using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDoorController : MonoBehaviour
{
    public GameObject myDoor = null;
    public bool open = false;
    //public bool opened = false;
    public float openAngle = 0;
    public Material green;
    private MeshRenderer mr;
    public int idx;
    public GameObject gm;
    public startTheGame gms;
    void Start()
    {
        //anim = myDoor.GetComponent<Animator>();
        mr = GetComponent<MeshRenderer>();
        gm = GameObject.Find("gameManager");
        gms = gm.GetComponent<startTheGame>();
    }
    void FixedUpdate()
    {
        if (open)
        {
            mr.material = green;
            if (openAngle < 90)
            {
                if (myDoor.transform.localScale.x > 0)
                {
                    myDoor.transform.Rotate(new Vector3(0, -2, 0));
                }
                else
                {
                    myDoor.transform.Rotate(new Vector3(0, 2, 0));
                }
                openAngle += 2;
            }
            global.triggers[idx] = true;
        }
        if (gms.triggers[idx]==true)
        {
            open = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("pickup")){
            open = true; 
        }
    }
}