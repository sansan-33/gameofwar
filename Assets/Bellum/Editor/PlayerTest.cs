using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Test]
    public void DragCardTestSuiteSimplePasses()
    {
        // Use the Assert class to test conditions
        Debug.Log($"Test Now 123");
        GameObject obj = new GameObject();
        obj.AddComponent<Player>();
        obj.GetComponent<Player>().dragCardMerge();
    }
}
