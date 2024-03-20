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

namespace CompetitiveCompany.patches {

    [HarmonyPatch(typeof(HUDManager))]
    internal class HudManagerPatch {
        public static TextMeshProUGUI _blueTeam;
        public static TextMeshProUGUI _seperator;
        public static TextMeshProUGUI _redTeam;

        [HarmonyPatch(typeof(HUDManager), "Start")]
            [HarmonyPrefix]
            public static void fix(HUDManager __instance) {
                GameObject val = new GameObject("BlueHUD");
			    val.AddComponent<RectTransform>();
			    TextMeshProUGUI obj = val.AddComponent<TextMeshProUGUI>();
			    RectTransform rectTransform = ((TMP_Text)obj).rectTransform;
			    ((Transform)rectTransform).SetParent(((Component)__instance.PTTIcon).transform, false);
			    rectTransform.anchoredPosition = new UnityEngine.Vector2(0f, 2f);
			    ((TMP_Text)obj).font = ((TMP_Text)__instance.controlTipLines[0]).font;
			    ((TMP_Text)obj).fontSize = 16f;
			    ((TMP_Text)obj).text = "1000";
			    ((TMP_Text)obj).color = Color.red;
			    ((TMP_Text)obj).overflowMode = (TextOverflowModes)0;
			    ((Behaviour)obj).enabled = true;
			    _redTeam = obj;
                

                GameObject val2 = new GameObject("SepHUD");
			    val2.AddComponent<RectTransform>();
			    TextMeshProUGUI obj2 = val2.AddComponent<TextMeshProUGUI>();
			    RectTransform rectTransform2 = ((TMP_Text)obj2).rectTransform;
			    ((Transform)rectTransform2).SetParent(((Component)__instance.PTTIcon).transform, false);
			    rectTransform2.anchoredPosition = new UnityEngine.Vector2(0f, 2f);
			    ((TMP_Text)obj2).font = ((TMP_Text)__instance.controlTipLines[0]).font;
			    ((TMP_Text)obj2).fontSize = 16f;
			    ((TMP_Text)obj2).text = "vs";
			    ((TMP_Text)obj2).color = Color.gray;
			    ((TMP_Text)obj2).overflowMode = (TextOverflowModes)0;
			    ((Behaviour)obj2).enabled = true;
			    _seperator = obj2;

                GameObject val3 = new GameObject("RedHUD");
			    val3.AddComponent<RectTransform>();
			    TextMeshProUGUI obj3 = val3.AddComponent<TextMeshProUGUI>();
			    RectTransform rectTransform3 = ((TMP_Text)obj3).rectTransform;
			    ((Transform)rectTransform3).SetParent(((Component)__instance.PTTIcon).transform, false);
			    rectTransform3.anchoredPosition = new UnityEngine.Vector2(0f, 2f);
			    ((TMP_Text)obj3).font = ((TMP_Text)__instance.controlTipLines[0]).font;
			    ((TMP_Text)obj3).fontSize = 16f;
			    ((TMP_Text)obj3).text = "1000";
			    ((TMP_Text)obj3).color = Color.blue;
			    ((TMP_Text)obj3).overflowMode = (TextOverflowModes)0;
			    ((Behaviour)obj3).enabled = true;
			    _blueTeam = obj3;
            }

            [HarmonyPatch(typeof(HUDManager), "DisplayDaysLeft")]
            [HarmonyPrefix]
            static bool teams2()
            {
                if (GameNetworkManager.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.localPlayerController.IsServer)
			    {
				    TimeOfDay.Instance.timeUntilDeadline = 2000f;
			    }
                if (Plugin.blueScore > Plugin.redScore) {
                    HUDManager.Instance.profitQuotaDaysLeftText.text = Config.Instance.team2Name.Value +  " Wins!";
		            HUDManager.Instance.profitQuotaDaysLeftText2.text = Config.Instance.team2Name.Value + " Wins!";
                }
                if (Plugin.blueScore < Plugin.redScore) {
                    HUDManager.Instance.profitQuotaDaysLeftText.text = Config.Instance.team1Name.Value + " Wins!";
		            HUDManager.Instance.profitQuotaDaysLeftText2.text = Config.Instance.team1Name.Value + " Wins!";
                }
                if (Plugin.blueScore == Plugin.redScore) {
                    HUDManager.Instance.profitQuotaDaysLeftText.text = "Tie!";
		            HUDManager.Instance.profitQuotaDaysLeftText2.text = "Tie!";
                }
                if (GameNetworkManager.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.localPlayerController.IsServer)
			    {
                    RoundManager.Instance.DespawnPropsAtEndOfRound(true);
                }
                HUDManager.Instance.reachedProfitQuotaAnimator.SetTrigger("displayDaysLeftCalm");
                HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.profitQuotaDaysLeftCalmSFX);
                return false;
            }
            [HarmonyPatch(typeof(HUDManager), "FillEndGameStats")]
            [HarmonyPostfix]
            static void teams()
            {
                if (Plugin.blueScore > Plugin.redScore) {
                    HUDManager.Instance.statsUIElements.gradeLetter.text = Config.Instance.team2Name.Value.Substring(0,2);
                    if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost) {
                        HUDManager.Instance.AddTextToChatOnServer("<color=" + Config.Instance.team2ColorCode.Value + ">" + Config.Instance.team2Name.Value + " won the game!!", -1);
                    }
                }
                if (Plugin.blueScore < Plugin.redScore) {
                    HUDManager.Instance.statsUIElements.gradeLetter.text = Config.Instance.team1Name.Value.Substring(0,2);;
                    if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost) {
                        HUDManager.Instance.AddTextToChatOnServer("<color=" + Config.Instance.team1ColorCode.Value +">" + Config.Instance.team1Name.Value + " won the game!!", -1);
                    }
                }
                if (Plugin.blueScore == Plugin.redScore) {
                    HUDManager.Instance.statsUIElements.gradeLetter.text = "T";
                    if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost) {
                        HUDManager.Instance.AddTextToChatOnServer("<color=white>Tied.", -1);
                    }
                }

            }

            [HarmonyPatch(typeof(HUDManager), "AddChatMessage")]
            [HarmonyPrefix]
		    public static bool Prefix(HUDManager __instance, string chatMessage, string nameOfUserWhoTyped = "")
		    {
                if (string.IsNullOrEmpty(nameOfUserWhoTyped)) {
                    if (chatMessage.StartsWith("<color="+ Config.Instance.team1ColorCode.Value +">") && chatMessage.EndsWith(" was put on "+ Config.Instance.team1Name.Value +"!"))
			        {
			    	    string parser = (string)chatMessage.Clone();
                        parser = parser.Remove(0,8+Config.Instance.team1ColorCode.Value.Length);
                        String[] strings = parser.Split(" was");
                        Plugin.Instance.mls.LogInfo("Detected Player " + strings[0]);
                        foreach (PlayerControllerB pcb in RoundManager.Instance.playersManager.allPlayerScripts) {
                            if (strings[0].Equals(pcb.playerUsername) && !Plugin.teams.ContainsKey(pcb)) {
                                Plugin.teams.Add(pcb,0);
                            }
                        }
			        }
                    if (chatMessage.Equals("<color=green>Use the suit rack to switch team or\nPut on the Yellow suit to be randomly selected!")) {
                        HUDManager.Instance.DisplayTip("Competitive Company", "Use the suit rack to switch team or\nPut on the Yellow suit to be randomly selected!");
                        return false;
                    }
                    if (chatMessage.StartsWith("<color="+ Config.Instance.team2ColorCode.Value +">") && chatMessage.EndsWith(" was put on "+ Config.Instance.team2Name.Value +"!"))
			        {
			    	    string parser = (string)chatMessage.Clone();
                        parser = parser.Remove(0,8+Config.Instance.team2ColorCode.Value.Length);
                        String[] strings = parser.Split(" was");
                        Plugin.Instance.mls.LogInfo("Detected Player " + strings[0]);
                        foreach (PlayerControllerB pcb in RoundManager.Instance.playersManager.allPlayerScripts) {
                            if (strings[0].Equals(pcb.playerUsername)&& !Plugin.teams.ContainsKey(pcb)) {
                                Plugin.teams.Add(pcb,1);
                            }
                        }
			        }
			        if (chatMessage.StartsWith("<color="+ Config.Instance.team1ColorCode.Value +">" + GameNetworkManager.Instance.localPlayerController.playerUsername + " was put on "+ Config.Instance.team1Name.Value +"!"))
			        {
			    	    __instance.DisplayTip("Competitive Company", "You're on "+ Config.Instance.team1Name.Value +"!");
			        }
                    if (chatMessage.StartsWith("<color="+ Config.Instance.team2ColorCode.Value +">" + GameNetworkManager.Instance.localPlayerController.playerUsername + " was put on "+ Config.Instance.team2Name.Value +"!"))
			        {
			    	    __instance.DisplayTip("Competitive Company", "You're on "+ Config.Instance.team2Name.Value +"!");
			        }
                }
			    return true;
		    }
	    }
    }