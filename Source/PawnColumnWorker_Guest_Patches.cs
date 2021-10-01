using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MiscRobotsWorkTabSupport
{
    [HarmonyPatch(typeof(PawnColumnWorker_Guest), "GetIconFor")]
    static class PawnColumnWorker_Guest_GetIconFor
    {
        [HarmonyPrefix]
        public static bool Prefix(PawnColumnWorker_Guest __instance, ref Texture2D __result, Pawn pawn)
        {
            __result = pawn?.guest?.GetIcon();
            return false;
        }
    }

    [HarmonyPatch(typeof(PawnColumnWorker_Guest), "GetIconTip")]
    static class PawnColumnWorker_Guest_GetIconTip
    {
        [HarmonyPrefix]
        public static bool Prefix(PawnColumnWorker_Guest __instance, ref string __result, Pawn pawn)
        {
            string str = pawn?.guest?.GetLabel();
            if (!str.NullOrEmpty())
            {
                __result = str.CapitalizeFirst();
                return false;
            }
            __result = null;
            return false;
        }
    }
}
