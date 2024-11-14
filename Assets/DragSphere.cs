using UnityEngine;
using UnityEngine.UI;

public class DragSphere : MonoBehaviour
{
    public Slider xSlider; // Slider for X position
    public Slider ySlider; // Slider for Y position
    public Slider zSlider; // Slider for Z position

    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Please assign a Main Camera in the scene.");
        }

        if (xSlider == null || ySlider == null || zSlider == null)
        {
            Debug.LogError("Please assign all sliders in the Inspector.");
        }

        // Initialize slider values to match the object's initial position
        xSlider.value = transform.localPosition.x;
        ySlider.value = transform.localPosition.y;
        zSlider.value = transform.localPosition.z;

        // Add listeners to sliders
        xSlider.onValueChanged.AddListener(OnXSliderChanged);
        ySlider.onValueChanged.AddListener(OnYSliderChanged);
        zSlider.onValueChanged.AddListener(OnZSliderChanged);
        UpdateSliders();
    }
    

    private void OnXSliderChanged(float value)
    {
        Vector3 newPosition = transform.localPosition;
        newPosition.x = value;
        transform.localPosition = newPosition;
    }

    private void OnYSliderChanged(float value)
    {
        Vector3 newPosition = transform.localPosition;
        newPosition.y = value;
        transform.localPosition = newPosition;
    }

    private void OnZSliderChanged(float value)
    {
        Vector3 newPosition = transform.localPosition;
        newPosition.z = value;
        transform.localPosition = newPosition;
    }

    private void UpdateSliders()
    {
        xSlider.value = transform.localPosition.x;
        ySlider.value = transform.localPosition.y;
        zSlider.value = transform.localPosition.z;
    }
}
