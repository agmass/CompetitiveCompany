using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CompetitiveCompany.patches;
using CompetitiveCompany.Util;
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
        public static bool pvpEnabled = true;

        public static bool startLogic = false;
        public static bool warnship = true;

        public static bool finedForShip = false;

        public static int curHour = 0;
        public static int nextHour = 0;

        public static Collection<Team> teamList = [new Team("Example Team 1", "red"),new Team("Example Team 2", "Blue")];
        public static Team teamless = new Team("Teamless", "gray");
        
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

        public static Team getWinningTeam() {
            int maximumPoints = 0;
            int ties = 0;
            Team winningTeam = teamList[0];
            foreach (Team t in teamList) {
                if (t.score > maximumPoints) {
                    winningTeam = t;
                    maximumPoints = t.score;
                }
                if (t.score == maximumPoints) {
                    ties++;
                    winningTeam = t;
                }
            }
            if (ties == teamList.Count) {
                return teamless;
            }
            return winningTeam;
        }
        public static Team getPlayerTeam(PlayerControllerB player) {
            if (teamless.playersinTeam.Contains(player)) {
                return teamless;
            }
            foreach (Team t in teamList) {
                if (t.playersinTeam.Contains(player)) {
                    return t;
                }
            }
            return teamless;
        }
    }
}
