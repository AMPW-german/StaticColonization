﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KerbalKonstructs;
using UnityEngine;

// KC: Kerbal Colonies
// This mod aimes to create a colony system with Kerbal Konstructs statics
//Copyright (C) 2024 AMPW, Halengar

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/

namespace KerbalColonies
{
    internal static class Colonies
    {
        static string activeColony = "";

        internal static void GroupSaved(KerbalKonstructs.Core.GroupCenter groupCenter)
        {
            KerbalKonstructs.API.CreateGroup(activeColony, groupCenter.RadialPosition);
            KerbalKonstructs.API.CopyGroup(activeColony, $"{activeColony}_temp");
            KerbalKonstructs.API.RemoveGroup($"{activeColony}_temp");
            KerbalKonstructs.API.UnRegisterOnGroupSaved(GroupSaved);
            KerbalKonstructs.API.Save();
        }

        internal static bool CreateColony()
        {
            Vessel vessel = FlightGlobals.ActiveVessel;
            vessel.SetPosition(new Vector3d(vessel.latitude, vessel.longitude, vessel.GetHeightFromTerrain() + 4));
            vessel.easingInToSurface = true;
            EditorGroupPlace("KC_CAB", $"KC_{FlightGlobals.currentMainBody.name}"); //CAB: Colony Assembly Hub, initial start group
            return true;
        }

        internal static bool EditorGroupPlace(string groupName, string colonyName)
        {
            activeColony = colonyName;
            KerbalKonstructs.API.CreateGroup($"{colonyName}_temp");
            KerbalKonstructs.API.CopyGroup($"{colonyName}_temp", groupName);
            KerbalKonstructs.API.OpenGroupEditor($"{colonyName}_temp");
            KerbalKonstructs.API.RegisterOnGroupSaved(GroupSaved);
            return true;
        }
    }
}