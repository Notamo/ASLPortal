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
	public override void Init () {
        base.Init();
        Debug.Log("HubWorld " + name);

        Camera cam = Camera.main;

        Debug.Assert(defaultPortal != null);
        selector.Initialize(cam, defaultPortal);
	}
}
