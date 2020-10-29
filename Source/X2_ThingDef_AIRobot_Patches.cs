using AIRobot;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MiscRobotsWorkTabSupport
{

    [HarmonyPatch(typeof(X2_ThingDef_AIRobot), "robotWorkTags", MethodType.Getter)]
    static class X2_ThingDef_AIRobot_robotWorkTags
    {
        [HarmonyPrefix]
        public static bool Prefix(X2_ThingDef_AIRobot __instance, ref WorkTags ___robotWorkTagsInt, ref WorkTags __result)
        {
            if (___robotWorkTagsInt == WorkTags.None && __instance.robotWorkTypes.Count > 0)
            {
                foreach (X2_ThingDef_AIRobot.RobotWorkTypes robotWorkType in __instance.robotWorkTypes)
                {
                    ___robotWorkTagsInt |= robotWorkType.workTypeDef.workTags;
                }

                ___robotWorkTagsInt &= ~(WorkTags.AllWork | WorkTags.Commoner | WorkTags.ManualDumb | WorkTags.ManualSkilled);
                Log.Message($"{__instance.defName}   result {___robotWorkTagsInt}");
            }

            __result = ___robotWorkTagsInt;
            return false;
        }
    }
}
