using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class SlidingNumber : MonoBehaviour
{
    public TMP_Text numberText;
    private float animationTime = 0.5f;
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
        float timeElapsed = Time.deltaTime;
        while (currentNumber != desiredNumber) {
            yield return new WaitForSeconds(0.05f);

            if (initialNumber < desiredNumber) {
                //currentNumber += (animationTime * Time.deltaTime * 0.1f) * (desiredNumber - initialNumber);
                currentNumber = Mathf.Lerp(initialNumber, desiredNumber, timeElapsed / animationTime);
                timeElapsed += Time.deltaTime;
                if (currentNumber >= desiredNumber)
                    currentNumber = desiredNumber;
            }
            else
            {
                currentNumber -= (animationTime * Time.deltaTime) * (initialNumber - desiredNumber);
                if (currentNumber <= desiredNumber)
                    currentNumber = desiredNumber;
            }
            //Debug.Log($"animationTime {animationTime} Sliding number {currentNumber}");
            numberText.text = currentNumber.ToString("0");
            
        }
    }
   
}
