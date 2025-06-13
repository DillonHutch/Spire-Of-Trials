using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPathScript : MonoBehaviour
{
    [SerializeField]
    private Transform[] waypoints;


    private float moveSpeed = 2f;


    private int waypointIndex = 0;



    // Start is called before the first frame update
    void Start()
    {
        transform.position = waypoints[waypointIndex].transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }


    private void Move()
    {
        if(waypointIndex <= waypoints.Length - 1)
        {

            
            transform.position = Vector2.MoveTowards(transform.position, waypoints[waypointIndex].transform.position, moveSpeed * Time.deltaTime); 


            if(transform.position == waypoints[waypointIndex].transform.position )
            {
                waypointIndex++;
                
            }
        }
        else
        {
            waypointIndex =  0;
        }
    }
}
