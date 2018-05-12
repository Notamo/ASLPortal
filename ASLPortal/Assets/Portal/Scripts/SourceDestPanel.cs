using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SourceDestPanel : MonoBehaviour {

    public Text sourceText = null;
    public Text destText = null;

    //pulic PortalManager portalMgr = null;

	// Use this for initialization
	void Start () {
        Debug.Assert(sourceText != null);
        Debug.Assert(destText != null);
        //Debug.Assert(portalMgr != null);
	}

    public void setSourceID(int id)
    {
        sourceText.text = "Source ID: " + id;
    }

    public void setDestID(int id)
    {
        destText.text = "Destination ID: " + id;
    }
}
