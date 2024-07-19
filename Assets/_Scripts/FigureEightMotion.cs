using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureEightMotion : MonoBehaviour
{
    public float speed = 2f; // Adjust speed of motion
    public float width = 2f; // Adjust width of the figure-8
    public float height = 2f; // Adjust height of the figure-8

    private float time = 0f;

    void Update()
    {
        time += speed * Time.deltaTime;
        float x = Mathf.Sin(time) * width;
        float y = Mathf.Sin(time * 2) * height;
        transform.localPosition = new Vector3(x, y, 0);
    }
}
