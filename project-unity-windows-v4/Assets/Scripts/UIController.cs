using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    [Tooltip("Refernz to the Inputfild for the IP-Address")]
    public InputField ipAddress;
    [Tooltip("Referenz to the Text for the local IP-Address")]
    public Text localIPAddress;


    public void Start()
    {
        // Get the local IP-Address and display it
        LocalIPAddress();
    }

    // Function for starting as a host
    public void StartLegoBuilder()
    {
        // Set the IP-Address
        GameManager.otherIp = ipAddress.text;
        SceneManager.LoadScene("AryzonScene");
    }

    public void LocalIPAddress()
    {
//        GameManager.localIp = IPManager.GetIP(ADDRESSFAM.IPv4);
        IPHostEntry host;
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                GameManager.localIp = ip.ToString();
                break;
            }
        }
        localIPAddress.text = $"Your local IP-Address is: {GameManager.localIp}";
    }
}

// Get IP Address: https://stackoverflow.com/questions/51975799/how-to-get-ip-address-of-device-in-unity-2018 
public class IPManager
{
    public static string GetIP(ADDRESSFAM Addfam)
    {
        //Return null if ADDRESSFAM is Ipv6 but Os does not support it
        if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
        {
            return null;
        }

        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
            NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif 
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    //IPv4
                    if (Addfam == ADDRESSFAM.IPv4)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }

                    //IPv6
                    else if (Addfam == ADDRESSFAM.IPv6)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
        }
        return output;
    }
}

public enum ADDRESSFAM
{
    IPv4, IPv6
}