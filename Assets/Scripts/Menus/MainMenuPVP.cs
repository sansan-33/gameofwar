using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuPVP : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject landingPagePanel = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        landingPagePanel.SetActive(false);
        Mirror.NetworkManager.singleton.StartHost();
    }

}
