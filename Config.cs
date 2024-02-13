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
public class Config : SyncedInstance<Config>
{
        [DataMember] internal SyncedEntry<int> team1Suit { get; private set; }
        [DataMember] internal SyncedEntry<int> team2Suit { get; private set; }

        [DataMember] internal SyncedEntry<string> team1Name { get; private set; }
        [DataMember] internal SyncedEntry<string> team2Name { get; private set; }

        [DataMember] internal SyncedEntry<string> team1ColorCode { get; private set; }
        [DataMember] internal SyncedEntry<string> team2ColorCode { get; private set; }

        [DataMember] internal SyncedEntry<bool> ALLREQUIRED_respawning { get; private set; }



        
        public Config(ConfigFile cfg) {
            InitInstance(this);
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

            ALLREQUIRED_respawning = cfg.BindSyncedEntry(
                "All Required",                  // Config subsection
                "Respawn",                  // Key of this config
                false,                               // Default value
                "WARNING: ALL PLAYERS ARE REQUIRED TO HAVE THE MOD, OR IT COULD CAUSE MASSIVE DESYNC!!!\nRespawns all dead players every hour."         // Description
            );
            
        }

        [HarmonyPostfix]
[HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
public static void InitializeLocalPlayer() {
    Plugin.Instance.mls.LogError("10088 - Start");
    if (IsHost) {
        MessageManager.RegisterNamedMessageHandler("CompCompany_OnRequestConfigSync", OnRequestSync);
        Synced = true;

        return;
    }

    Synced = false;
    MessageManager.RegisterNamedMessageHandler("CompCompany_OnReceiveConfigSync", OnReceiveSync);
    RequestSync();
}

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        public static void PlayerLeave() {
            Config.RevertSync();
        }

    internal static void RequestSync() {
    if (!IsClient) return;

    using FastBufferWriter stream = new(IntSize, Allocator.Temp);

    // Method `OnRequestSync` will then get called on host.
    stream.SendMessage("CompCompany_OnRequestConfigSync");
}

internal static void OnRequestSync(ulong clientId, FastBufferReader _) {
    if (!IsHost) return;

    byte[] array = SerializeToBytes(Instance);
    int value = array.Length;

    using FastBufferWriter stream = new(value + IntSize, Allocator.Temp);

    try {
        stream.WriteValueSafe(in value, default);
        stream.WriteBytesSafe(array);

        stream.SendMessage("CompCompany_OnReceiveConfigSync", clientId);
    } catch(Exception e) {
        Plugin.Instance.mls.LogError($"10088 - Error occurred syncing config with client: {clientId}\n{e}");
    }
}

internal static void OnReceiveSync(ulong _, FastBufferReader reader) {
    if (!reader.TryBeginRead(IntSize)) {
        Plugin.Instance.mls.LogError("10088 - Config sync error: Could not begin reading buffer.");
        return;
    }

    reader.ReadValueSafe(out int val, default);
    if (!reader.TryBeginRead(val)) {
        Plugin.Instance.mls.LogError("10088 - Config sync error: Host could not sync.");
        return;
    }

    byte[] data = new byte[val];
    reader.ReadBytesSafe(ref data, val);

    try {
        SyncInstance(data);
    } catch(Exception e) {
        Plugin.Instance.mls.LogError($"10088 - Error syncing config instance!\n{e}");
    }
}
}