using System.Collections;
using Cinemachine;
using Mirror;
using UnityEngine;

public class FlipCamera : MonoBehaviour
{

    public CinemachineVirtualCamera camPlayer0;
    public CinemachineVirtualCamera camPlayer1;
    public GameObject groundPlayer0;
    public GameObject groundPlayer1;
    public Light lightPlayer0;
    public Light lightPlayer1;
    public Light lightSecondaryPlayer0;
    public Light lightSecondaryPlayer1;
    float lensSize = 30f;
    private CinemachineVirtualCamera camCurrent;
    private bool zooming = false;
    [SerializeField] public GameObject outsideFrame;

    public void Awake()
    {
        //FoW.FogOfWarTeam.GetTeam(0).SetAll();
        if (NetworkClient.connection.identity == null) { return; }
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        //Debug.Log($"Flip Cam Player ID  {player.GetPlayerID()} , Enemy ID {player.GetEnemyID()}");
        if (player.GetPlayerID() == 0)
        {
            camPlayer0.enabled = true;
            camCurrent = camPlayer0;
            //lightPlayer0.enabled = true;
            //lightSecondaryPlayer0.enabled = true;
            groundPlayer0.SetActive(true);
            camPlayer1.enabled = false;
            //lightPlayer1.enabled = false;
            //lightSecondaryPlayer1.enabled = false;
            groundPlayer1.SetActive(false);
            StaticClass.IsFlippedCamera = false;
        }
        else
        {
            camPlayer1.enabled = true;
            camCurrent = camPlayer1;
            //lightPlayer1.enabled = true;
            //lightSecondaryPlayer1.enabled = true;
            groundPlayer1.SetActive(true);
            camPlayer0.enabled = false;
            //lightPlayer0.enabled = false;
            //lightSecondaryPlayer0.enabled = false;
            groundPlayer0.SetActive(false);
            StaticClass.IsFlippedCamera = true;
        }
    }
    public void Update()
    {
        if (zooming) return;
        StartCoroutine(ZoomCamera());
    }
    private IEnumerator ZoomCamera()
    {
        zooming = true;
        //float increment = -0.15f;
        float zoomInMax = 39.9f;
        //float zoomOutMax = 40f;
        float zoomSpeed = 0.1f;
        float blend = 0f;
        float referenceFramerate = 30f;
        while (camCurrent.m_Lens.OrthographicSize > lensSize)
        {
            yield return new WaitForSeconds(0.02f);
            float fov = camCurrent.m_Lens.OrthographicSize;
            //float target = Mathf.Clamp(fov + increment, zoomInMax, zoomOutMax);
            blend = 1f - Mathf.Pow(1f - zoomSpeed, Time.deltaTime * referenceFramerate);
            //Debug.Log($"New lens size : {Mathf.Lerp(fov, zoomInMax, blend)}, blend:{blend} ,  target: {zoomInMax} ");
            camCurrent.m_Lens.OrthographicSize = Mathf.Lerp(fov, zoomInMax, blend);
        }
        
    }

}
