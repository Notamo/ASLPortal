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

        public const byte EV_WORLD_MADE = 30;
        //Portal Events
        public const byte EV_PORTAL_REG = 40;
        public const byte EV_PORTAL_UNREG = 41;
        public const byte EV_PORTAL_LINK = 42;
        public const byte EV_PORTAL_UNLINK = 43;

        //Portal Event Master Client Responses
        public const byte EV_PORTAL_ACK = 44;
        public const byte EV_PORTAL_NCK = 45;
    }
}