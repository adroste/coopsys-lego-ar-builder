using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class GameManager : MonoBehaviour, IVirtualButtonEventHandler
{
    private GameObject _buildMarker;
    private GameObject[] _modelParts = new GameObject[5];
    private GameObject _part1Marker;
    private GameObject[] _part1Parts = new GameObject[5];
    private GameObject _part2Marker;
    private GameObject[] _part2Parts = new GameObject[5];
    private Text _curStepText;
    private int _curStep = 1;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting...");
        this._buildMarker = GameObject.Find("BuildMarker");
        this._part1Marker = GameObject.Find("NextPartMarker");
        this._part2Marker = GameObject.Find("SubsequentPartMarker");
        this._curStepText = GameObject.Find("CurrentStepText").GetComponent<Text>();

        // get single parts (sorted) of composed model
        Transform model = this._buildMarker.transform.Find("model");
        for (int i = 0; i < 5; ++i)
            this._modelParts[i] = model.Find("b" + (i + 1)).gameObject;

        // get single parts (sorted) for next/subsequent part sections
        Transform parts1 = this._part1Marker.transform.Find("parts");
        for (int i = 0; i < 5; ++i)
            this._part1Parts[i] = parts1.Find("b" + (i + 1)).gameObject;

        Transform parts2 = this._part2Marker.transform.Find("parts");
        for (int i = 0; i < 5; ++i)
            this._part2Parts[i] = parts2.Find("b" + (i + 1)).gameObject;

        // register next/previous buttons
        foreach (var virtualButtonBehaviour in this._buildMarker.GetComponentsInChildren<VirtualButtonBehaviour>(true))
            virtualButtonBehaviour.RegisterEventHandler(this);

        this._switchStep(1);
        Debug.Log("Finished Start()");
    }

    void OnDestroy()
    {
        foreach (var virtualButtonBehaviour in this._buildMarker.GetComponentsInChildren<VirtualButtonBehaviour>(true))
            virtualButtonBehaviour.UnregisterEventHandler(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonPressed(VirtualButtonBehaviour vb)
    {
        Debug.Log($"Button {vb.VirtualButtonName} pressed");
        switch (vb.VirtualButtonName)
        {
            case "VirtualButtonBack":
                // this._whiteSphere.SetActive(false);
                this._switchStep(this._curStep - 1);
                break;
            case "VirtualButtonNext":
                // this._blueSphere.SetActive(false);
                this._switchStep(this._curStep + 1);
                break;  
            default:
                throw new UnityException("Button not supported: " + vb.VirtualButtonName);
                break;
        }
    }
    
    public void OnButtonReleased(VirtualButtonBehaviour vb)
    {
        Debug.Log($"Button {vb.VirtualButtonName} released");
    }

    private void _switchStep(int nextStep) {
        if (nextStep < 1)
            nextStep = 1;
        else if (nextStep > 5)
            nextStep = 5;
        
        this._curStep = nextStep;
        this._curStepText.text = "Step: " + nextStep + "/5";

        for (int i = 0; i < 5; ++i) {
            // change displayed model
            if (i <= nextStep - 1)
                this._modelParts[i].SetActive(true);
            else
                this._modelParts[i].SetActive(false);

            // change displayed bricks on part list
            if (i == nextStep - 1)
                this._part1Parts[i].SetActive(true);
            else
                this._part1Parts[i].SetActive(false);

            if (i == nextStep)
                this._part2Parts[i].SetActive(true);
            else
                this._part2Parts[i].SetActive(false);
        }

        for (int i = 0; i < 5; ++i) {
            if (i == nextStep - 1)
                this._part1Parts[i].SetActive(true);
            else
                this._part1Parts[i].SetActive(false);
        }
    }
}
