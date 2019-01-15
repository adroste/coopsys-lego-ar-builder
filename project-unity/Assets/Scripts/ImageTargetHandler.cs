using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ImageTargetHandler : MonoBehaviour, IVirtualButtonEventHandler, ITrackableEventHandler
{
    private TrackableBehaviour _trackableBehaviour;
    private GameObject _blueSphere, _whiteSphere;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting...");
        this._blueSphere = GameObject.Find("BlueSphere");
        this._whiteSphere = GameObject.Find("WhiteSphere");
        this._trackableBehaviour = GetComponent<TrackableBehaviour>();
        
        this._blueSphere.SetActive(true);
        this._whiteSphere.SetActive(true);

        this._trackableBehaviour.RegisterTrackableEventHandler(this);
        foreach (var virtualButtonBehaviour in GetComponentsInChildren<VirtualButtonBehaviour>(true))
            virtualButtonBehaviour.RegisterEventHandler(this);
        Debug.Log("Finished Start()");
    }

    // Update is called once per frame
    void OnDestroy()
    {
        foreach (var virtualButtonBehaviour in GetComponentsInChildren<VirtualButtonBehaviour>(true))
            virtualButtonBehaviour.UnregisterEventHandler(this);
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            Debug.Log($"Trackable {this._trackableBehaviour.TrackableName} found");
            OnTrackableFound();
        }
        else
        {
            Debug.Log($"Trackable {this._trackableBehaviour.TrackableName} lost");
            OnTrackableLost();
        }
    }

    private void OnTrackableFound()
    {
        foreach (var rendererComponent in GetComponentsInChildren<Renderer>(true))
            rendererComponent.enabled = true;
    }

    private void OnTrackableLost()
    {
        foreach (var rendererComponent in GetComponentsInChildren<Renderer>(true))
            rendererComponent.enabled = false;
    }

    public void OnButtonPressed(VirtualButtonBehaviour vb)
    {
        Debug.Log($"Button {vb.VirtualButtonName} pressed");
        switch (vb.VirtualButtonName)
        {
            case "VirtualButtonBack":
                this._whiteSphere.SetActive(false);
                break;
            case "VirtualButtonNext":
                this._blueSphere.SetActive(false);
                break;  
            default:
                throw new UnityException("Button not supported: " + vb.VirtualButtonName);
                break;
        }
    }
    
    public void OnButtonReleased(VirtualButtonBehaviour vb)
    {
        Debug.Log($"Button {vb.VirtualButtonName} released");
        switch (vb.VirtualButtonName)
        {
            case "VirtualButtonBack":
                this._whiteSphere.SetActive(true);
                break;
            case "VirtualButtonNext":
                this._blueSphere.SetActive(true);
                break;
            default:
                throw new UnityException("Button not supported: " + vb.VirtualButtonName);
                break;
        }
    }

}
