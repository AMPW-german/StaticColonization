﻿using KerbalKonstructs.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalColonies.colonyFacilities
{
    internal class KCStorageFacilityWindow : KCWindowBase
    {
        KCStorageFacility storageFacility;
        private static HashSet<PartResourceDefinition> allResources = new HashSet<PartResourceDefinition>();
        private static HashSet<string> blackListedResources = new HashSet<string> { "ElectricCharge", "IntakeAir" };


        internal static void GetVesselResources()
        {
            double amount = 0;
            double maxAmount = 0;
            foreach (PartResourceDefinition availableResource in PartResourceLibrary.Instance.resourceDefinitions)
            {
                foreach (var partSet in FlightGlobals.ActiveVessel.crossfeedSets)
                {
                    partSet.GetConnectedResourceTotals(availableResource.id, out amount, out maxAmount, true);
                    if (maxAmount > 0)
                    {
                        allResources.Add(availableResource);
                        break;
                    }

                }
            }
        }

        private bool vesselHasRessources(Vessel v, float amount)
        {
            v.GetConnectedResourceTotals(storageFacility.getRessource().id, false, out double vesselAmount, out double vesselMaxAmount);
            if (vesselAmount >= amount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool facilityHasRessources(float amount)
        {
            KSPLog.print(amount);
            KSPLog.print(storageFacility.amount);
            if (storageFacility.amount >= amount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// checks if the vessel v has enough space to add amount of r to it.
        /// </summary>
        private bool vesselHasSpace(Vessel v, PartResourceDefinition r, float amount)
        {
            v.GetConnectedResourceTotals(r.id, false, out double vesselAmount, out double vesselMaxAmount);
            if (vesselMaxAmount - vesselAmount >= amount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool facilityHasSpace(float amount)
        {
            if (storageFacility.getMaxVolume() - storageFacility.getCurrentVolume() >= amount * storageFacility.getRessource().volume)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void CustomWindow()
        {
            //int maxVolume = (int)Math.Round(KCStorageFacility.maxVolume, 0);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"MaxVolume: {storageFacility.maxVolume}", LabelGreen, GUILayout.Height(18));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"UsedVolume: {storageFacility.getCurrentVolume()}", LabelGreen, GUILayout.Height(18));
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUI.enabled = true;
            List<int> valueList = new List<int> { -100, -10, -1, 1, 10, 100 };

            foreach (int i in valueList)
            {
                if (GUILayout.Button(i.ToString(), GUILayout.Height(18), GUILayout.Width(32)))
                {
                    if (i < 0)
                    {
                        if (vesselHasSpace(FlightGlobals.ActiveVessel, storageFacility.getRessource(), i) && facilityHasRessources(-i))
                        {
                            FlightGlobals.ActiveVessel.rootPart.RequestResource(storageFacility.getRessource().id, (double) i);
                            storageFacility.changeAmount(i);
                            Configuration.SaveColonies("KCCD");
                        }
                    }
                    else
                    {
                        if (facilityHasSpace(i) && vesselHasRessources(FlightGlobals.ActiveVessel, i))
                        {
                            FlightGlobals.ActiveVessel.rootPart.RequestResource(storageFacility.getRessource().id, (double)i);
                            storageFacility.changeAmount(i);
                            Configuration.SaveColonies("KCCD");
                        }
                    }
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            bool changeRessource = true;

            if (GUILayout.Button("Ressource: " + storageFacility.getRessource().name, GUILayout.Height(23)))
            {
                changeRessource = true;
                toolRect = new Rect(toolRect.x, toolRect.y, 330, 300);
            }

            if (changeRessource)
            {
                GUILayout.BeginScrollView(new Vector2());
                if (GUILayout.Button("Cancel - No change", GUILayout.Height(23)))
                {
                    changeRessource = false;
                    toolRect = new Rect(toolRect.x, toolRect.y, 330, 100);
                }

                foreach (PartResourceDefinition r in allResources)
                {
                    if (blackListedResources.Contains(r.name))
                    {
                        continue;
                    }
                    if (GUILayout.Button(r.name, GUILayout.Height(23)))
                    {
                        KSPLog.print(r.name);
                        storageFacility.setRessource(r);
                        storageFacility.changeAmount(-storageFacility.getAmount());
                        changeRessource = false;
                        toolRect = new Rect(toolRect.x, toolRect.y, 330, 100);
                    }
                }
                GUILayout.EndScrollView();
            }
        }

        public KCStorageFacilityWindow(KCStorageFacility storageFacility) : base(Configuration.createWindowID(storageFacility))
        {
            this.storageFacility = storageFacility;
            GetVesselResources();
            toolRect = new Rect(100, 100, 330, 120);
        }
    }

    [System.Serializable]
    internal class KCStorageFacility : KCFacilityBase
    {
        internal PartResourceDefinition resource;
        internal float amount = 0f;
        internal float maxVolume;
        internal float currentVolume { get { return amount * ((resource != null) ? resource.volume : 0); } }

        internal PartResourceDefinition getRessource() { return resource; }
        internal void setRessource(PartResourceDefinition r) { resource = r; }

        internal float getAmount()
        {
            return amount;
        }
        internal float getCurrentVolume()
        {
            return currentVolume;
        }
        internal float getMaxVolume()
        {
            return maxVolume;
        }

        private KCStorageFacilityWindow StorageWindow;

        public override void EncodeString()
        {
            facilityData = $"ressource&{((resource != null) ? resource.id : -1)}|amount&{amount}|maxVolume&{maxVolume}";
        }

        public override void DecodeString()
        {
            if (facilityData != "")
            {
                {
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    foreach (string s in facilityData.Split('|'))
                    {
                        data.Add(s.Split('&')[0], s.Split('&')[1]);
                    }
                    resource = (int.Parse(data["ressource"]) != -1) ? PartResourceLibrary.Instance.GetDefinition(int.Parse(data["ressource"])) : null;
                    amount = float.Parse(data["amount"]);
                    maxVolume = float.Parse(data["maxVolume"]);
                }
            }
        }

        /// <summary>
        /// changes the stored amount by a given value. Returns false if more is pulled out than stored.
        /// </summary>
        internal bool changeAmount(float amount)
        {
            if (amount * -1.0 > this.amount)
            {
                return false;
            }
            else
            {
                this.amount += amount;
                return true;
            }
        }

        internal override void Update()
        {
            base.Update();

            if (maxVolume == 0f)
            {
                GameObject instace = KerbalKonstructs.API.GetGameObject(KCFacilityBase.GetUUIDbyFacility(this));
                if (instace != null)
                {
                    Vector3 size = instace.GetRendererBounds().extents;
                    maxVolume = (size.x * size.y * size.z);
                }
            }
        }

        internal override void OnBuildingClicked()
        {
            KSPLog.print("KCStorageWindow: " + StorageWindow.ToString());
            StorageWindow.Toggle();
        }

        internal override void Initialize(string facilityName, int id, string facilityData, bool enabled)
        {
            base.Initialize(facilityName, id, facilityData, enabled);
            this.StorageWindow = new KCStorageFacilityWindow(this);
            resource = PartResourceLibrary.Instance.GetDefinition("Ore");
        }

        internal KCStorageFacility(bool enabled, float maxVolume = 0f)
        {
            this.maxVolume = maxVolume;
            Initialize("KCStorageFacility", createID(), "", enabled);
        }
    }
}