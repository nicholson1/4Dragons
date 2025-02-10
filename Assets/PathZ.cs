using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathZ : MonoBehaviour
{
    [SerializeField] bool showRays;
    [SerializeField] Transform pointParent;
    Vector3[] _pathPoints;
    
    void Start()
    {
        _pathPoints = new Vector3[pointParent.childCount];

        for (int i = 0; i < pointParent.childCount; i++)
            _pathPoints[i] = pointParent.GetChild(i).position;
    }

    void Update()
    {
        if (_pathPoints.Length < 2) return;
        Vector3 originalLocal = transform.localPosition;
        FindClosestSegment(transform.position, out Vector3 closestA, out Vector3 closestB);
        Vector3 projectedPoint = ProjectPointOnLineSegment(closestA, closestB, transform);
        transform.position = projectedPoint;
        transform.localPosition = new Vector3( originalLocal.x, originalLocal.y, transform.localPosition.z);
    }

    void FindClosestSegment(Vector3 pos, out Vector3 closestA, out Vector3 closestB)
    {
        closestA = _pathPoints[0];
        closestB = _pathPoints[1];
        float minDist = float.MaxValue;

        for (int i = 0; i < _pathPoints.Length; i++)
        {
            Vector3 a = _pathPoints[i];
            Vector3 b = _pathPoints[(i + 1) % _pathPoints.Length];

            float dist = DistanceToSegment(pos, a, b);
            if (dist < minDist)
            {
                minDist = dist;
                closestA = a;
                closestB = b;
            }
        }
    }

    float DistanceToSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        Vector3 ap = p - a;
        float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / ab.sqrMagnitude);
        Vector3 closestPoint = a + t * ab;
        return Vector3.Distance(p, closestPoint);
    }

    Vector3 ProjectPointOnLineSegment(Vector3 a, Vector3 b, Transform p)
    {
        Vector3 ab = b - a;      
        Vector3 rayOrigin = p.position;
        Vector3 rayDirection = p.right;
        Vector3 ao = rayOrigin - a;
        float denominator = rayDirection.x * -ab.z + rayDirection.z * ab.x;

        // Check if lines are nearly parallel
        if (Mathf.Abs(denominator) < 0.0001f)
            return rayOrigin; // No valid intersection, return original position

        float t = (ao.x * -ab.z + ao.z * ab.x) / denominator;
        float u = (ao.x * rayDirection.z - ao.z * rayDirection.x) / denominator;
        u = Mathf.Clamp01(u);
        Vector3 intersection = a + u * ab;

        if (showRays)
        {
            Debug.DrawLine(transform.position, a, Color.yellow);
            Debug.DrawLine(transform.position, b, Color.yellow);
            Debug.DrawRay(a, ab, Color.cyan);
            Debug.DrawRay(rayOrigin + Vector3.up, rayDirection * 5f, Color.green);
            Debug.DrawLine(rayOrigin, intersection, Color.magenta);
        }

        return intersection;
    }
}
