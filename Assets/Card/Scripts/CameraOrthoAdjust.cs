using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Camera))]
public class CameraOrthoAdjust : MonoBehaviour {
    Camera cam;
    float landscapeSizeTarget = 0.25f;
    float landScapeOrthoScale;
    float portraitOrthoScale;
    float aspectRatio;

    void Awake () {
        cam = GetComponent<Camera> ();
        gameObject.AddComponent<DeviceChange> ();
        aspectRatio = Screen.width / Screen.height;
        landScapeOrthoScale = landscapeSizeTarget / aspectRatio;
        portraitOrthoScale = landScapeOrthoScale * aspectRatio;
    }

    void Start () {
        DeviceChange.OnOrientationChange.AddListener (DeviceOrientationChanged);
    }

    void DeviceOrientationChanged (DeviceOrientation orientation) {
        Debug.Log ("Orientation Changed");
        if (orientation == DeviceOrientation.LandscapeLeft || orientation == DeviceOrientation.LandscapeRight) {
            cam.orthographicSize = landScapeOrthoScale;
        } else {
            cam.orthographicSize = portraitOrthoScale;
        }
    }
}