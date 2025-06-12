using AIRobot;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MiscRobotsWorkTabSupport
{
    [HarmonyPatch(typeof(X2_AIRobot), "GetPriority")]
    static class X2_AIRobot_Priority
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn __instance, ref int __result, X2_ThingDef_AIRobot ___def2, WorkTypeDef workTypeDef)
        {
            if (___def2 == null)
                return true;

            __result = __instance.workSettings.GetPriority(workTypeDef);

            //var robot = __instance as X2_AIRobot;

            //if (__instance.ThingID == "RPP_Bot_Omni_V923240")
            //    Log.Message($@"work priority for {__instance.Name} for {workTypeDef.defName} priority {__result} tags:{___def2.robotWorkTags}--{workTypeDef.workTags} canwork:{robot.CanDoWorkType(workTypeDef)}");
            return false;
        }
    }

    [HarmonyPatch(typeof(X2_AIRobot), "GetWorkGivers")]
    public static class X2_AIRobot_GetWorkGivers
    {
        [HarmonyPrefix]
        static bool Prefix(X2_AIRobot __instance, ref List<WorkGiver> ___workGiversEmergencyCache, ref List<WorkGiver> ___workGiversNonEmergencyCache)
        {
            if (Traverse.Create(__instance.workSettings).Field("workGiversDirty").GetValue<bool>())
            {
                ___workGiversEmergencyCache = null;
                ___workGiversNonEmergencyCache = null;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(X2_AIRobot), "CanDoWorkType")]
    public static class X2_AIRobot_CanDoWorkType
    {
        static readonly Dictionary<(string, string), bool> _cache = [];
        static bool Prefix(X2_AIRobot __instance, WorkTypeDef workTypeDef, ref bool __result)
        {
            var robotThing = __instance.def as X2_ThingDef_AIRobot;

#pragma warning disable CS0219 // Variable is assigned but its value is never used
            int resultMethod = 0;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            bool cached = false;
            try
            {
                if (_cache.TryGetValue((__instance.def.defName, workTypeDef.defName), out bool cachedValue))
                {
                    cached = true;
                    __result = cachedValue;
                    return false;
                }

                var thingDef = __instance.def as X2_ThingDef_AIRobot;

                int numRequiredSkills = workTypeDef.relevantSkills?.Count ?? 0;

                string[] globalDisabledWorkDefs = ["TM_Magic", "Patient", "PatientBedRest", "VBE_Writing", "FSFTraining", "Handling", "Hunting", "PruneGauranlenTree"];
                string[] haulerAllowedDefs = ["Hauling", "HaulingUrgent", "NuclearWork", "FSFHauling", "FSFRearming", "FSFTransport", "FSFLoading", "FSFDeliver"];
                string[] crafterAllowedDefs = ["RimefellerCrafting", "RB_BeekeepingWork", "NuclearWork", "FSFSmelt", "FSFStoneCut", "FSFCremating"];
                string[] builderAllowedDefs = ["NuclearWork"];
                string[] kitchenAllowedDefs = ["BlightsCut"];
                string[] omniOnlyAllowedDefs = ["Research", "WTH_Hack"];
                var omniAllowedDefs = omniOnlyAllowedDefs.Union(haulerAllowedDefs).Union(crafterAllowedDefs).Union(builderAllowedDefs);

#if DEBUG
                Log.Message($"{workTypeDef.defName} - skills: {numRequiredSkills} tags: {workTypeDef.workTags}");
#endif

                if (workTypeDef == WorkTypeDefOf.Firefighter)
                {
                    resultMethod = 1;
                    __result = true;
                }
                else if (thingDef.robotWorkTypes.Any(a => a.workTypeDef == workTypeDef))
                {
                    resultMethod = 2;
                    __result = true;
                }
                else if (globalDisabledWorkDefs.Contains(workTypeDef.defName))
                {
                    resultMethod = 3;
                    __result = false;
                }
                else if (numRequiredSkills > 0 && thingDef.robotSkills?.Any(a => a.level == 0 && workTypeDef.relevantSkills.Any(b => b == a.skillDef)) == true)
                {
                    resultMethod = 4;
                    __result = false;
                }
                else if (!__instance.def.defName.StartsWith("RPP_Bot_Omni_") && omniOnlyAllowedDefs.Contains(workTypeDef.defName))
                {
                    resultMethod = 5;
                    __result = false;
                }
                else if (numRequiredSkills == 0)
                {
                    //hauler or cleaner only here
                    if (workTypeDef == DefOfs.Cleaning)
                    {
                        resultMethod = 6;
                        __result = robotThing.robotWorkTypes.Any(a => a.workTypeDef == DefOfs.Cleaning);
                    }
                    else if (__instance.def.defName.Contains("Hauler"))
                    {
                        resultMethod = 7;
                        __result = (workTypeDef.workTags & WorkTags.Hauling) != 0;
                    }
                    else if (__instance.def.defName.StartsWith("RPP_Bot_Omni_"))
                    {
                        resultMethod = 8;
                        __result = omniAllowedDefs.Contains(workTypeDef.defName) ||
                            DefDatabase<X2_ThingDef_AIRobot>.AllDefs.Any(a => a.robotWorkTypes.Any(b => b.workTypeDef == workTypeDef));
                    }
                    else if (__instance.def.defName.StartsWith("RRP_Bot_Kitchen_"))
                    {
                        resultMethod = 9;
                        __result = kitchenAllowedDefs.Contains(workTypeDef.defName);
                    }
                    else if (__instance.def.defName.StartsWith("RPP_Bot_Crafter_") || __instance.def.defName.StartsWith("AIRobot_CraftingBot"))
                    {
                        resultMethod = 10;
                        __result = crafterAllowedDefs.Contains(workTypeDef.defName);
                    }
                    else if (__instance.def.defName.StartsWith("RPP_Bot_Builder_"))
                    {
                        resultMethod = 11;
                        __result = builderAllowedDefs.Contains(workTypeDef.defName);
                    }
                    else
                    {
                        resultMethod = 12;
                        __result = false;
                    }

                    return false;
                }
                else
                {
                    resultMethod = 13;
                    __result = (robotThing.robotSkills.Select(a => a.skillDef).Intersect(workTypeDef.relevantSkills).Any());
                }

                return false;
            }
            finally
            {
                if (!cached)
                    _cache.Add((__instance.def.defName, workTypeDef.defName), __result);
#if DEBUG
                if (!_debug.ContainsKey((__instance.def.defName, workTypeDef.defName)))
                {
                    Log.Message($"{__instance.def.defName} - {workTypeDef.defName} - CanWork:{__result} - ResultMethod:{resultMethod}");
                    _debug.Add((__instance.def.defName, workTypeDef.defName), __result);
                }
#endif
            }
        }

#if DEBUG
        readonly static Dictionary<(string, string), bool> _debug = [];
#endif
    }

    [HarmonyPatch(typeof(Pawn), "WorkTypeIsDisabled")]
    public static class Pawn_WorkTypeIsDisabled
    {
        [HarmonyPostfix]
        static bool Postfix(bool __result, Pawn __instance, WorkTypeDef w)
        {
            if (__instance is X2_AIRobot x)
            {
                if (__result)
                    return true;

                return !x.CanDoWorkType(w);
            }
            return __result;
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetDisabledWorkTypes")]
    public static class Pawn_GetDisabledWorkTypes
    {
        [HarmonyPostfix]
        static List<WorkTypeDef> Postfix(List<WorkTypeDef> __result, Pawn __instance, bool permanentOnly = false)
        {
            if (__instance is X2_AIRobot x)
            {
                __result = [.. __result.Union(DefDatabase<WorkTypeDef>.AllDefs.Where(a => !x.CanDoWorkType(a)))];
                //Log.Message($"GetDisabledWorkTypes - {x.def.defName} -- {__result.Join(a => a.defName)}");
            }
            return __result;
        }
    }

    //[HarmonyPatch(typeof(X2_AIRobot), "SpawnSetup")]
    public static class X2_AIRobot_SpawnSetup
    {
        //[HarmonyPostfix]
        static void Postfix(Map map, bool respawningAfterLoad)
        {
        }
    }

    [HarmonyPatch(typeof(X2_AIRobot), "InitPawn_Setup")]
    public static class X2_AIRobot_InitPawn_Setup
    {
        [HarmonyPrefix]
        static bool Prefix(X2_AIRobot __instance)
        {
            if (Scribe.mode == LoadSaveMode.Inactive)
            {
                __instance.equipment ??= new Pawn_EquipmentTracker(__instance);

                __instance.apparel ??= new Pawn_ApparelTracker(__instance);

                __instance.skills ??= new Pawn_SkillTracker(__instance);
                typeof(X2_AIRobot).GetMethod("SetSkills", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, [false]);

                __instance.story ??= new Pawn_StoryTracker(__instance);
                __instance.story.traits.GainTrait(new Trait(DefOfs.AIRobot_BaseTrait, 1, true));

                __instance.story.bodyType = BodyTypeDefOf.Male;

                __instance.Drawer.renderer.SetAllGraphicsDirty();

                __instance.relations ??= new Pawn_RelationsTracker(__instance);
                __instance.relations.ClearAllRelations();

                if (!__instance.ignoreSpawnRename)
                    typeof(X2_AIRobot).GetMethod("SetBasename", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, [__instance]);
                __instance.ignoreSpawnRename = false;

                __instance.timetable ??= new Pawn_TimetableTracker(__instance);
                for (int i = 0; i < 24; i++)
                    __instance.timetable.SetAssignment(i, TimeAssignmentDefOf.Work);

                if (__instance.workSettings == null || __instance.workSettings is X2_AIRobot_Pawn_WorkSettings)
                {
                    __instance.workSettings = new Pawn_WorkSettings(__instance);
                    __instance.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
                }

#if DEBUG
                Log.Message(__instance.workSettings.DebugString());
#endif
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(JobGiver_Work), "PawnCanUseWorkGiver")]
    static class JobGiver_Work_PawnCanUseWorkGiver
    {
        [HarmonyPrefix]
        static bool Prefix(Pawn pawn, WorkGiver giver, ref bool __result)
        {
            try
            {
                if (pawn is X2_AIRobot p)
                {
                    if (pawn.WorkTagIsDisabled(giver.def.workTags))
                        __result = false;
                    else if (giver.ShouldSkip(pawn))
                        __result = false;
                    else if (giver.MissingRequiredCapacity(pawn) != null)
                        __result = false;
                    else
                        __result = true;
                    return false;
                }
            }
            finally
            {
#if DEBUG
                Log.Message($"Checking {pawn.def.defName} - {giver.def.defName} -- __result {__result}.");
#endif
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(GenGrid), "Walkable")]
    static class GenGrid_Walkable
    {
        [HarmonyPrefix]
        static bool Prefix(IntVec3 c, Map map, ref bool __result)
        {
            if (map == null)
                Log.WarningOnce($"map is null when Walkable for {c}", c.GetHashCode());
            else if (map?.pathing == null)
                Log.WarningOnce($"map.pathing is null when Walkable for {c}", c.GetHashCode());
            else if (map?.pathing?.Normal == null)
                Log.WarningOnce($"map.pathing.Normal is null when Walkable for {c}", c.GetHashCode());
            else if (map?.pathing?.Normal?.pathGrid == null)
                Log.WarningOnce($"map.pathing.Normal.pathGrid is null when Walkable for {c}", c.GetHashCode());
            else if (map?.pathing?.FenceBlocked?.pathGrid == null)
                Log.WarningOnce($"map.pathing.FenceBlocked.pathGrid is null when Walkable for {c}", c.GetHashCode());
            else
                return true;

            return false;
        }
    }
}
