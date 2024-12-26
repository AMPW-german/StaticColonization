﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalColonies.colonyFacilities
{
    internal class KC_CAB_Window : KCWindowBase
    {
        private KC_CAB_Facility facility;

        protected override void CustomWindow()
        {
            KCFacilityBase.GetInformationByUUID(KCFacilityBase.GetUUIDbyFacility(facility), out string saveGame, out int bodyIndex, out string colonyName, out GroupPlaceHolder gph, out List<KCFacilityBase> facilities);

            GUILayout.BeginScrollView(new Vector2());
            {
                foreach (Type t in Configuration.BuildableFacilities.Keys)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(t.Name);
                    GUILayout.BeginVertical();
                    {
                        for (int i = 0; i < Configuration.BuildableFacilities[t].resourceCost.Count; i++)
                        {
                            GUILayout.Label($"{Configuration.BuildableFacilities[t].resourceCost.ElementAt(i).Key.displayName}: {Configuration.BuildableFacilities[t].resourceCost.ElementAt(i).Value}");
                        }
                    }
                    GUILayout.EndVertical();

                    if (!Configuration.BuildableFacilities[t].VesselHasRessources(FlightGlobals.ActiveVessel, 0)) { GUI.enabled = false; }
                    if (GUILayout.Button("Build"))
                    {
                        Configuration.BuildableFacilities[t].RemoveVesselRessources(FlightGlobals.ActiveVessel, 0);
                        KCFacilityBase KCFac = Configuration.CreateInstance(t, true, "");

                        KCFacilityBase.CountFacilityType(t, saveGame, bodyIndex, colonyName, out int count);
                        string groupName = $"{colonyName}_{t.Name}_{count}";

                        KerbalKonstructs.API.CreateGroup(groupName);
                        Colonies.EditorGroupPlace(t, KCFac.baseGroupName, groupName, colonyName);
                    }
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }

                GUILayout.Label("Facilities in this colony:");

                GUILayout.BeginVertical();

                List<KCFacilityBase> colonyFacilitiyList = new List<KCFacilityBase>();

                Configuration.coloniesPerBody[saveGame][bodyIndex][colonyName].Values.ToList().ForEach(UUIDdict =>
                {
                    UUIDdict.Values.ToList().ForEach(colonyFacilitys =>
                    {
                        colonyFacilitys.ForEach(colonyFacility =>
                        {
                            if (!colonyFacilitiyList.Contains(colonyFacility))
                            {
                                colonyFacilitiyList.Add(colonyFacility);
                            }
                        });
                    });
                });


                colonyFacilitiyList.ForEach(colonyFacility =>
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(colonyFacility.name);
                    GUILayout.Label(colonyFacility.level.ToString());
                    GUILayout.Label(colonyFacility.GetFacilityProductionDisplay());

                    if (colonyFacility.upgradeable && colonyFacility.level < colonyFacility.maxLevel)
                    {
                        if (Configuration.BuildableFacilities[colonyFacility.GetType()].VesselHasRessources(FlightGlobals.ActiveVessel, colonyFacility.level + 1))
                        {
                            GUILayout.BeginVertical();
                            {
                                Configuration.BuildableFacilities[colonyFacility.GetType()].resourceCost.ToList().ForEach(pair =>
                                {
                                    GUILayout.Label($"{pair.Key.displayName}: {pair.Value}");
                                });

                                if (GUILayout.Button("Upgrade"))
                                {
                                    Configuration.BuildableFacilities[colonyFacility.GetType()].RemoveVesselRessources(FlightGlobals.ActiveVessel, colonyFacility.level + 1);
                                    if (colonyFacility.upgradeWithGroupChange)
                                    {
                                        KCFacilityBase.UpgradeFacilityWithGroupChange(colonyFacility);
                                    }
                                    else
                                    {
                                        KCFacilityBase.UpgradeFacility(colonyFacility);
                                    }
                                }
                            }
                            GUILayout.EndVertical();
                        }
                    }
                    GUILayout.EndHorizontal();
                });
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }


        public KC_CAB_Window(KC_CAB_Facility facility) : base(Configuration.createWindowID(facility), facility.name)
        {
            this.facility = facility;
            this.toolRect = new Rect(100, 100, 800, 1200);
        }
    }

    [System.Serializable]
    internal class KC_CAB_Facility : KCFacilityBase
    {
        private KC_CAB_Window window;

        internal override void OnBuildingClicked()
        {
            window.Toggle();
        }

        internal override void Initialize(string facilityData)
        {
            base.Initialize(facilityData);
            window = new KC_CAB_Window(this);
            enabled = true;
        }

        public KC_CAB_Facility() : base("KCCABFacility", true, "")
        {

        }
    }
}