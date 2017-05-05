using UnityEngine;
using System.Collections;

public class HidingPlace : MonoBehaviour {

    private bool isUsed = false;
    private MeshRenderer renderer;

    private void Awake()
    {
        renderer = GetComponent<MeshRenderer>();   
    }

    public bool hideIn()
    {
        if (!isUsed)
        {
            renderer.enabled = false;
            isUsed = true;
            return true;
        }

        return false;
    }

    public void leave()
    {
        isUsed = false;
        renderer.enabled = true;
        Debug.Log("left");
    }
}
