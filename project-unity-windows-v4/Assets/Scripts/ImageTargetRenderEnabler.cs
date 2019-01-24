using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ImageTargetRenderEnabler : MonoBehaviour, ITrackableEventHandler
{
    private TrackableBehaviour _trackableBehaviour;
    
    // Start is called before the first frame update
    void Start()
    {
        this._trackableBehaviour = GetComponent<TrackableBehaviour>();
        
        this._trackableBehaviour.RegisterTrackableEventHandler(this);
        Debug.Log("Finished Start()");
    }

    // Update is called once per frame
    void OnDestroy()
    {
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        Debug.Log($"state: {newStatus}");
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED)
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
}
