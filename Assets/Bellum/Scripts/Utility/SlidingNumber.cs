using System;
using TMPro;
using UnityEngine;

public class SlidingNUmber : MonoBehaviour
{
    public TMP_Text numberText;
    public float animationTime = 1.5f;
    private float desiredNumber;
    private float initialNumber;
    private float currentNumber;

    public void SetNumber(float value)
    {
        initialNumber = currentNumber;
        desiredNumber = value;
    }
    public void AddToNumber(float value)
    {
        initialNumber = currentNumber;
        desiredNumber += value;
    }
    public void Update()
    {
        if (initialNumber < desiredNumber) {
            currentNumber += (animationTime * Time.deltaTime) * (desiredNumber - initialNumber);
            if (currentNumber >= desiredNumber)
                currentNumber = desiredNumber;
        }
        else
        {
            currentNumber -= (animationTime * Time.deltaTime) * (initialNumber - desiredNumber);
            if (currentNumber <= desiredNumber)
                currentNumber = desiredNumber;
        }
        numberText.text = currentNumber.ToString("0");
    }

}
