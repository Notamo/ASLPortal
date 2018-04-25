﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using UWBNetworkingPackage;

public class RoomLoader : MonoBehaviour {

    public string RoomFolder = "";

    /// <summary>
    /// Reference to the persistent data path for the application
    /// </summary>
    private DirectoryInfo root;

    /// <summary>
    /// List of all Room objects
    /// </summary>
    private List<GameObject> roomList = new List<GameObject>();

    /// <summary>
    /// Stack to keep track of room loading
    /// </summary>
    private Stack<fileToLoad> FilesToLoad = new Stack<fileToLoad>();

    public Transform parent = null;

    /// <summary>
    /// Struct to keep track of file path and name for loading (i.e. metadata)
    /// </summary>
    private struct fileToLoad
    {
        public string filePath;
        public string name;
    }
    // Use this for initialization
    void Awake () {
        SetRoot();
        UnityEngine.Debug.Log("Root: " + root);
	}
	
	// Update is called once per frame
	void Update () {

        if (FilesToLoad.Count > 0)
        {
            fileToLoad f = FilesToLoad.Pop();
            ReadRoom(f.filePath, f.name);
        }
    }

    public void LoadRoom(string roomName, Transform parent)
    {
        this.parent = parent;
        RoomFolder = Path.Combine(root.FullName, roomName);
        
        DirectoryInfo info = new DirectoryInfo(RoomFolder);
        LoadRoomDI(info);

        UnityEngine.Debug.Log(FilesToLoad.Count + " room files");
    }

    private void LoadRoomDI(DirectoryInfo Dir)
    {
        foreach (FileInfo f in Dir.GetFiles())
        {
            fileToLoad file = new fileToLoad();
            int numNameComponents = f.Name.Split('.').Length;

            file.filePath = f.FullName;
            file.name = f.Name.Split('.')[0];
            string extension = '.' + f.Name.Split('.')[numNameComponents - 1];

            if (extension.Equals(Config.Current.Room.TangoFileExtension))
            {
                bool cached = false;
                foreach (GameObject g in roomList)
                {
                    if (g.name == file.name)
                    {
                        cached = true;
                    }
                }
                if (cached == false)
                {
                    FilesToLoad.Push(file);
                }
            }
        }
    }

    private DirectoryInfo SetRoot()
    {
#if UNITY_EDITOR
        root = new DirectoryInfo(Config.Current.Room.CompileAbsoluteAssetDirectory());
#else
            root = new DirectoryInfo(Config.Current.Room.CompileUnityAssetDirectory());
#endif
        return root;
    }


    /// <summary>
    /// Reads the room game object from the file path and sends it to the TangoDatabase with it's name
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="name"></param>
    private void ReadRoom(string filePath, string name)
    {
        byte[] b = File.ReadAllBytes(filePath);
        TangoDatabase.UpdateMesh(b, name, parent.GetComponent<PhotonView>().viewID);
    }
}
