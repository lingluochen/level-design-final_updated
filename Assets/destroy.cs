using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroy : MonoBehaviour
{
    public GameObject player;
    public throwAway throwStuff;
    // Start is called before the first frame update
    void Start()
    {
        throwStuff = player.GetComponent<throwAway>();
    }

    // Update is called once per frame
    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        throwStuff = player.GetComponent<throwAway>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "pickup")
        {
            GameObject pickup = other.gameObject;
            Destroy(pickup);
            if (throwStuff.picked)
            {
                throwStuff.picked = false;
                throwStuff.thisBall = null;
                throwStuff.handBall = null;
                throwStuff.inHand = false;
            }
        }
    }

}
