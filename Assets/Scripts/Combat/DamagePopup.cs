using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(TMP_Text))]
public class DamagePopup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
        TMP_Text tmp_text = GetComponent<TMP_Text>();

        tmp_text.text = transform.root.gameObject.GetComponent<DamageTextHolder>().displayText;
        tmp_text.color = transform.root.gameObject.GetComponent<DamageTextHolder>().displayColor;
        tmp_text.DOFade( 0f, 5.7f );
        transform.DOMove( transform.position + 2 *(Vector3.up) , 1.75f ).OnComplete( () => {
            Destroy(transform.root.gameObject);
        } );
        
    }
}