using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UWBNetworkingPackage
{
    public static class ASLEventCode
    {
        public const byte EV_INSTANTIATE = 99;
        public const byte EV_DESTROYOBJECT = 98;
        public const byte EV_SYNCSCENE = 97;
        public const byte EV_JOIN = 55;
        public const byte EV_SYNC_OBJECT_OWNERSHIP_RESTRICTION = 96;

        public const byte EV_REGISTER_PORTAL = 40;
        public const byte EV_UNREGISTER_PORTAL = 41;
        public const byte EV_LINK_PORTALS = 42;
        public const byte EV_UNLINK_PORTALS = 43;
    }
}