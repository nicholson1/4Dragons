using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAndSpin : MonoBehaviour
{
    private Vector3 target; // The target object to move towards and spin around
    private float speed = 3f; // Speed of the object
    private float spinSpeed = 250f; // Speed of spinning
    private int spins = 3; // Number of spins around the target
    private float withinDistance = .75f; // Distance to stop from the target
    private float radius = .75f; // Radius of the spinning

    private Vector3 startPosition; // Original start position
    private bool isReturning = false; // Check if the object is returning to start

    public void MoveToCleanse(Transform tar)
    {
        startPosition = transform.position;
        target = tar.position + new Vector3(0,1.5f,0);
        StartCoroutine(MoveToWithinDistance());
    }

    IEnumerator MoveToWithinDistance()
    {
        while (Vector3.Distance(transform.position, target) > withinDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }

        StartCoroutine(SpinAroundTarget());
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
        ReturnToStartPosition();
    }

    void ReturnToStartPosition()
    {
        while (Vector3.Distance(transform.position, startPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);
            return;
        }

        transform.position = startPosition; // Ensure the exact start position is reached
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
}
