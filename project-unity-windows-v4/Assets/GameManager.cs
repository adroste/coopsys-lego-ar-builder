using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Vuforia;

public class GameManager : MonoBehaviour, IVirtualButtonEventHandler
{
    private GameObject _buildMarker;
    private GameObject[] _modelParts = new GameObject[5];
    private GameObject[] _modelWfParts = new GameObject[5];
    private GameObject _part1Marker;
    private GameObject[] _part1Parts = new GameObject[5];
    private GameObject _part2Marker;
    private GameObject[] _part2Parts = new GameObject[5];
    private Text _curStepText;
    private TextMesh _part1InfoText;
    private TextMesh _part2InfoText;
    private int _curStep = 1;
    private static readonly string[] _partInfoTexts = {
        "Height: 2\n\nWidth: 4\n\nDepth: 2\n\nColor: light yellow", // part 1
        "Height: 1\n\nWidth: 4\n\nDepth: 2\n\nColor: red",
        "Height: 1\n\nWidth: 4\n\nDepth: 2\n\nColor: red",
        "Height: 1\n\nWidth: 6\n\nDepth: 2\n\nColor: orange",
        "Height: 2\n\nWidth: 2\n\nDepth: 2\n\nColor: yellow", // part 5
    };
    private Color[][] _partColors = new Color[5][];

    private int hostId;
    private int connectionId = 0;
    private int myReliableChannelId;
    
    public static string localIp = "127.0.0.1";
    public static string otherIp = "127.0.0.1";
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError("Starting...");
        this._buildMarker = GameObject.Find("BuildMarker");
        this._part1Marker = GameObject.Find("NextPartMarker");
        this._part2Marker = GameObject.Find("SubsequentPartMarker");
        this._curStepText = GameObject.Find("CurrentStepText").GetComponent<Text>();

        this._part1InfoText = this._part1Marker.transform.Find("InfoText").GetComponent<TextMesh>();
        this._part2InfoText = this._part2Marker.transform.Find("InfoText").GetComponent<TextMesh>();

        // get single parts (sorted) of composed model
        Transform model = this._buildMarker.transform.Find("model");
        for (int i = 0; i < 5; ++i)
        {
            this._modelParts[i] = model.Find("b" + (i + 1)).gameObject;
            var renderers = this._modelParts[i].GetComponentsInChildren<Renderer>();
            this._partColors[i] = new Color[renderers.Length];
            for (int j = 0; j < renderers.Length; j++)
                this._partColors[i][j] = renderers[j].material.color;
        }

        Transform modelWf = this._buildMarker.transform.Find("modelWireframe");
        for (int i = 0; i < 5; ++i)
            this._modelWfParts[i] = modelWf.Find("b" + (i + 1)).gameObject;

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

        initialiseNetwork();

        this._switchStep(1);
        
//        InvokeRepeating("BlinkActivePart", 0f, .4f);
        
        Debug.LogError("Finished Start()");
    }

    void initialiseNetwork()
    {        
        Debug.LogError("Initialising Network----------------------------------------------------------------------------");
        NetworkTransport.Init();
        
        ConnectionConfig config = new ConnectionConfig();
        myReliableChannelId  = config.AddChannel(QosType.Reliable);
        
        HostTopology topology = new HostTopology(config, 10);

        hostId = NetworkTransport.AddHost(topology, 8888);
        
        Debug.LogError($"LocalIP {localIp}");
        Debug.LogError($"OtherIP {otherIp}");
        connect();
        
        Debug.LogError("Finished initialising network ------------------------------------------------------------------");
    }

    void connect()
    {
        byte error;
        connectionId = NetworkTransport.Connect(hostId, otherIp, 8888, 0, out error);
        Debug.LogError($"Connection ID: {connectionId}");
    }

    void sendProgressUpdate()
    {
        Debug.LogError($"Sending ({_curStep}) -------------------------------------------------");
        byte error;
        byte[] currentStepBuffer = BitConverter.GetBytes(_curStep);
//        byte[] currentStepBuffer = BitConverter.GetBytes(4);
        NetworkTransport.Send(hostId, connectionId, myReliableChannelId, currentStepBuffer, currentStepBuffer.Length,  out error);
        Debug.LogError($"Finished sending ({_curStep}) -------------------------------------------------");
    }

    void receiveProgressUpdate()
    {
        int recHostId; 
        int connId; 
        int channelId; 
        int bufferSize = sizeof(int);
        byte[] recBuffer = new byte[bufferSize]; 
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.Nothing:                     
                break;
            case NetworkEventType.ConnectEvent:                
                Debug.LogError($"Connected to {otherIp} -------------------------------------------------");
                break;
            case NetworkEventType.DataEvent:
                int receivedStep = BitConverter.ToInt32(recBuffer, 0);
                Debug.LogError($"Receiving DATA: {receivedStep} ------------------------------------------------------");
                if (receivedStep != _curStep)
                {
                    _curStep = receivedStep;
                    _switchStep(_curStep);
                }
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.LogError($"Disconnect -------------------------------------------------");
                connectionId = 0;
                break;
            case NetworkEventType.BroadcastEvent:
                break;
        }
    }

    void OnDestroy()
    {
        byte error;
        NetworkTransport.Disconnect(hostId, connectionId, out error);
        foreach (var virtualButtonBehaviour in this._buildMarker.GetComponentsInChildren<VirtualButtonBehaviour>(true))
            virtualButtonBehaviour.UnregisterEventHandler(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (connectionId == 0)
            connect();
        if (connectionId != 0)
        {
//                sendProgressUpdate();
            receiveProgressUpdate();
        }
    }

    void BlinkActivePart()
    {
        GameObject mp = this._modelParts[this._curStep - 1];
        GameObject mwfp = this._modelWfParts[this._curStep - 1];
        mp.SetActive(!mp.activeSelf);
        mwfp.SetActive(!mwfp.activeSelf);
    }

    public void OnButtonPressed(VirtualButtonBehaviour vb)
    {
        Debug.LogError($"Button {vb.VirtualButtonName} pressed");
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
        Debug.LogError($"Button {vb.VirtualButtonName} released");
    }

    private void _switchStep(int nextStep) {
        if (nextStep < 1)
            nextStep = 1;
        else if (nextStep > 5)
            nextStep = 5;
        
        this._curStep = nextStep;
        this._curStepText.text = "Step: " + nextStep + "/5";

        this._part1InfoText.text = _partInfoTexts[nextStep - 1];
        this._part2InfoText.text = nextStep < 5 ? _partInfoTexts[nextStep] : "";

        for (int i = 0; i < 5; ++i) {
            // change displayed model
            if (i <= nextStep - 1) {
                this._modelParts[i].SetActive(true);
                this._modelWfParts[i].SetActive(false);
            }
            else {
                this._modelParts[i].SetActive(false);
                this._modelWfParts[i].SetActive(true);
            }
        
            if (i == nextStep - 2)
            {
                var renderers = this._modelParts[i].GetComponentsInChildren<Renderer>();
                for (int j = 0; j < renderers.Length; j++)
                {
                    renderers[j].material.shader = Shader.Find("Transparent/VertexLit with Z");
//                    StandardShaderUtils.ChangeRenderMode(renderers[j].material, StandardShaderUtils.BlendMode.Transparent);
                    var actualColor = this._partColors[i][j];
                    var newColor = actualColor;
                    newColor.a = 0.5f;
//                    var newColor = actualColor + 0.5f * (Color.white - actualColor);
                    renderers[j].material.color = newColor;
                }
            }

            if (i >= nextStep - 1)
            {
                var renderers = this._modelParts[i].GetComponentsInChildren<Renderer>();
                for(int j = 0; j < renderers.Length; j++)
                    renderers[j].material.color = this._partColors[i][j];
            }

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
        
        if (connectionId != 0)
            sendProgressUpdate();
    }
}