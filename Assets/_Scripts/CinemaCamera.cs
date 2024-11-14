using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemaCamera : MonoBehaviour
{
    public Transform target; // The object to orbit around
    public float distance = 10f; // Initial distance from the target
    public float zoomSpeed = 2f; // Speed of zooming in/out
    public float rotationSpeed = 100f; // Speed of rotation

    private float currentYaw = 0f; // Current horizontal rotation
    private float currentPitch = 0f; // Current vertical rotation

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("No target assigned for CameraOrbit script.");
            return;
        }

        // Initialize pitch, yaw, and distance based on the camera's position relative to the target
        Vector3 offset = transform.position - target.position;
        distance = offset.magnitude;
        Vector3 direction = offset.normalized;

        currentPitch = Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        currentYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }

    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("No target assigned for CameraOrbit script.");
            return;
        }

        // Get input for horizontal and vertical rotation
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Adjust the yaw and pitch based on input
        currentYaw += horizontalInput * rotationSpeed * Time.deltaTime;
        currentPitch -= verticalInput * rotationSpeed * Time.deltaTime;

        // Clamp the pitch to prevent flipping the camera
        currentPitch = Mathf.Clamp(currentPitch, -85f, 85f);

        // Get input for zooming in/out using + and - keys
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Plus))
        {
            distance -= zoomSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.Underscore))
        {
            distance += zoomSpeed * Time.deltaTime;
        }
        
        distance = Mathf.Clamp(distance, 2f, 20f); // Clamp distance to avoid extreme zooms

        // Calculate the new position of the camera
        Vector3 direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        transform.position = target.position + rotation * direction;

        // Always look at the target
        transform.LookAt(target);
    }
}
