using System;
using System.Runtime.Serialization;
using BepInEx.Configuration;
using CompetitiveCompany;
using CSync.Lib;
using CSync.Util;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Collections;
using Unity.Netcode;

[DataContract]
public class Config : SyncedConfig<Config>
{
        [DataMember] internal SyncedEntry<int> team1Suit { get; private set; }
        [DataMember] internal SyncedEntry<int> team2Suit { get; private set; }

        [DataMember] internal SyncedEntry<string> team1Name { get; private set; }
        [DataMember] internal SyncedEntry<string> team2Name { get; private set; }

        [DataMember] internal SyncedEntry<string> team1ColorCode { get; private set; }
        [DataMember] internal SyncedEntry<string> team2ColorCode { get; private set; }

        [DataMember] internal SyncedEntry<int> graceTime { get; private set; }
        [DataMember] internal SyncedEntry<int> fineAmount { get; private set; }



        
        public Config(ConfigFile cfg) : base("CompetitiveCompany") {
            
            ConfigManager.Register(this);
            //InitInstance(this);
            team1Suit = cfg.BindSyncedEntry(
                "Suits",                          // Config section
                "Team 1 Suits",                     // Key of this config
                0,                    // Default value
                "Originally the orange suit.\nIf you want to set a custom suit with AdditionalSuits, their suits\nGet logged when the game starts. Simply read the logs\nAnd use their SuitIDs!"    // Description
            );
    
            team2Suit = cfg.BindSyncedEntry(
                "Suits",                  // Config subsection
                "Team 2 Suits",                  // Key of this config
                3,                               // Default value
                "Originally the Pajama Suit"         // Description
            );

            team1Name = cfg.BindSyncedEntry(
                "Names",                          // Config section
                "Team 1 Name",                     // Key of this config
                "Orange",                    // Default value
                "Set the name for team 1."    // Description
            );
    
            team2Name = cfg.BindSyncedEntry(
                "Names",                  // Config subsection
                "Team 2 Name",                  // Key of this config
                "Blue",                               // Default value
                "Set the name for team 2."         // Description
            );

            team1ColorCode = cfg.BindSyncedEntry(
                "Colors",                          // Config section
                "Team 1 Color",                     // Key of this config
                "orange",                    // Default value
                "Set the color code for team 1. Hex codes can be used."    // Description
            );
    
            team2ColorCode = cfg.BindSyncedEntry(
                "Colors",                  // Config subsection
                "Team 2 Color",                  // Key of this config
                "blue",                               // Default value
                "Set the color code for team 2. Hex codes can be used."         // Description
            );

            graceTime = cfg.BindSyncedEntry(
                "Grace Time",                  // Config subsection
                "Grace Time",                  // Key of this config
                14,                               // Default value
                "How long grace lasts for. 12+ = PM"         // Description
            );

            fineAmount = cfg.BindSyncedEntry(
                "Grace Time",                  // Config subsection
                "Fine Amount",                  // Key of this config
                200,                               // Default value
                "How much should you be fined for leaving early"         // Description
            );
            
        }

        [HarmonyPostfix]
[HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
public static void InitializeLocalPlayer() {
    Plugin.Instance.mls.LogError("10088 - Start");
    if (IsHost) {
        Synced = true;

        return;
    }

    Synced = false;
}
}       
