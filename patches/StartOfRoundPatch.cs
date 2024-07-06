using HarmonyLib;
using Unity.Netcode;
using System;
using GameNetcodeStuff;
using UnityEngine.UIElements.Collections;
using UnityEngine;
using CompetitiveCompany.Util;
using System.Linq;

namespace CompetitiveCompany.patches {
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch {

        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        [HarmonyPostfix]
        static void undo(StartOfRound __instance) {
            Plugin.startLogic = false;
            if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost) {
            foreach (PlayerControllerB playerControllerB in StartOfRound.Instance.allPlayerScripts) {
                UnlockableSuit[] array = UnityEngine.Object.FindObjectsOfType<UnlockableSuit>(includeInactive: true);
                foreach (UnlockableSuit us in array) {
                    if (us.suitID == 0) {
                            us.SwitchSuitToThis(playerControllerB);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "WritePlayerNotes")]
        [HarmonyPrefix]
        static void teams()
        {
            Team winningTeam = Plugin.getWinningTeam();

            foreach (PlayerControllerB playerControllerB in StartOfRound.Instance.allPlayerScripts) {
                if (playerControllerB.disconnectedMidGame || playerControllerB.isPlayerDead || playerControllerB.isPlayerControlled) {
                        StartOfRound.Instance.gameStats.allPlayerStats[playerControllerB.playerClientId].playerNotes.Add(Plugin.getPlayerTeam(playerControllerB).teamName);
                        if (winningTeam.playersinTeam.Contains(playerControllerB)) {
                            StartOfRound.Instance.gameStats.allPlayerStats[playerControllerB.playerClientId].playerNotes.Add("Won the game!");
                        } else {
                            StartOfRound.Instance.gameStats.allPlayerStats[playerControllerB.playerClientId].playerNotes.Add("Lost the game..");
                        }
                }
            }
        }
        

        [HarmonyPatch(typeof(StartOfRound), "StartGame")]
        [HarmonyPostfix]
        static void gammaPatch(StartOfRound __instance) {
            if (__instance.IsServer || __instance.IsHost) {
                int a = 0;
                PlayerControllerB[] shuffled = (PlayerControllerB[])RoundManager.Instance.playersManager.allPlayerScripts.Clone();
                Randomizer.Randomize<PlayerControllerB>(shuffled);
                foreach (PlayerControllerB pcb in shuffled) {
                    if (pcb.isPlayerControlled) {
                        if (Plugin.teamList.Count >= a) {
                            a = 0;
                        }
                        Team assigned = Plugin.teamList[a];
                        HUDManager.Instance.AddTextToChatOnServer("<color=" + assigned.teamColorCode + ">" +pcb.playerUsername + " was put on " + assigned.teamName + "!");
                        
                        assigned.joinTeam(pcb);
                        a++;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "EndGameClientRpc")]
        [HarmonyPostfix]
        static void fine(StartOfRound __instance, ref int playerClientId)
        {
            StartOfRound.Instance.localPlayerController.isInElevator = true;
	        StartOfRound.Instance.localPlayerController.isInHangarShipRoom = true;
	        StartOfRound.Instance.localPlayerController.isInsideFactory = false;
            StartOfRound.Instance.localPlayerController.TeleportPlayer(RoundManagerPatch.GetPlayerSpawnPosition());
        
        }
    }
}

public class Randomizer
{
    public static void Randomize<T>(T[] items)
    {
        System.Random rand = new System.Random();

        // For each spot in the array, pick
        // a random item to swap into that spot.
        for (int i = 0; i < items.Length - 1; i++)
        {
            int j = rand.Next(i, items.Length);
            T temp = items[i];
            items[i] = items[j];
            items[j] = temp;
        }
    }
}