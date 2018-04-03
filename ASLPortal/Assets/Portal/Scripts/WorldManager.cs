using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASL.Manipulation.Objects;

public class WorldManager : MonoBehaviour {

    public Dictionary<int, World> worlds;
    public List<string> worldPrefabs;

    public bool masterClient = false;
    private ObjectInteractionManager objManager;

    // Use this for initialization
    private void Awake()
    {

        objManager = GameObject.Find("ObjectInteractionManager").GetComponent<ObjectInteractionManager>();
        worlds = new Dictionary<int, World>();
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void CreateDefaultWorlds()
    {
        foreach (string worldName in worldPrefabs)
        {
            CreateWorld(worldName);
        }
    }

    
    public void CreateWorld(string prefabName)
    {
        Debug.Log("making world: " + prefabName);
        GameObject go = objManager.InstantiateOwnedObject(prefabName);
        World world = go.GetComponent<World>();

        AddWorld(world);
    }

    private void AddWorld(World world)
    {
        int worldId = world.GetComponent<PhotonView>().viewID;

        world.transform.parent = gameObject.transform;
        world.transform.localPosition = Vector3.right * worlds.Count * 20;          //improvements necessary (what if we remove a world?)

        worlds.Add(worldId, world);

        RaiseEventOptions options = new RaiseEventOptions();
        options.Receivers = ReceiverGroup.Others;
        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_WORLD_ADD, worldId, true, options);
    }

    //Set an object as a child of a world
    public void AddToWorld(World world, GameObject go)
    {
        int worldId = world.GetComponent<PhotonView>().viewID;

        PhotonView view = go.GetComponent<PhotonView>();
        if(view == null)
        {
            Debug.LogError("Cannot add to world! Must have photon view!");
            return;
        }

        go.transform.parent = worlds[worldId].transform;

        //send an event off
        int[] pair = { worldId, view.viewID };

        RaiseEventOptions options = new RaiseEventOptions();
        options.Receivers = ReceiverGroup.Others;
        PhotonNetwork.RaiseEvent(UWBNetworkingPackage.ASLEventCode.EV_WORLD_ADD_TO, pair, true, options);
    }


    public World GetWorldById(int id)
    {
        return worlds[id];
    }

    public World getWorldByName(string worldName)
    {
        foreach (KeyValuePair<int, World> pair in worlds)
        {
            if (pair.Value.name == worldName)
                return pair.Value;
        }

        return null;
    }

    //responses to events. In other worlds. i.e. synchronizing with the master client
    #region EVENT_PROCESSING
    //handle events specifically related to portal stuff
    private void OnEvent(byte eventCode, object content, int senderID)
    {
        //handle events specifically related to portal stuff
        switch (eventCode)
        {
            case UWBNetworkingPackage.ASLEventCode.EV_WORLD_ADD:
                ProcessWorldAdd((int)content);
                break;
            case UWBNetworkingPackage.ASLEventCode.EV_WORLD_ADD_TO:
                int[] addToWorldPair = (int[])content;
                ProcessWorldSet(addToWorldPair[0], addToWorldPair[1]);
                break;
        }
    }


    //someone else added a world
    //add it here too!
    private void ProcessWorldAdd(int worldId)
    {
        World world = PhotonView.Find(worldId).GetComponent<World>();
        worlds.Add(worldId, world);
    }
    
    private void ProcessWorldSet(int worldId, int toSetId)
    {
        World world = worlds[worldId];
        GameObject toSet = PhotonView.Find(toSetId).gameObject;

        toSet.transform.parent = world.transform;
    }

    #endregion

    //commented out for the time being
    #region WORLD_VISIBILITY
    /*
    //set the layer the camera can see
    public bool SetVisibleWorld(int playerID)
    {
        if (!worlds.ContainsKey(playerID))
        {
            Debug.LogError("Could not set world visible -- world does not exist!");
            return false;
        }

        if(visibleWorld != playerID)
        {
            if(visibleWorld != -1)
                SetGOInvisible(worlds[visibleWorld]);
            SetGOVisible(worlds[playerID]);
            visibleWorld = playerID;
        }

        return true;
    }

    public bool setAppropriateLayer(GameObject go, int playerID)
    {
        if (!worlds.ContainsKey(playerID))
        {
            Debug.LogError("Could not set world visible -- world does not exist!");
            return false;
        }

        if (playerID == visibleWorld)
            SetGOVisible(go);
        else
            SetGOInvisible(go);

        return true;
    }

    private void SetGOInvisible(GameObject go)
    {
        go.layer = LayerMask.NameToLayer("OtherWorld");

        for(int i = 0; i < go.transform.childCount; i++)
        {
            GameObject child = go.transform.GetChild(i).gameObject;
            SetGOInvisible(child);
        }
    }

    private void SetGOVisible(GameObject go)
    {
        go.layer = LayerMask.NameToLayer("ActiveWorld");

        for (int i = 0; i < go.transform.childCount; i++)
        {
            GameObject child = go.transform.GetChild(i).gameObject;
            SetGOVisible(child);
        }
    }
    */
    #endregion  //commented ot for the time being
}
