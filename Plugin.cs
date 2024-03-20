using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CompetitiveCompany.patches;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace CompetitiveCompany
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("io.github.CSync")]
    public class Plugin : BaseUnityPlugin
    {
        
        private const string modGUID = "org.agmas.CompetitiveCompany";
        private const string modName = "Competitive Company";

        private const string modVersion = "0.3.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static Plugin Instance;

        public static int blueScore = 0;

        public static bool startLogic = false;
        public static bool warnship = true;

        public static bool finedForShip = false;
        public static int redScore = 0;

        public static int curHour = 0;
        public static int nextHour = 0;

        public static Dictionary<PlayerControllerB, int> teams = new Dictionary<PlayerControllerB, int>();
        public static Dictionary<PlayerControllerB, bool> initiated = new Dictionary<PlayerControllerB, bool>();
        public ManualLogSource mls;

        public static Config MyConfig { get; internal set; }

        

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
            } 
            MyConfig = new(base.Config);
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("We're up and running!");
            harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(HudManagerPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(GrabbableObjectPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(Config));
        }
    }
}
