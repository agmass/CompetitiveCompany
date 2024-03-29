using HarmonyLib;
using Unity.Netcode;
using System;
using GameNetcodeStuff;
using UnityEngine.UIElements.Collections;
using UnityEngine;

namespace CompetitiveCompany.patches {
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch {

        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        [HarmonyPostfix]
        static void undo(StartOfRound __instance) {
            Plugin.startLogic = false;
            Plugin.teams.Clear();
            if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost) {
            foreach (PlayerControllerB playerControllerB in StartOfRound.Instance.allPlayerScripts) {
                UnlockableSuit[] array = UnityEngine.Object.FindObjectsOfType<UnlockableSuit>(includeInactive: true);
                foreach (UnlockableSuit us in array) {
                    if (us.suitID == 2) {
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
            foreach (PlayerControllerB playerControllerB in StartOfRound.Instance.allPlayerScripts) {
                if (playerControllerB.disconnectedMidGame || playerControllerB.isPlayerDead || playerControllerB.isPlayerControlled) {
                    if (Plugin.teams.ContainsKey(playerControllerB)) {
                        if (Plugin.teams.Get(playerControllerB) == 0) {
                            StartOfRound.Instance.gameStats.allPlayerStats[playerControllerB.playerClientId].playerNotes.Add(Config.Instance.team1Name.Value + " Team");
                            if (Plugin.redScore >= Plugin.blueScore) {
                                StartOfRound.Instance.gameStats.allPlayerStats[playerControllerB.playerClientId].playerNotes.Add("Won the game!");
                            } else {
                                StartOfRound.Instance.gameStats.allPlayerStats[playerControllerB.playerClientId].playerNotes.Add("Lost the game..");
                            }
                        }
                        if (Plugin.teams.Get(playerControllerB) == 1) {
                            StartOfRound.Instance.gameStats.allPlayerStats[playerControllerB.playerClientId].playerNotes.Add(Config.Instance.team2Name.Value + " Team");
                            if (Plugin.blueScore >= Plugin.redScore) {
                                StartOfRound.Instance.gameStats.allPlayerStats[playerControllerB.playerClientId].playerNotes.Add("Won the game!");
                            } else {
                                StartOfRound.Instance.gameStats.allPlayerStats[playerControllerB.playerClientId].playerNotes.Add("Lost the game..");
                            }
                        }
                    }
                }
            }
        }
        

        [HarmonyPatch(typeof(StartOfRound), "StartGame")]
        [HarmonyPostfix]
        static void gammaPatch(StartOfRound __instance) {
            int curB = 0;
            int curO = 0;
            int a = 0;
            if (__instance.IsServer || __instance.IsHost) {
                PlayerControllerB[] shuffled = (PlayerControllerB[])RoundManager.Instance.playersManager.allPlayerScripts.Clone();
                Randomizer.Randomize<PlayerControllerB>(shuffled);
                foreach (PlayerControllerB pcb in shuffled) {
                    if (pcb.currentSuitID == Config.Instance.team1Suit.Value) {
                            curO++;

                        }
                        if (pcb.currentSuitID == Config.Instance.team2Suit.Value) {
                            curB++;
                        }
                }
                foreach (PlayerControllerB pcb in shuffled) {
                    if (pcb.isPlayerControlled) {
                        if (pcb.currentSuitID != Config.Instance.team1Suit.Value && pcb.currentSuitID != Config.Instance.team2Suit.Value) {
                            if (curO <= curB) {
                                HUDManager.Instance.AddTextToChatOnServer("<color="+Config.Instance.team1ColorCode.Value+">" +pcb.playerUsername + " was put on " + Config.Instance.team1Name.Value + "!");
                                a = 0;
                                curO++;
                            }
                            else {
                                HUDManager.Instance.AddTextToChatOnServer("<color="+Config.Instance.team2ColorCode.Value+">" +pcb.playerUsername + " was put on " + Config.Instance.team2Name.Value + "!");
                                curB++;
                                a = 1;
                            }
                            if (Plugin.teams.ContainsKey(pcb)) {
                                Plugin.teams.Remove(pcb);
                            }
                            Plugin.teams.Add(pcb, a);
                        }
                        if (pcb.currentSuitID == Config.Instance.team1Suit.Value) {
                            HUDManager.Instance.AddTextToChatOnServer("<color=" + Config.Instance.team1ColorCode.Value + ">" +pcb.playerUsername + " was put on " + Config.Instance.team1Name.Value + "!");
                            if (Plugin.teams.ContainsKey(pcb)) {
                                Plugin.teams.Remove(pcb);
                            }
                            Plugin.teams.Add(pcb, 0);
                        }
                        if (pcb.currentSuitID == Config.Instance.team2Suit.Value) {
                            HUDManager.Instance.AddTextToChatOnServer("<color=" + Config.Instance.team2ColorCode.Value + ">" +pcb.playerUsername + " was put on " + Config.Instance.team2Name.Value + "!");
                            if (Plugin.teams.ContainsKey(pcb)) {
                                Plugin.teams.Remove(pcb);
                            }
                            Plugin.teams.Add(pcb, 1);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "EndGameClientRpc")]
        [HarmonyPostfix]
        static void fine(StartOfRound __instance, ref int playerClientId)
        {
            if (playerClientId != -1) {
                    StartOfRound.Instance.localPlayerController.isInElevator = true;
	                StartOfRound.Instance.localPlayerController.isInHangarShipRoom = true;
	                StartOfRound.Instance.localPlayerController.isInsideFactory = false;
                    StartOfRound.Instance.localPlayerController.TeleportPlayer(RoundManagerPatch.GetPlayerSpawnPosition((int)StartOfRound.Instance.localPlayerController.playerClientId, true));
            PlayerControllerB pulled = StartOfRound.Instance.allPlayerScripts[playerClientId];
            if (!Plugin.finedForShip) {
                Plugin.finedForShip = true;
                int num = (int)(TimeOfDay.Instance.normalizedTimeOfDay * (60f * TimeOfDay.Instance.numberOfHours)) + 360;
                int num2 = (int)Mathf.Floor(num / 60);
                if (Plugin.teams.ContainsKey(pulled) && num2 < Config.Instance.graceTime.Value) {
                    if (Plugin.teams.Get(pulled) == 0) {
                        if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost) {
                            HUDManager.Instance.AddTextToChatOnServer("<color=" + Config.Instance.team1ColorCode.Value + ">" + Config.Instance.team1Name.Value  +" <color=red>was fined for leaving the ship early...");
                            HUDManager.Instance.DisplayTip("Fined!", Config.Instance.team1Name.Value + " was fined for leaving!", true);
                        }
                        Plugin.redScore -= Config.Instance.fineAmount.Value;
                        
                    } else {
                        if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost) {
                            HUDManager.Instance.AddTextToChatOnServer("<color=" + Config.Instance.team2ColorCode.Value  + ">" + Config.Instance.team2Name.Value  + " <color=red>was fined for leaving the ship early...");
                            HUDManager.Instance.DisplayTip("Fined!", Config.Instance.team1Name.Value + " was fined for leaving!", true);
                        }
                        Plugin.blueScore -= Config.Instance.fineAmount.Value;
                    }
                }
            }
            }
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