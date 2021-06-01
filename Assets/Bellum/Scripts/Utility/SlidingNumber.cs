using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class SlidingNumber : MonoBehaviour
{
    public TMP_Text numberText;
    public float animationTime = 0.0005f;
    private float desiredNumber;
    private float initialNumber;
    private float currentNumber=0;

    public void SetNumber(float value)
    {
        initialNumber = currentNumber;
        desiredNumber = value;
        StartCoroutine(slideNumber());
    }
    public void AddToNumber(float value)
    {
        initialNumber = currentNumber;
        desiredNumber += value;
    }
    IEnumerator slideNumber()
    {
        while (currentNumber != desiredNumber) {
            yield return new WaitForSeconds(0.05f);
            var speedFactor = Mathf.Sqrt(desiredNumber / initialNumber);

            if (initialNumber < desiredNumber) {
                currentNumber += (animationTime * Time.deltaTime) * (desiredNumber - initialNumber);
                //currentNumber += speedFactor * (desiredNumber - initialNumber);
                if (currentNumber >= desiredNumber)
                    currentNumber = desiredNumber;
            }
            else
            {
                currentNumber -= (animationTime * Time.deltaTime) * (initialNumber - desiredNumber);
                if (currentNumber <= desiredNumber)
                    currentNumber = desiredNumber;
            }
            Debug.Log($"Sliding number {currentNumber}");
            numberText.text = currentNumber.ToString("0");
            
        }
    }

}
