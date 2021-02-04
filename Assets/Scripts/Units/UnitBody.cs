using System.Collections.Generic;
using UnityEngine;


public class UnitBody : MonoBehaviour, IBody
{

    [SerializeField] private List< Material> material;
    [SerializeField] private  Renderer unitRenderer;
    [SerializeField] private Transform unitTransform;

    public void SetRenderMaterial(int star)
    {
        unitRenderer.sharedMaterial = material[star-1];
    }

    public void SetUnitSize(int star)
    {
        unitTransform.localScale = new Vector3(star, star, star);
    }
}
