using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class startTheGame : MonoBehaviour
{
    public Transform cp1;
    public Vector3 cp;
    public bool restart = false;
    public GameObject player;
    public bool restarted = false;
    public bool[] triggers;
    // Start is called before the first frame update
    void Start()
    {
        if (!global.firstDie)
        {
            cp = cp1.position;
            global.position = cp;
        }
        else
        {
            cp = global.position;
        }
        restarted = false;
    }

    // Update is called once per frame
    void Update()
    {
        triggers = global.triggers;
        cp1 = global.tran;
        cp = global.position;
        if (cp1 != null)
        {
            global.position = global.tran.position;
        }
        if (restart)
        {
            global.restart = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (restarted == false)
        {
            global.restart = true;
        }

        if (global.restart)
        {

            GameObject thisP = Instantiate(player, cp, Quaternion.Euler(0, 0, 0));
            throwAway thisTA = thisP.GetComponent<throwAway>();
            thisTA.start = this;
            global.restart = false;
            restarted = true;
        }
    }
}
