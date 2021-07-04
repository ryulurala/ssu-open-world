using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager
{
    ServerSession _session = new ServerSession();

#if UNITY_EDITOR
    const string url = "ws://localhost";
#else
    const string url = "ws://localhost";
#endif

    public void OnUpdate()
    {
        _session.OnDispatch();

        // 1-frame
        List<Packet> list = Manager.Packet.Queue.PopAll();
        foreach (Packet packet in list)
            Manager.Packet.HandlePacket(_session, packet);
    }

    public void Open(Action callback)
    {
        new Connector().Connect(_session, url, callback);
    }

    public void Close()
    {
        _session.Close("Exit Button Cliked");
    }

    public void Send<T>(Packet packet) where T : Packet
    {
        if (packet == null)
            return;

        T body = packet as T;
        string message = JsonUtility.ToJson(body);

        Send(message);
    }

    public void Send(string message)
    {
        if (_session == null)
            return;

        _session.Send(message);
    }
}