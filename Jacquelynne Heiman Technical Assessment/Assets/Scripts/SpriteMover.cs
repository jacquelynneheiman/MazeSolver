using System.Collections.Generic;
using UnityEngine;

public class SpriteMover : MonoBehaviour
{
    [HideInInspector] public bool isAtGoal;

    [SerializeField] float moveSpeed;

    int currentWaypoint;
    List<Vector3> waypoints;

    Animator animator;


    private void Awake()
    {
        waypoints = new List<Vector3>();
        animator = GetComponent<Animator>();
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        //set our current waypoint to 0 & clear our waypoints list
        currentWaypoint = 0;
        waypoints.Clear();

        //search the maze for the path and add each position to our waypoints list
        foreach(Vector3 position in GameManager.instance.solver.FindPath(transform.position, targetPosition))
        {
            waypoints.Add(position);
        }

        //then remove our current position from the list
        if(waypoints != null && waypoints.Count > 1)
        {
            waypoints.RemoveAt(0);
        }
    }

    public void HandleMovement()
    {
        //if we have waypoints
        if (waypoints.Count > 0)
        {
            //set our target position to our current waypoint
            Vector3 targetPosition = waypoints[currentWaypoint];

            //if we are not close enough to our waypoint
            if(Vector3.Distance(transform.position, targetPosition) > .1f)
            {
                //keep moving
                Vector3 moveDirection = (targetPosition - transform.position).normalized;

                //animate our character for the direction we are facing
                animator.SetFloat("Horizontal", moveDirection.x);
                animator.SetFloat("Vertical", moveDirection.y);

                //update our position
                transform.position = transform.position + moveDirection * moveSpeed * Time.deltaTime;
            }
            else
            {
                //if we are close enough, set the next waypoint to our current
                currentWaypoint++;

                //if we are at the end of our list or past it
                if(currentWaypoint >= waypoints.Count)
                {
                    //stop moving! We are at our goal!
                    StopMoving();
                    isAtGoal = true;

                    //play the end stage music!
                    GameManager.instance.audioSource.clip = GameManager.instance.endStage;
                    GameManager.instance.audioSource.Play();
                }
            }

        }
    }

    void StopMoving()
    {
        //empty our waypoint list
        waypoints.Clear();
    }

}
