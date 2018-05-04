using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalDestination : MonoBehaviour {

    private PortalManager portalMgr = null;
    private Transform follow = null;

    private void Start()
    {
        //alert portal manager
        //GameObject.Find("PortalManager").GetComponent<PortalManager>().ReceiveNewPortal(this);
    }
	
	// Update is called once per frame
	void Update () {
        if(follow != null)
        {
            transform.localPosition = follow.localPosition;
            transform.localScale = follow.localScale;
            transform.localRotation = follow.localRotation;
        }
    }

    public void setFollow(Transform follow)
    {
        this.follow = follow;
    }
}
