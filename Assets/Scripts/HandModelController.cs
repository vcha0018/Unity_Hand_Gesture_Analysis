using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandModelController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void WithUpdate()
    {
        Debug.Log("WithUpdate");
    }

    public void PointerEnter()
    {
        Debug.Log("Entred!!");
        
    }

    public void PointerExit()
    {
        Debug.Log("Exitted!!");
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    Debug.Log("Entred 22222!!");
    //}
}
