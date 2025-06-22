using UnityEngine;

public class Inspectable : MonoBehaviour
{
    bool currentlyBeingInspected;

    void Start()
    {
        currentlyBeingInspected = false;
    }

    public void StartInspecting()
    {
        currentlyBeingInspected = true;
        Debug.Log("Started inspecting...");
    }

    public void StopInspecting()
    {
        currentlyBeingInspected = false;
        Debug.Log("Stopped inspecting.");
    }
}
