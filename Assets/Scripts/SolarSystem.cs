using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SolarSystem : MonoBehaviour
{
    [SerializeField]
    GameObject CirclePrefab;
    [SerializeField]
    GameObject PenPointPrefab;

    const int minNumCircles = 2;
    const int maxNumCircles = 4;
    [Range(minNumCircles, maxNumCircles)]
    public int numberOfCircles = 4;

    const float minRatio = 0.1f;
    const float maxRatio = 0.9f;
    [Range(minRatio, maxRatio)]
    public float smallerCircleRadiusRatio;

    float largeCircleRadius;

    GameObject[] Circles;
    int indexOfLastCircle;
    GameObject penPoint;

    [SerializeField]
    Slider ratioSlider;
    [SerializeField]
    Slider PenPointPosSlider;
    [SerializeField]
    Dropdown numCircDropdown;
    [SerializeField]
    Button drawButton;

    [SerializeField]
    Material largeCircleMat;
    [SerializeField]
    Material smallCirclesMat;
    [SerializeField]
    Sprite largeCircleSprite;
    [SerializeField]
    Sprite smallCircleSprite;

    float angleIncr = 1.1f;

    bool drawing;
    int numIterations = 8000;
    int currentIteration;
    int batchSizeMin = 8;
    int batchSizeMax = 30;
    int batchSize;
    
    void Start()
    {
        smallerCircleRadiusRatio = ratioSlider.value;

        // Construct Circle hierarchy
        Circles = new GameObject[numberOfCircles];
        
        Circles[0]= Instantiate(CirclePrefab);
        GameObject first = Circles[0];
        first.GetComponent<Circle>().center = transform.position;
        first.GetComponent<Circle>().angleIncrement = angleIncr;
        first.GetComponent<SpriteRenderer>().material = largeCircleMat;
        first.GetComponent<SpriteRenderer>().sprite = largeCircleSprite;
        first.GetComponent<SpriteRenderer>().sortingLayerName = "Sun";

        indexOfLastCircle = numberOfCircles - 1;

        for (int i = 1; i < numberOfCircles; i++)
        {   
            Circles[i] = Instantiate(CirclePrefab);
            GameObject current = Circles[i];
            current.transform.SetParent(Circles[i - 1].transform, false);
            current.GetComponent<SpriteRenderer>().material = smallCirclesMat;
            current.GetComponent<SpriteRenderer>().sprite = smallCircleSprite;
            current.GetComponent<SpriteRenderer>().sortingOrder = i;
            float sign = 1 - (i % 2) * 2; // Mathf.Pow(-1, i);
            current.GetComponent<Circle>().angleIncrement = sign * (angleIncr / Mathf.Pow(smallerCircleRadiusRatio, i));
        }
        for (int i = 1; i < numberOfCircles; i++)
        {
            Circle circleComp = Circles[i].GetComponent<Circle>();
            circleComp.SetRadius(smallerCircleRadiusRatio);
            float centerY = 4 - 4*Mathf.Pow(smallerCircleRadiusRatio, i);
            circleComp.SetCenter(new Vector3(0, centerY, 0) + transform.position);
        }
        penPoint = Instantiate(PenPointPrefab);
        penPoint.transform.SetParent(Circles[Circles.Length - 1].transform, false);
        SetPenPointPosition();
        numCircDropdown.value = 1;
        OnNumberOfCirclesValueChanged();
    }

    public void RunDrawing()
    {
        SetInteractable(false);
        currentIteration = 0;
        drawing = true;
        penPoint.GetComponent<PenPoint>().SetPositionArray(numIterations);
        StartCoroutine(Draw());
    }

    void SetInteractable(bool condition)
    {
        drawButton.interactable = condition;
        ratioSlider.interactable = condition;
        PenPointPosSlider.interactable = condition;
        numCircDropdown.interactable = condition;
    }

    public void SetPenPointPosition()
    {
        PenPoint penComp = penPoint.GetComponent<PenPoint>();
        Circle lastCircleComp = Circles[indexOfLastCircle].GetComponent<Circle>();
        float penPointPosY = PenPointPosSlider.value * Mathf.Pow(smallerCircleRadiusRatio, indexOfLastCircle) + Circles[indexOfLastCircle].transform.position.y;
        penComp.SetCenter(new Vector3(0, penPointPosY, 0) + transform.position);
    }

    public void Reset()
    {
        drawing = false;
        penPoint.GetComponent<PenPoint>().ResetCurve();
        SetPenPointPosition();
        ToggleSpritesVisible(true);
        OnSmallerCircleRatioChanged();
        SetInteractable(true);

    }

    protected IEnumerator Draw()
    {
        while (currentIteration < numIterations && drawing)
        {
            int max = (currentIteration + batchSize < numIterations ? currentIteration + batchSize : numIterations);
            for (int i = currentIteration; i < max; i++)
            {
                for (int j = Circles.Length-1; j >= 0; j--)
                {
                    Circle circleComp = Circles[j].GetComponent<Circle>();
                    circleComp.Iterate();
                    
                }
                penPoint.GetComponent<PenPoint>().StorePoint(i);
            }
            // Speed up drawing by increasing batchsize
            currentIteration += batchSize;
            currentIteration = (currentIteration < numIterations ? currentIteration : numIterations);
            penPoint.GetComponent<PenPoint>().DrawCurve(currentIteration);
            int batchSizeDelta = Mathf.RoundToInt(Mathf.Sqrt(currentIteration * 0.1f)); //Mathf.RoundToInt(Mathf.Log10(10 + currentIteration));
            batchSize = (batchSizeMin + batchSizeDelta < batchSizeMax ? batchSizeMin + batchSizeDelta : batchSizeMax);
            
            yield return new WaitForSeconds(0.04f);
        }
        
        ToggleSpritesVisible(!drawing);
    }

    void ToggleSpritesVisible(bool condition)
    {
        for (int i = 1; i < Circles.Length; i++)
        {
            Circles[i].GetComponent<SpriteRenderer>().enabled = condition;
        }
        penPoint.GetComponent<PenPoint>().SpriteVisible(condition);
    }

    public void OnNumberOfCirclesValueChanged()
    {
        int index = numCircDropdown.value + 2;
        for (int i = 0; i < Circles.Length; i++)
        {
            Circles[i].SetActive(i < index);
        }
        penPoint.transform.SetParent(Circles[index-1].transform, false);
        indexOfLastCircle = index - 1;
        SetPenPointPosition();
    }

    public void OnSmallerCircleRatioChanged()
    {
        smallerCircleRadiusRatio = ratioSlider.value;

        for (int i = 1; i < numberOfCircles; i++)
        {
            Circle circleComp = Circles[i].GetComponent<Circle>();
            circleComp.SetRadius(smallerCircleRadiusRatio);
            float centerY = 4 - 4 * Mathf.Pow(smallerCircleRadiusRatio, i);
            circleComp.SetCenter(new Vector3(0, centerY, 0) + transform.position);
            float sign = Mathf.Pow(-1, i);
            Circles[i].GetComponent<Circle>().angleIncrement = sign * (angleIncr / Mathf.Pow(smallerCircleRadiusRatio, i));
        }
        SetPenPointPosition();

    }
}
