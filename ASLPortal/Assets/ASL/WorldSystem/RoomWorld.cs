using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomWorld : HubWorld {
    public RoomLoader roomLoader = null;
    public string roomName = "UW1_260";
    public Transform roomOrigin = null;

    public override void Awake()
    {
        base.Awake();
        roomLoader = GetComponent<RoomLoader>();
        Debug.Assert(roomOrigin != null);
    }
    public override void Init()
    {
        base.Init();
        Debug.Log("Room World " + name);

        //load in the room geometry
        GetComponent<RoomLoader>().LoadRoom(roomName, roomOrigin);
    }
}
