using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Data struct used for properties across the network
[System.Serializable]
public struct AvatarInfo
{
    [SerializeField] public int playerID;
    [SerializeField] public int viewID;
    [SerializeField] public Vector3 spawnPosition;
    [SerializeField] public Color color;

    public AvatarInfo(int playerID, int viewID, Vector3 spawnPosition, Color color)
    {
        this.playerID = playerID;
        this.viewID = viewID;
        this.spawnPosition = spawnPosition;
        this.color = color;
    }
}


public class PlayerAvatar : MonoBehaviour {
    private bool initialized = false;
    private bool controlled = false;

    public float movementSpeed = 10.0f;
    public float rotateSpeed = 10.0f;
    public float acceleration = 10.0f;

    private Rigidbody rigidBody = null;

    public Camera userCamera = null;
    public MasterController controller = null;

    private UserCursor mCursor = null;
    int src = -1;
    int dest = -1;

    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody>();
        Debug.Assert(rigidBody != null);
    }

    public void Initialize(AvatarInfo avatarProperties, Camera mainCamera, MasterController mc, GameObject cursorPrefab)
    {
        SetColor(avatarProperties.color);
        transform.localPosition = avatarProperties.spawnPosition;

        if (avatarProperties.playerID == PhotonNetwork.player.ID) {
            controlled = true;
            controller = mc;
            userCamera = mainCamera;
            userCamera.transform.SetParent(transform);
            userCamera.transform.localPosition = 0.5f * transform.up;
            SetCursor(cursorPrefab);
        }

        initialized = true;
    }

    public void SetColor(Color toSet)
    {
        GetComponent<MeshRenderer>().material.color = toSet;
    }

    public void SetCursor(GameObject cursorPrefab)
    {
        mCursor = Instantiate(cursorPrefab, transform).GetComponent<UserCursor>();
    }

    // Update is called once per frame
    void Update () {
        if (!initialized)
            return;

        if (controlled)
        {
            PlayerMovementControls();
            PlayerPortalControls();
        }
    }
    private void PlayerPortalControls()
    {
        // Toggle cursor and controls
        if (Input.GetKeyDown(KeyCode.P))
        {
            mCursor.HideCursor(!mCursor.IsHidden());
        }
        //Create/Register/Link Portals
        if (!mCursor.IsHidden())
        {
            mCursor.UpdateCursor(transform.rotation);

            if (controller != null)
            {
                //Create Portal
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 pos = mCursor.transform.position;
                    controller.PlayerCreatePortal(pos, -mCursor.transform.forward, mCursor.transform.up);
                }

                //Create webcam portal
                if(Input.GetKeyDown(KeyCode.C))
                {
                    Vector3 pos = mCursor.transform.position + 0.01f * mCursor.transform.up;
                    controller.PlayerCreatePortal(pos, mCursor.transform.up, -mCursor.transform.forward, Portal.ViewType.PHYSICAL);
                }

                //Register Portal
                if (Input.GetKeyDown(KeyCode.R))
                {
                    GameObject portalObj = mCursor.GetPortal();
                    if (portalObj != null)
                    {
                        controller.PlayerRegisterPortal(portalObj);
                    }
                }
                //Link Portal Source
                if (Input.GetKeyDown(KeyCode.T))
                {
                    GameObject portalObj = mCursor.GetPortal();
                    if (portalObj != null)
                    {
                        src = portalObj.GetComponent<PhotonView>().viewID;
                    }
                    controller.linkPanel.setSourceID(src);
                }
                //Link Portal Destination
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    GameObject portalObj = mCursor.GetPortal();
                    if (portalObj != null)
                    {
                        dest = portalObj.GetComponent<PhotonView>().viewID;
                    }
                    controller.linkPanel.setDestID(dest);
                }
                //Link Portal
                if (Input.GetKeyDown(KeyCode.U))
                {
                    if (src != -1 && dest != -1)
                    {
                        controller.portalManager.RequestLinkPortal(src, dest);
                        src = -1;
                        dest = -1;
                    }
                }
                //UnLink Portal
                if (Input.GetKeyDown(KeyCode.X))
                {
                    GameObject portalObj = mCursor.GetPortal();
                    if (portalObj != null)
                    {
                        controller.portalManager.RequestUnlinkPortal(portalObj.GetComponent<Portal>());
                    }
                }
            }
            else
            {
                Debug.LogError("No World Set!");
            }
        }
    }

    private void PlayerMovementControls()
    {

        #region WASD
        Vector3 moveVelocity = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            //transform.localPosition += transform.forward * Time.deltaTime * movementSpeed;
            moveVelocity += transform.forward * acceleration;
            
        }        

        if (Input.GetKey(KeyCode.A))
        {
            //transform.localPosition -= transform.right * Time.deltaTime * movementSpeed;
            moveVelocity -= transform.right * movementSpeed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            //transform.localPosition -= transform.forward * Time.deltaTime * movementSpeed;
            moveVelocity -= transform.forward * movementSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            //transform.localPosition += transform.right * Time.deltaTime * movementSpeed;
            moveVelocity += transform.right * movementSpeed;
        }
        rigidBody.velocity = moveVelocity;



        #endregion

        #region CAM_ROTATE
        if (Input.GetMouseButton(1))
        {
            float deltaMouseX = Input.GetAxis("Mouse X");
            float deltaMouseY = Input.GetAxis("Mouse Y");
            
            transform.Rotate(Vector3.up, deltaMouseX * rotateSpeed);
            userCamera.transform.Rotate(Vector3.right, -deltaMouseY * rotateSpeed);
        }
        #endregion
    }
}
