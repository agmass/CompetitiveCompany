using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements.Collections;

namespace CompetitiveCompany.patches {

    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch {

        [HarmonyPatch(typeof(RoundManager), "OnDestroy")]
        [HarmonyPostfix]
        static void undo(RoundManager __instance) {
            Plugin.startLogic = false;
            Plugin.teams.Clear();
        }

        [HarmonyPatch(typeof(RoundManager), "Update")]
        [HarmonyPostfix]
        static void checkIfDead(RoundManager __instance) {
            if (GameNetworkManager.Instance.localPlayerController == null) {
                return;
            }
            if (!StartOfRound.Instance.unlockablesList.unlockables[2].alreadyUnlocked) {
                if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost) {
                    if (Config.Instance.team2Suit.Value == 0 || Config.Instance.team1Suit.Value == 1 || Config.Instance.team1Suit.Value == 24 || Config.Instance.team1Suit.Value == 3)
                        UnlockShipItem(StartOfRound.Instance, Config.Instance.team1Suit.Value, "Pajama Suit");
                    if (Config.Instance.team2Suit.Value == 0 || Config.Instance.team2Suit.Value == 1 || Config.Instance.team2Suit.Value == 24 || Config.Instance.team2Suit.Value == 3)
                        UnlockShipItem(StartOfRound.Instance, Config.Instance.team2Suit.Value, "Pajama Suit");
                    UnlockShipItem(StartOfRound.Instance, 2, "Hazard Suit");
                }
            }
            if (Plugin.startLogic) {
                if (StartOfRound.Instance.shipHasLanded) {
                    if (Plugin.curHour >= Plugin.nextHour) {
                        
                    }
                int orange = 0;
                int blue = 0;
                foreach (PlayerControllerB playerControllerB in RoundManager.Instance.playersManager.allPlayerScripts) {
                    if (Plugin.teams.ContainsKey(playerControllerB) && !playerControllerB.isPlayerDead && playerControllerB.isPlayerControlled) {
                        if (Plugin.teams.Get(playerControllerB) == 0) {
                            orange++;
                        }
                        if (Plugin.teams.Get(playerControllerB) == 1) {
                            blue++;
                        }
                }
                }
                if (!Config.Instance.ALLREQUIRED_respawning.Value) {
                if (orange == 0) {
                    Plugin.startLogic = false;
                    Plugin.redScore = -400;
                    if (GameNetworkManager.Instance.isHostingGame) {
                        HUDManager.Instance.AddTextToChatOnServer("<color="+ Config.Instance.team1ColorCode.Value +">"+ Config.Instance.team1Name.Value + " was team-wiped...");
                        StartOfRound.Instance.EndGameServerRpc(-1);
                    }
                }
                if (blue == 0) {
                    Plugin.startLogic = false;
                    Plugin.blueScore = -400;
                    if (GameNetworkManager.Instance.isHostingGame) {
                        HUDManager.Instance.AddTextToChatOnServer("<color=" + Config.Instance.team2ColorCode.Value + ">" + Config.Instance.team2Name.Value + " was team-wiped...");
                        StartOfRound.Instance.EndGameServerRpc(-1);
                    }
                }
                }
                }
                HudManagerPatch._blueTeam.text = Plugin.blueScore.ToString();
                HudManagerPatch._seperator.text = "vs";
                HudManagerPatch._redTeam.text = Plugin.redScore.ToString();
                HudManagerPatch._seperator.rectTransform.anchoredPosition = new UnityEngine.Vector2((HudManagerPatch._redTeam.text.Length+1)*8, 2f);
                HudManagerPatch._blueTeam.rectTransform.anchoredPosition = new UnityEngine.Vector2((HudManagerPatch._redTeam.text.Length+1+HudManagerPatch._seperator.text.Length+1)*8, 2f);
                } else {
                    HudManagerPatch._redTeam.text = "";
                    HudManagerPatch._blueTeam.text = "";
                    HudManagerPatch._seperator.text = "";
                }
            }
        
        [HarmonyPatch(typeof(RoundManager), "Start")] 
        [HarmonyPostfix]
        static void getthatshit(RoundManager __instance) {
            Plugin.startLogic = false;
            if (GameNetworkManager.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.localPlayerController.IsServer)
			    {
				    TimeOfDay.Instance.timeUntilDeadline = 2000f;
			    }
        }

        [HarmonyPatch(typeof(RoundManager), "GenerateNewLevelClientRpc")]
        [HarmonyPostfix]
        static void gammaPatch(RoundManager __instance) {
            Plugin.warnship = true;
            Plugin.blueScore = 0;
            Plugin.startLogic = true;
            Plugin.redScore = 0;
        }

        [HarmonyPatch(typeof(StartMatchLever), "LeverAnimation")]
        [HarmonyPrefix]
        static bool noLeave(StartMatchLever __instance) {
            int num = (int)(TimeOfDay.Instance.normalizedTimeOfDay * (60f * TimeOfDay.Instance.numberOfHours)) + 360;
            int num2 = (int)Mathf.Floor(num / 60);
            if (num2 < 17 && Plugin.warnship && RoundManager.Instance.playersManager.shipHasLanded) {
                HUDManager.Instance.DisplayTip("Competitive Company", "You can't leave this early!\nIf you want to leave anyways, You will be fined 200$ and the ship will leave.", true);
                Plugin.warnship = false;
                return false;
            }
            return true;
        }

        private static void UnlockShipItem(StartOfRound instance, int unlockableID, string name)
		{
			try
			{
				MethodInfo method = (instance).GetType().GetMethod("UnlockShipObject", BindingFlags.Instance | BindingFlags.NonPublic);
				method.Invoke(instance, new object[1] { unlockableID });
			}
			catch (NullReferenceException arg)
			{
				Plugin.Instance.mls.LogError($"Could not invoke UnlockShipObject method: {arg}");
			}
		}

    static public UnityEngine.Vector3 GetPlayerSpawnPosition(int playerNum, bool simpleTeleport = false)
    {
        Transform[] playerSpawnPositions = StartOfRound.Instance.playerSpawnPositions;
	if (simpleTeleport)
	{
		return playerSpawnPositions[0].position;
	}
	Debug.DrawRay(playerSpawnPositions[playerNum].position, UnityEngine.Vector3.up, Color.red, 15f);
	if (!Physics.CheckSphere(playerSpawnPositions[playerNum].position, 0.2f, 67108864, QueryTriggerInteraction.Ignore))
	{
		return playerSpawnPositions[playerNum].position;
	}
	if (!Physics.CheckSphere(playerSpawnPositions[playerNum].position + UnityEngine.Vector3.up, 0.2f, 67108864, QueryTriggerInteraction.Ignore))
	{
		return playerSpawnPositions[playerNum].position + UnityEngine.Vector3.up * 0.5f;
	}
	for (int i = 0; i < playerSpawnPositions.Length; i++)
	{
		if (i != playerNum)
		{
			Debug.DrawRay(playerSpawnPositions[i].position, UnityEngine.Vector3.up, Color.green, 15f);
			if (!Physics.CheckSphere(playerSpawnPositions[i].position, 0.12f, -67108865, QueryTriggerInteraction.Ignore))
			{
				return playerSpawnPositions[i].position;
			}
			if (!Physics.CheckSphere(playerSpawnPositions[i].position + UnityEngine.Vector3.up, 0.12f, 67108864, QueryTriggerInteraction.Ignore))
			{
				return playerSpawnPositions[i].position + UnityEngine.Vector3.up * 0.5f;
			}
		}
	}
	System.Random random = new System.Random(65);
	float y = playerSpawnPositions[0].position.y;
	for (int j = 0; j < 15; j++)
	{
		UnityEngine.Vector3 vector = new UnityEngine.Vector3(random.Next((int)StartOfRound.Instance.shipInnerRoomBounds.bounds.min.x, (int)StartOfRound.Instance.shipInnerRoomBounds.bounds.max.x), y, random.Next((int)StartOfRound.Instance.shipInnerRoomBounds.bounds.min.z, (int)StartOfRound.Instance.shipInnerRoomBounds.bounds.max.z));
		vector = StartOfRound.Instance.shipInnerRoomBounds.transform.InverseTransformPoint(vector);
		Debug.DrawRay(vector, UnityEngine.Vector3.up, Color.yellow, 15f);
		if (!Physics.CheckSphere(vector, 0.12f, 67108864, QueryTriggerInteraction.Ignore))
		{
			return playerSpawnPositions[j].position;
		}
	}
	return playerSpawnPositions[0].position + UnityEngine.Vector3.up * 0.5f;
}
        
    }
}