using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hole : MonoBehaviour
{
    public Rigidbody rb;
    public int counter = 0;
    public bool count = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (count)
        {
            counter += 1;
        }
        if (counter > 50)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            rb.isKinematic = false;
            count = true;
        }
    }

}
