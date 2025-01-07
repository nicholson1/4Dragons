using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoveAndSpin : MonoBehaviour
{
    private Vector3 target; // The target object to move towards and spin around
    private float speed = 3f; // Speed of the object
    private float spinSpeed = 350f; // Speed of spinning
    private int spins = 2; // Number of spins around the target
    private float withinDistance = .75f; // Distance to stop from the target
    private float radius = .75f; // Radius of the spinning

    [SerializeField] private Transform startPosition; // Original start position
    private bool isReturning = false; // Check if the object is returning to start
    
    
    private bool isMoving = false;
    public float initialAcceleration = -0.1f; // Starting acceleration in the -x direction
    public float accelerationIncreaseRate = 0.05f; // Rate at which acceleration increases over time
    public float maxSpeed = 10f; // Maximum speed
    public float arrivalThreshold = 0.1f;
    private Vector3 velocity = Vector3.zero; // Current velocity
    private float currentAcceleration;
    public float overshootDistance = 1f;


    [SerializeField] private Transform[] tutorial1Location;

    private Coroutine c = null;

    public void MoveToRandom()
    {

        if (c != null)
        {
            StopCoroutine(c);
        }
        c = StartCoroutine(MoveToPos1());
        
    }

    public IEnumerator MoveToPos1()
    {
        
        c = StartCoroutine(MoveToTargetWithOvershoot(tutorial1Location[Random.Range(0,tutorial1Location.Length )].position));
        while (isMoving)
        {
            yield return null;
        }
        
        if (c != null)
        {
            StopCoroutine(c);
        }

        yield return new WaitForSeconds(3);
        c =StartCoroutine(MoveToTargetWithOvershoot(startPosition.position));

    }
    public void MoveToCleanse(Transform tar)
    {
        if (c != null)
        {
            StopCoroutine(c);
        }
        transform.localPosition = startPosition.localPosition;
        target = tar.position + new Vector3(0,1.5f,0);
        
        c =StartCoroutine(MoveToWithinDistance());
    }

    IEnumerator MoveToWithinDistance()
    {
        c = StartCoroutine(MoveToTarget(target));
        
        while (isMoving)
        {
            yield return null;
        }
        

        c = StartCoroutine(SpinAroundTarget());
    }

    private bool have_Bounced;
    private float bounceTimer = .15f;
    
    
    private IEnumerator MoveToTargetWithOvershoot(Vector3 target)
    {
        have_Bounced = false;
        isMoving = true;
        currentAcceleration = initialAcceleration;

        while (true)
        {
            Vector3 directionToTarget = (target - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target);

            if (distanceToTarget > arrivalThreshold)
            {
                if (distanceToTarget <= overshootDistance) // Decelerate near the target
                {
                    currentAcceleration -= accelerationIncreaseRate * Time.deltaTime;
                    currentAcceleration = Mathf.Max(0, currentAcceleration); // Prevent negative acceleration
                }
                else // Accelerate when far from the target
                {
                    currentAcceleration += accelerationIncreaseRate * Time.deltaTime;
                }

                // Apply acceleration to velocity
                velocity += directionToTarget * currentAcceleration * Time.deltaTime;

                // Clamp the velocity to the maximum speed
                velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            }
            else // Overshooting behavior
            {
                if (!have_Bounced) // If velocity points away from the target
                {
                    currentAcceleration += accelerationIncreaseRate * Time.deltaTime;
                    velocity += directionToTarget * currentAcceleration * Time.deltaTime;

                    bounceTimer -= Time.deltaTime;
                    if (bounceTimer < 0)
                    {
                        bounceTimer = .15f;
                        have_Bounced = true;
                    }

                }
                else // Snap to the target if it's within the threshold
                {
                    transform.position = target;
                    velocity = Vector3.zero;
                    break; // End the coroutine
                }
            }

            // Move the object
            transform.position += velocity * Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        isMoving = false;
        transform.position = target;
    }
    private IEnumerator MoveToTarget(Vector3 target)
    {
        isMoving = true;
        currentAcceleration = initialAcceleration;

        while (Vector3.Distance(transform.position, target) > arrivalThreshold)
        {
            // Calculate the direction to the target
            Vector3 direction = (target - transform.position).normalized;

            // Update acceleration to increase over time
            currentAcceleration += accelerationIncreaseRate * Time.deltaTime;

            // Apply acceleration to velocity
            velocity += direction * currentAcceleration * Time.deltaTime;

            // Clamp the velocity to the maximum speed
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

            // Move the object
            transform.position += velocity * Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Snap to the target position and stop the movement
        transform.position = target;
        velocity = Vector3.zero;
        isMoving = false;
    }
    

    IEnumerator SpinAroundTarget()
    {
        for (int i = 0; i < spins; i++)
        {
            float angle = 0f;
            while (angle < 360f)
            {
                angle += spinSpeed * Time.deltaTime;
                float radian = angle * Mathf.Deg2Rad;
                float x = target.x + radius * Mathf.Cos(radian);
                float y = target.y;
                float z = target.z + radius * Mathf.Sin(radian);
                transform.position = new Vector3(x, y, z);
                yield return null;
            }
        }

        isReturning = true;
        //c =StartCoroutine(MoveToTargetWithOvershoot(startPosition.position));
        ReturnToStartPosition();
    }

    void ReturnToStartPosition()
    {
        while (Vector3.Distance(transform.position, startPosition.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition.position, speed * Time.deltaTime);
            return;
        }

        transform.position = startPosition.position; // Ensure the exact start position is reached
        isReturning = false;
        //Debug.Log("Returned to start position.");
    }

    void Update()
    {
        if (isReturning)
        {
            ReturnToStartPosition();
        }
    }
    
    private float timer = 5f;
    private void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;

        if (timer < 0)
        {
            timer = 5;
            if (Vector3.Distance(transform.localPosition, startPosition.localPosition) > 10)
            {
                StopCoroutine(c);
                velocity = Vector3.zero;
                transform.localPosition = startPosition.localPosition;
                isReturning = false;
            }
        }
    }
}
