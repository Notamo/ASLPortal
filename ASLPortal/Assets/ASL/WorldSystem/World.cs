using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.WorldSystem
{
    /// <summary>
    /// The Primary World Class. Worlds with extra initialization, 
    /// functionality, etc. should inherit from this one.
    /// </summary>
    public class World : MonoBehaviour
    {
        /// <summary>
        /// This reference can be used to instantiate shared objects and check for master client.
        /// </summary>
        public UWBNetworkingPackage.NetworkManager network = null;

        /// <summary>
        /// Use this to add objects to your world and communicate with other worlds.
        /// </summary>
        public WorldManager worldManager = null;

        /// <summary>
        /// Find references to the NetworkManager and WorldManager.
        /// Alternatively, if you override this method, they can be 
        /// set within the Unity editor. 
        /// </summary>
        public virtual void Awake()
        {
            network = GameObject.Find("NetworkManager").GetComponent<UWBNetworkingPackage.NetworkManager>();
            worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
            Debug.Assert(network != null);
            Debug.Assert(worldManager != null);
        }

        /// <summary>
        /// Override this method to initialize your world.
        /// </summary>
        public virtual void Init()
        {
        }
    }
}
