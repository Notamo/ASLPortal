﻿using System.Collections;
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

        //WorldManager Events
        public const byte EV_WORLD_ADD = 30;    //add a world to the worldManager
        public const byte EV_WORLD_ADD_TO = 31;    //add an entity to one of the worlds
        public const byte EV_MASTER_LOAD = 32;  //master just created worlds
        public const byte EV_AVATAR_MAKE = 33;  //avatar created by a user

        //PortalManager Events
        public const byte EV_PORTAL_REG = 40;
        public const byte EV_PORTAL_UNREG = 41;
        public const byte EV_PORTAL_LINK = 42;
        public const byte EV_PORTAL_UNLINK = 43;

        //Portal Event Master Client Responses
        public const byte EV_PORTAL_ACK = 44;
        public const byte EV_PORTAL_NCK = 45;
    }
}