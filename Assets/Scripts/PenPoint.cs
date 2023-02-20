using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenPoint : MonoBehaviour
{
    public Vector3 center;
    SpriteRenderer spriteRenderer;
    bool displaySprite;
    LineRenderer lineRenderer;
    List<Vector3> pathPoints;
    Vector3[] positionArray;

    // Start is called before the first frame update
    void Start()
    {
        pathPoints = new List<Vector3>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        //lineRenderer.material.SetTextureScale("_MainTex", new Vector2(100f, 1f));
    }

    public void SetPositionArray(int size)
    {
        positionArray = new Vector3[size];
    }

    public void StorePoint(int index)
    {
        Vector3 newPosition = transform.position;
        positionArray[index] = newPosition;
        //pathPoints.Add(newPosition);
    }

    public void SetCenter(Vector3 center)
    {
        this.center = center;
        transform.position = center;
    }

    public void DrawCurve(int count)
    {
        //Vector3[] positionArray = pathPoints.ToArray();
        //lineRenderer.positionCount = positionArray.Length;
        lineRenderer.positionCount = count;
        lineRenderer.SetPositions(positionArray);
    }

    public void ResetCurve()
    {
        //pathPoints = new List<Vector3>();
        positionArray = new Vector3[0];
        DrawCurve(positionArray.Length);
    }

    public void SpriteVisible(bool condition)
    {
        if (condition)
        {
            spriteRenderer.enabled = true;
        }
        else
        {
            spriteRenderer.enabled = false;
        }
    }
}
