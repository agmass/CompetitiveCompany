
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

    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatch {
    }
}