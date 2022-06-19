using AIRobot;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MiscRobotsWorkTabSupport
{
    [HarmonyPatch(typeof(X2_AIRobot_Pawn_WorkSettings), "SetPriority")]
    public static class X2_AIRobot_WorkSettings_SetPriority
    {
        [HarmonyPrefix]
        public static bool Prefix(X2_AIRobot_Pawn_WorkSettings __instance, WorkTypeDef w, int priority)
        {
            (__instance as Pawn_WorkSettings).SetPriority(w, priority);
            return false;
        }
    }

    //[HarmonyPatch(typeof(X2_AIRobot_Pawn_WorkSettings), "EnableAndInitialize")]
    //public static class X2_AIRobot_WorkSettings_EnableAndInitialize
    //{
    //    [HarmonyPrefix]
    //    public static bool Prefix(X2_AIRobot_Pawn_WorkSettings __instance, DefMap<WorkTypeDef, int> ___priorities)
    //    {
    //        if (___priorities != null)
    //            (__instance as Pawn_WorkSettings).EnableAndInitialize();
    //        return false;
    //    }
    //}
}
