using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class picked : MonoBehaviour
{
    public bool approached = false;
    public GameObject halo;
    public bool thrown = false;
    public Rigidbody rb;
    public int throwCounter = 0;
    public int throwCounterMax = 5000;
    public float throwForce = 0;
    public float throwForceMax = 2000;
    public GameObject player;
    public throwAway ta;
    // Start is called before the first frame update
    void Start()
    {
        throwForce = throwForceMax;
        //player = GameObject.Find("Player(Clone)");
        ta = player.GetComponent<throwAway>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (thrown)
        {
            rb.AddRelativeForce(-Vector3.forward * throwForce);
            if (throwForce > 0)
            {
                throwForce -= throwForceMax*2 / throwCounterMax;
            }
            throwCounter += 1;
        }

        if (throwCounter > throwCounterMax)
        {
            thrown = false;
            throwCounter = 0;
            throwForce = throwForceMax;
        }

        if (ta.picked == false)
        {
            if (approached)
            {
                halo.SetActive(true);
            }
            else
            {
                halo.SetActive(false);
            }
        }
        else
        {
            halo.SetActive(false);
            approached = false;
        }



    }
}
