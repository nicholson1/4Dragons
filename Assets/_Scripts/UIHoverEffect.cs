using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.1f; // Scale factor when hovered
    public float shakeAmount = 10f; // Amount to shake when hovered (in degrees)
    public float shakeSpeed = 10f; // Speed of the shake effect

    private Vector3 initialScale; // Initial scale of the UI element

    void Start()
    {
        initialScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Scale up and start shaking when mouse enters the UI element
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, initialScale * hoverScale, 0.2f).setEaseInOutQuad();
        //LeanTween.rotateZ(gameObject, shakeAmount, shakeSpeed).setLoopPingPong().setEaseInOutQuad();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Scale down and stop shaking when mouse leaves the UI element
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, initialScale, 0.2f).setEaseInOutQuad();
        //LeanTween.cancel(gameObject, "rotateZ");
        //transform.localRotation = Quaternion.identity;
    }
}