using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText 
{
    public bool active;   
    public GameObject go;   
    public Text text;       
    //public Vector3 targetPos;
    public Vector3 motion;  
    public float duration;  
    public float lastshown;

    public void Show()
    {
        active = true;
        lastshown = Time.time;
        go.SetActive(active);
    }

    public void Hide()
    {
        active = false;
        go.SetActive(active);
    }

    public void UpdateFloatingText()
    {
        if (!active)
            return;

        if (Time.time - lastshown > duration)
            Hide();

        //Debug.Log("go.pos= " + go.transform.position);
        //go.transform.position = Camera.main.WorldToScreenPoint(position);

        go.GetComponent<Transform>().position += motion * Time.deltaTime;
        //go.GetComponent<Transform>().position = targetPos;
        //Debug.Log(go.GetComponent<Transform>().position);
        //Debug.Log(targetPos);
    }
}
