using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubWorld : World {

    public PortalSelector selector = null;

    public override void Awake()
    {
        base.Awake();
        Debug.Assert(selector != null);
    }

	// Use this for initialization
	public override void Start () {
        base.Start();
        Debug.Log("HubWorld " + name);

        Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        selector.Initialize(cam, defaultPortal);
	}
}
