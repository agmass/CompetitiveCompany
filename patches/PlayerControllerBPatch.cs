using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements.Collections;

namespace CompetitiveCompany.patches {

    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch {
        
        [HarmonyPatch(typeof(PlayerControllerB), "SetItemInElevator")]
        [HarmonyPrefix]
        static void gammaPatch2(PlayerControllerB __instance, ref GrabbableObject gObject, ref bool droppedInShipRoom) {
            
                if (gObject.itemProperties.isScrap && !gObject.scrapPersistedThroughRounds && droppedInShipRoom && !RoundManager.Instance.scrapCollectedThisRound.Contains(gObject) ) {
                    if (Plugin.teams.ContainsKey(__instance)) {
                        if (Plugin.teams.Get(__instance) == 0) {
                            Plugin.redScore += gObject.scrapValue;
                            if (GameNetworkManager.Instance.isHostingGame) {
                                Terminal te = UnityEngine.Object.FindObjectOfType<Terminal>();
                                te.groupCredits += gObject.scrapValue;
                                te.SyncGroupCreditsServerRpc(te.groupCredits, te.numberOfItemsInDropship);
                                HUDManager.Instance.AddTextToChatOnServer("<color="+ Config.Instance.team1ColorCode.Value + ">+" + gObject.scrapValue + "\n<color=white>(<color=red>" + Plugin.redScore + "<color=white> - <color=blue>" + Plugin.blueScore + "<color=white>)", -1);
                            }
                        }
                        if (Plugin.teams.Get(__instance) == 1) {
                            Plugin.blueScore += gObject.scrapValue;
                            if (GameNetworkManager.Instance.isHostingGame) {
                                Terminal te = UnityEngine.Object.FindObjectOfType<Terminal>();
                                te.groupCredits += gObject.scrapValue;
                                te.SyncGroupCreditsServerRpc(te.groupCredits, te.numberOfItemsInDropship);
                                HUDManager.Instance.AddTextToChatOnServer("<color="+ Config.Instance.team2ColorCode.Value + ">+" + gObject.scrapValue + "\n<color=white>(<color=red>" + Plugin.redScore + "<color=white> - <color=blue>" + Plugin.blueScore + "<color=white>)", -1);
                            }
                        }
                    }
                }
        
        }


        

        [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayerFromOtherClientServerRpc")]
        [HarmonyPrefix]
        static void ff(PlayerControllerB __instance, ref int damageAmount, ref int playerWhoHit) {
            int num = (int)(TimeOfDay.Instance.normalizedTimeOfDay * (60f * TimeOfDay.Instance.numberOfHours)) + 360;
            int num2 = (int)Mathf.Floor(num / 60);
            if (num2 < Config.Instance.graceTime.Value) {
                damageAmount = 0;
                if (RoundManager.Instance.playersManager.allPlayerScripts[playerWhoHit].Equals(GameNetworkManager.Instance.localPlayerController)) {
                    int a = 0;
                    string b = "am";
                    a += Config.Instance.graceTime.Value;
                    if (a > 12) {
                        a -= 12;
                        b = "pm";
                    }
                    HUDManager.Instance.DisplayTip("Competitive Company","You can only attack others in the factory or after "+ a + b + "!", true);
                }
            }
        }

        static IEnumerator SuitSetter(PlayerControllerB b) {
            yield return new WaitForSeconds(2f);
            UnlockableSuit[] array = UnityEngine.Object.FindObjectsOfType<UnlockableSuit>(includeInactive: true);;
            foreach (UnlockableSuit us in array) {
                if (us.suitID == 2) {
                        us.SwitchSuitToThis(b);
                    }
                }
            }



        
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        static void gammaPatch(PlayerControllerB __instance) {
            if (__instance != null) {
            if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost) {
                if (!Plugin.initiated.ContainsKey(__instance)) {
                    Plugin.initiated.Add(__instance, true);
                    HUDManager.Instance.AddTextToChatOnServer("<color=green>Use the suit rack to switch team or\nPut on the Yellow suit to be randomly selected!", -1);
                    __instance.StartCoroutine(SuitSetter(__instance));
                }
                if (Plugin.teams.ContainsKey(__instance) && Plugin.startLogic && RoundManager.Instance.dungeonFinishedGeneratingForAllPlayers) {
                    if (Plugin.teams.Get(__instance) == 0 && __instance.currentSuitID != Config.Instance.team1Suit.Value) {
                        UnlockableSuit[] array = UnityEngine.Object.FindObjectsOfType<UnlockableSuit>(includeInactive: true);;
                        foreach (UnlockableSuit us in array) {
                            if (us.suitID == Config.Instance.team1Suit.Value) {
                                us.SwitchSuitToThis(__instance);
                            }
                        }
                    }
                    if (Plugin.teams.Get(__instance) == 1 && __instance.currentSuitID != Config.Instance.team2Suit.Value) {
                        UnlockableSuit[] array = UnityEngine.Object.FindObjectsOfType<UnlockableSuit>(includeInactive: true);;
                        foreach (UnlockableSuit us in array) {
                            if (us.suitID == Config.Instance.team2Suit.Value) {
                                us.SwitchSuitToThis(__instance);
                            }
                        }
                    }
                }
            }
            if (__instance.currentSuitID == 0) {
                __instance.usernameBillboardText.color = Color.red;
            }
            if (__instance.currentSuitID == 3) {
                __instance.usernameBillboardText.color = Color.blue;
            }
            if (__instance.currentSuitID == 2) {
                __instance.usernameBillboardText.color = Color.yellow;
            }
        }
        }
    }
}