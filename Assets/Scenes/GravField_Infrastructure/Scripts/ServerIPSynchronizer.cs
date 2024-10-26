using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;
using UnityEngine.Events;


public class ServerIPSynchronizer : MonoBehaviour
{
    private string serverIp = "";
    public string ServerIP
    {
        get => serverIp;
    }

    public bool IsServerIpValid
    {
        get
        {
            return (serverIp.Length > 0 && IsIPAddressValide(serverIp));
        }
    }

    [SerializeField]
    OscPropertySender oscSender;
    [SerializeField]
    OscEventReceiver oscReceiver;


    void Start()
    {
        ResetConnection();
    }

    //public void StartReceivingServerIp(UnityAction<string> action)
    //{
    //    oscReceiver.enabled = true;

    //    TryReceivingServerIp(action);
    //}

    //IEnumerator TryReceivingServerIp(UnityAction<string> action)
    //{
    //    float start_time = Time.time;
    //    bool result = false;
    //    while (Time.time - start_time < 10)
    //    {
    //        if (serverIp.Length > 0 && IsIPAddressValide(serverIp))
    //        {
                
    //            result = true;
    //            break;
    //        }
    //        yield return null;
    //    }

    //    oscReceiver.enabled = false;
    //    if (result)
    //    {
    //        // successfully received the server ip
    //        action?.Invoke(serverIp);
    //    }
    //    else
    //    {
    //        // failed to receive the server ip
    //        action?.Invoke("");
    //    }
    //}

    public void OnReceiveServerIp(string ip)
    {
        if (ip.Length > 0 && IsIPAddressValide(ip) && serverIp != ip)
        {
            serverIp = ip;
            Debug.Log($"[{this.GetType()}]Received Server Ip:{ip}");
        }            
    }

    public void StartBroadcastingServerIp(string ip)
    {
        serverIp = ip;

        // keep sending server ip
        oscSender.enabled = true;
    }

    public void ResetConnection()
    {
        serverIp = "";
        oscSender.enabled = false;
        oscReceiver.enabled = true;
    }

    public bool IsIPAddressValide(string ip)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$");
    }
}
