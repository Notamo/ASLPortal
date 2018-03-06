using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {

    public Dictionary<int, GameObject> worlds;
    public int myWorld = -1;
    public int visibleWorld = -1;

    // Use this for initialization
    private void Awake()
    {
        worlds = new Dictionary<int, GameObject>();
    }
	
	// Update is called once per frame
	void Update () {
		
        //check for disconnections and remove the world
	}

    public void AddWorld(int playerID, GameObject worldOrigin)
    {
        if (worlds.ContainsKey(playerID))
        {
            Debug.LogError("Cannot add world! Already exists");
            return;
        }

        worlds[playerID] = worldOrigin;
        worldOrigin.transform.parent = transform;
        worldOrigin.transform.localPosition = Vector3.forward * 10 * playerID;
        if (worldOrigin.name == "MyWorld")
        {
            Debug.Log("adding MyWorld");
            myWorld = playerID;
        }
    }

    public void AddNewWorld(int playerID)
    {
        if (worlds.ContainsKey(playerID))
        {
            Debug.LogError("Cannot add world! Already exists");
            return;
        }

        GameObject worldOrigin = new GameObject();
        worldOrigin.name = "World:" + playerID;

        AddWorld(playerID, worldOrigin);
    }

    public void RemoveWorld(int playerID)
    {

    }

    public void AddToWorld(int playerID, GameObject toAdd)
    {
        //set the layer
        if (playerID == PhotonNetwork.player.ID)
        {
            toAdd.layer = LayerMask.NameToLayer("MyWorld");
        }
        else
        {
            toAdd.layer = LayerMask.NameToLayer("OtherWorld");
        }

        if (!worlds.ContainsKey(playerID)){
            AddNewWorld(playerID);
        }
        toAdd.transform.parent = worlds[playerID].transform;
        Debug.Log("AddToWorld(): id: " + playerID + " toAdd:" + toAdd.name + " layer: " + LayerMask.LayerToName(toAdd.layer));
    }

    public GameObject GetWorldByID(int playerID)
    {
        return worlds[playerID];
    }


    #region WORLD_VISIBILITY
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

    #endregion
}
