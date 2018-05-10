using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamQuadBehavior : MonoBehaviour {
    

    public static WebCamDevice[] devices;
    private static WebCamTexture camTexture;

    public int ActiveCam = 0;

    // Use this for initialization
    void Start () {


      devices = WebCamTexture.devices;
      Debug.Log("The following " + devices.Length + " webcams are available:");
      for (int i = 0; i < devices.Length; i++)
      {
         Debug.Log(devices[i].name);
      }

      //We must have at least one webcam device
      Debug.Assert(devices.Length >= 1);

      //We set our quad's texture to our camera feed.
      if (ActiveCam >= devices.Length)
      {
         Debug.Log("No camera available at index " + ActiveCam + ", switching to camera 0");
         ActiveCam = 0;
      }

      SetUpCamera();

      ToggleCam();
    }

    public void ToggleCam()
    {
        if (camTexture.isPlaying)
        {
            camTexture.Stop();
        } else
        {
            camTexture.Play();
        }
    }

   private void SetUpCamera()
   {
      Debug.Log("Attempting to activate the following camera: " + devices[ActiveCam].name);
      camTexture = new WebCamTexture("" + devices[ActiveCam].name);

      Debug.Log("Activating camera at index[" + ActiveCam + "]:" + camTexture.deviceName);
      GetComponent<MeshRenderer>().material.mainTexture = camTexture;
   }

   private void NextCamera()
   {
      ActiveCam++;
      if (devices == null)
      {
         SetUpCamera();
         return;
      }

      if (ActiveCam >= devices.Length)
      {
         ActiveCam = 0;
      }

      SetUpCamera();
      

   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Q))
      {
         NextCamera();
      }
   }
}
