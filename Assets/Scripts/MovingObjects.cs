using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObjects : MonoBehaviour
{
    public RectTransform[] movingObjects; 
    public float speed = 200f; 
    private Vector2[] directions; 

    private float canvasWidth;

    void Start()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasWidth = canvasRect.rect.width / 2;

        directions = new Vector2[movingObjects.Length];
        for (int i = 0; i < movingObjects.Length; i++)
        {
            directions[i] = Vector2.right; 
        }
    }

    void Update()
    {
        for (int i = 0; i < movingObjects.Length; i++)
        {
            RectTransform obj = movingObjects[i];
            obj.anchoredPosition += directions[i] * speed * Time.deltaTime;

            if (obj.anchoredPosition.x > canvasWidth || obj.anchoredPosition.x < -canvasWidth)
            {
                directions[i] *= -1;
            }
        }
    }
}
