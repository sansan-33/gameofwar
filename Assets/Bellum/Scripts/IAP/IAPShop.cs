using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPShop : MonoBehaviour
{
    private string buy = "com.sansan-33.bellum.buy";
    [SerializeField] public GameObject restorePurchaseBtn;

    private void Awake()
    {
        DisableRestorePurchaseBtn();
    }

    public void OnPurchaseComplete(Product product)
    {
        Debug.Log($"OnPurchaseComplete() product: {product.definition.id}");
        if (product.definition.id == buy)
        {
            // implement buy action
            Debug.Log("Buy Success! Get gem!!!");
        }

    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.Log($"Buy {product.definition.id} failed due to {reason}!");
    }

    private void DisableRestorePurchaseBtn()
    {
        // Restore button only show in iPhone, disable in all other platform
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            restorePurchaseBtn.SetActive(false);
        }
    }
}