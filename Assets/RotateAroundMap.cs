using UnityEngine;
using Random = UnityEngine.Random;

public class RotateAroundMap : MonoBehaviour
{
    public Transform TargetObject;

    public bool SlowRotate;
    public float speed = 10f;

    public static RotateAroundMap _instance;

    private float currentRotation = 0f;

    private bool onStart = true;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        if (onStart)
        {
            SlowRotate = true;
            onStart = false;
        }
    }

    public void ToggleRotate(bool isRotating)
    {
        SlowRotate = isRotating;
    }

    public void StopRandomRotate(bool forcePosition = false)
    {
        SlowRotate = false;

        float targetRotation;

        if (forcePosition)
        {
            targetRotation = 330f;

            Debug.Log("Level 30 detected, setting rotation to 330");
        }
        else
        {
            // Pick a position 20-270 degrees from the current position
            float randomOffset = Random.Range(20f, 270f);

            targetRotation = currentRotation + randomOffset;
        }

        // Calculate how far we actually need to rotate
        float rotationDelta = Mathf.DeltaAngle(
            currentRotation,
            targetRotation
        );

        // Update our stored rotation
        currentRotation = targetRotation;

        // Move the map
        transform.RotateAround(
            TargetObject.position,
            Vector3.up,
            rotationDelta
        );
    }

    private void Update()
    {
        if (SlowRotate && TargetObject != null)
        {
            float rotationDelta = speed * Time.deltaTime;

            transform.RotateAround(
                TargetObject.position,
                Vector3.up,
                rotationDelta
            );

            // Keep track of the rotation!
            currentRotation += rotationDelta;

            // Prevent the value from growing forever
            currentRotation %= 360f;
        }
    }
}
