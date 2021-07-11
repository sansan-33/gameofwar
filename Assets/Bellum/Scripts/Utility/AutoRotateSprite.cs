using System.Collections;
using UnityEngine;

public class AutoRotateSprite : MonoBehaviour
{
    bool rotating = false;
    public GameObject objectToRotate;
    public int speed;

    void Start()
    {
        StartCoroutine(rotateObject(objectToRotate, new Vector3(0, 0, speed), 3600f));
    }
    IEnumerator rotateObject(GameObject gameObjectToMove, Vector3 eulerAngles, float duration)
    {
        if (rotating)
        {
            yield break;
        }
        rotating = true;

        Vector3 newRot = gameObjectToMove.transform.eulerAngles + eulerAngles;

        Vector3 currentRot = gameObjectToMove.transform.eulerAngles;

        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            gameObjectToMove.transform.eulerAngles = Vector3.Lerp(currentRot, newRot, counter / duration);
            yield return null;
        }
        rotating = false;
    }
}
