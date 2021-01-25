using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    [SerializeField] private float cameraOffset;
    [SerializeField] private float aspectRatio;
    [SerializeField] private float padding;

    public void ScaleCamera(Vector2 center)
    {
        RepositionCamera(center.x - 1, center.y + 1);
        AdjustOrthograpicSize(center.x, center.y);
    }

    void RepositionCamera(float x, float y)
    {
        //calculate our new position & set it
        Vector3 tempPosition = new Vector3(x/2 , y/2, cameraOffset);
        transform.position = tempPosition;
    }

    void AdjustOrthograpicSize(float x, float y)
    {
        //set our orthograpic size based on the height and width of the maze

        if(x >= y)
        {
            Camera.main.orthographicSize = (x / 2 + padding) / aspectRatio;
        }
        else
        {
            Camera.main.orthographicSize = y / 2 + padding;
        }
    }

}
