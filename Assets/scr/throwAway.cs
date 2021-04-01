using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class throwAway : MonoBehaviour
{
    public bool picked = false;
    public GameObject thisBall;
    public GameObject cam;
    public Camera realCam;
    public bool inHand = false;
    public GameObject handBall;
    public int throwCounter = 0;
    public bool throwing = false;
    public GameObject sight;
    private GameObject dia;
    public Transform checkpoint;
    public bool death;
    public int deathCounter = 0;
    public startTheGame start;
    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        realCam = cam.GetComponent<Camera>();
        GameObject d = Instantiate(sight, new Vector3(0,0,0),Quaternion.Euler(0,0,0));
        d.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height/2, Camera.main.nearClipPlane + 1));
        dia = d;
        dia.transform.parent = cam.transform;
        dia.SetActive(false);
    }

    // Update is called once per frame

    
    void Update()
    {

        global.position = checkpoint.position;

        if (death)
        {
            deathCounter += 1;
            if (deathCounter > 50)
            {
                death = false;
                deathCounter = 0;
                global.firstDie = true;
                start.restart = true;

            }
        }
        dia.transform.forward = cam.transform.forward;

        if (!picked && thisBall != null && !inHand)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                picked = true;
                thisBall.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width*5 / 6, Screen.height / 5, Camera.main.nearClipPlane+1));
                thisBall.transform.parent = cam.transform;
                thisBall.transform.forward = -cam.transform.forward;
                Rigidbody rb = thisBall.GetComponent<Rigidbody>();
                rb.isKinematic = true;
                picked ballP = thisBall.GetComponent<picked>();
                ballP.approached = false;
                inHand = true;
                handBall = thisBall;
                throwing = true;
                dia.SetActive(true);
            }
        }
        if (picked && inHand && handBall != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                picked = false;
                Rigidbody rb = thisBall.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                handBall.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane + 1));
                handBall.transform.parent = null;
                picked ballP = handBall.GetComponent<picked>();
                ballP.thrown = true;
                handBall = null;
                inHand = false;
                throwing = true;
                dia.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.E) && !throwing)
            {
                picked = false;
                Rigidbody rb = thisBall.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                handBall.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z) + transform.forward;
                handBall.transform.parent = null;
                handBall = null;
                inHand = false;
                throwing = true;
                dia.SetActive(false);
            }
        }
    }
    void FixedUpdate()
    {
        if (throwing)
        {
            throwCounter += 1;
        }
        if (throwCounter > 10)
        {
            throwing = false;
            throwCounter = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "pickup" && !picked && !inHand && !throwing)
        {
            picked p = other.gameObject.GetComponent<picked>();
            p.approached = true;
            thisBall = other.gameObject;
        }
        if (other.gameObject.tag == "checkpoint")
        {
            checkpoint = other.gameObject.transform;
            global.position = checkpoint.position;
        }
        if (other.gameObject.tag == "enemy")
        {
            death = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "pickup" && !picked && !inHand && !throwing)
        {
            picked p = other.gameObject.GetComponent<picked>();
            p.approached = true;
            thisBall = other.gameObject;
        }
        if (other.gameObject.tag == "enemy")
        {
            death = true;
        }
        if (other.gameObject.tag == "checkpoint")
        {
            checkpoint = other.gameObject.transform;
            global.position = checkpoint.position;
            global.tran = checkpoint;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "pickup" && !inHand)
        {
            picked p = other.gameObject.GetComponent<picked>();
            p.approached = false;
            thisBall = null;
        }
    }

}
