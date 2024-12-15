﻿using KerbalKonstructs.Modules;
using System;

namespace KerbalColonies.colonyFacilities
{

    /// <summary>
    /// The KCFaciltiyBase class is used to create custom KCFacilities, you must register your types in the typeregistry at startup with the AWAKE method.
    /// If you have custom fields that you want to save overwrite the encode and decode string methods and save the values of your custom fields in the facilityData string.
    /// You can encode the data however you want but you are not allowed to use "{", "}", ",", ":", "=" and "//"
    /// This is because the datastring is saved as a value in a KSP confignode and using these symbols can mess up the loading
    /// I recommend using "|" as seperator and "&" instead of "="
    /// It's NOT checked if the datastring contains any of these symbols!
    /// </summary>
    [System.Serializable]
    public abstract class KCFacilityBase
    {
        public string facilityName;
        public int id;
        public bool enabled;
        public double lastUpdateTime;
        public double creationTime;
        public string facilityData;
        protected bool initialized = false;

        /// <summary>
        /// This function get automatically called, do not call it manually.
        /// The OnBuildingClicked method function should be used for custom windows.
        /// </summary>
        virtual internal void Update()
        {
            lastUpdateTime = HighLogic.CurrentGame.UniversalTime;
        }

        /// <summary>
        /// This function gets automatically called when the building is clicked, it might get used for custom windows.
        /// The update function will be called BEFORE this one, you don't need to do it manually
        /// </summary>
        virtual internal void OnBuildingClicked() { }

        internal static void OnBuildingClickedHandler(KerbalKonstructs.Core.StaticInstance instance)
        {
            if (Configuration.coloniesPerBody.ContainsKey(HighLogic.CurrentGame.Seed.ToString()))
            {
                if (Configuration.coloniesPerBody[HighLogic.CurrentGame.Seed.ToString()].ContainsKey(FlightGlobals.Bodies.IndexOf(FlightGlobals.currentMainBody)))
                {
                    if (Configuration.coloniesPerBody[HighLogic.CurrentGame.Seed.ToString()][FlightGlobals.Bodies.IndexOf(FlightGlobals.currentMainBody)].ContainsKey(instance.Group))
                    {
                        if (Configuration.coloniesPerBody[HighLogic.CurrentGame.Seed.ToString()][FlightGlobals.Bodies.IndexOf(FlightGlobals.currentMainBody)][instance.Group].ContainsKey(instance.UUID))
                        {
                            foreach (KCFacilityBase kcFacility in Configuration.coloniesPerBody[HighLogic.CurrentGame.Seed.ToString()][FlightGlobals.Bodies.IndexOf(FlightGlobals.currentMainBody)][instance.Group][instance.UUID])
                            {
                                KSPLog.print(Configuration.APP_NAME + ": " + instance.ToString());
                                kcFacility.Update();
                                kcFacility.OnBuildingClicked();
                            }
                        }
                    }
                }
            }
        }

        internal static string GetUUIDbyFacility(KCFacilityBase facility)
        {
            foreach (string saveGame in Configuration.coloniesPerBody.Keys)
            {
                foreach (int bodyId in Configuration.coloniesPerBody[saveGame].Keys)
                {
                    foreach (string colonyName in Configuration.coloniesPerBody[saveGame][bodyId].Keys)
                    {
                        foreach (string uuid in Configuration.coloniesPerBody[saveGame][bodyId][colonyName].Keys)
                        {
                            if (Configuration.coloniesPerBody[saveGame][bodyId][colonyName][uuid].Contains(facility))
                            {
                                return uuid;
                            }
                        }
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// Returns true if the facility with the corresponding id was found
        /// Returns false if the the id wasn't found.
        /// </summary>
        /// <returns>Important: if the facility wasn't found the outed facility is NULL</returns>
        internal static bool GetFacilityByID(int id, out KCFacilityBase facility)
        {
            foreach (string saveGame in Configuration.coloniesPerBody.Keys)
            {
                foreach (int bodyId in Configuration.coloniesPerBody[saveGame].Keys)
                {
                    foreach (string colonyName in Configuration.coloniesPerBody[saveGame][bodyId].Keys)
                    {
                        foreach (string uuid in Configuration.coloniesPerBody[saveGame][bodyId][colonyName].Keys)
                        {
                            foreach (KCFacilityBase fac in Configuration.coloniesPerBody[saveGame][bodyId][colonyName][uuid])
                            {
                                if (fac.id == id)
                                {
                                    facility = fac;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            facility = null;
            return false;
        }

        internal static bool IDexists(int id)
        {
            foreach (string saveGame in Configuration.coloniesPerBody.Keys)
            {
                foreach (int bodyId in Configuration.coloniesPerBody[saveGame].Keys)
                {
                    foreach (string colonyName in Configuration.coloniesPerBody[saveGame][bodyId].Keys)
                    {
                        foreach (string uuid in Configuration.coloniesPerBody[saveGame][bodyId][colonyName].Keys)
                        {
                            foreach (KCFacilityBase fac in Configuration.coloniesPerBody[saveGame][bodyId][colonyName][uuid])
                            {
                                if (fac.id == id)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        internal static int createID()
        {
            int id = 0;
            Random r = new Random();

            while (true)
            {
                id = r.Next();

                if (!IDexists(id))
                {
                    return id;
                }
            }

        }

        /// <summary>
        /// This method should create the facilityData string from custom fields in derived classes, this base method returns an empty string.
        /// </summary>
        virtual public void EncodeString()
        {
            facilityData = "";
        }

        /// <summary>
        /// This method should fill the custom fields of derived classes, this base method DOES NOTHING.
        /// </summary>
        virtual public void DecodeString() { }


        virtual internal void Initialize(string facilityName, int id, string facilityData, bool enabled)
        {
            if (!initialized)
            {
                this.facilityName = facilityName;
                this.id = id;
                this.facilityData = facilityData;
                this.enabled = enabled;
                lastUpdateTime = HighLogic.CurrentGame.UniversalTime;
                creationTime = HighLogic.CurrentGame.UniversalTime;
                initialized = true;
                DecodeString();
            }
        }

        /// <summary>
        /// The base constructor of the kc facilities. It only calls the initialize function.
        /// You can use a custom constructor but it should only call an overriden initialize function and not the base constructor
        /// This is necessary because of the serialization.
        /// </summary>
        protected KCFacilityBase(string facilityName, bool enabled, string facilityData)
        {
            Initialize(facilityName, createID(), facilityData, enabled);
        }

        protected KCFacilityBase() { }
    }
}
