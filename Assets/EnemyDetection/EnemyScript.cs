using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float speed = 5;
    public float waitTime = 0.5f;
    public float turnSpeed = 80;
    public Transform pathHolder;
    public Detection detc;
    public Vector3[] waypoints;
    public bool beingHit = false;
    public Material white;
    public MeshRenderer mr;
    public GameObject circle;
    public Transform player;
    public int counter = 0;
    public GameObject playerGO;
    void Start()
    {
        player = playerGO.transform;
        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }
        StartCoroutine(FollowPath(waypoints));
        
    }

    private void Update()
    {
        playerGO = GameObject.FindGameObjectWithTag("Player");
        player = playerGO.transform;
        if (detc.detected && !beingHit)
        {
            StopCoroutine(FollowPath(waypoints));
            transform.position = Vector3.Lerp(transform.position, player.position, 10*Time.deltaTime);
        }

        else if (beingHit)
        {
            if (!detc.detected)
            {
                Destroy(circle);
                mr.material = white;
                counter += 1;
                if (counter > 100)
                {
                    Destroy(gameObject);
                }
            }
        }


    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "pickup")
        {
            beingHit = true;
        }
    }


    IEnumerator FollowPath(Vector3[] waypoints)
    {

            transform.position = waypoints[0];

            int targetWaypointIndex = 1;
            Vector3 targetWaypoint = waypoints[targetWaypointIndex];


            while (!beingHit && !detc.detected)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
                if (transform.position == targetWaypoint)
                {
                    targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                    targetWaypoint = waypoints[targetWaypointIndex];
                    yield return new WaitForSeconds(waitTime);
                }
                yield return null;

            }
        }

    


    void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform wp in pathHolder)
        {
            Gizmos.DrawSphere(wp.position, .3f);
            Gizmos.DrawLine(previousPosition, wp.position);
            previousPosition = wp.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);
    }

}
