using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuPvpButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private GameObject multiplayerPanel = null;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        multiplayerPanel.SetActive(true);
        landingPagePanel.SetActive(false);
        Mirror.NetworkManager.singleton.StartHost();
    }
}

