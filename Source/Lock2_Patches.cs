using AIRobot;
using HarmonyLib;
using Locks2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MiscRobotsWorkTabSupport
{
    static class Lock2_Pawns_Patches
    {
        
        //static bool Prefix(ref IEnumerable<Pawn> __result, ITab_Lock __instance, Building __door)
        //{
        //    List<Pawn> pawns = __door.Map.mapPawns.FreeColonists;
        //    pawns.AddRange(__door.Map.mapPawns.PrisonersOfColony.AsEnumerable());

        //    pawns.AddRange(__door.Map.listerBuildings.AllBuildingsColonistOfClass<X2_Building_AIRobotRechargeStation>()
        //        .Where(a => a != null && a.Spawned && !a.Destroyed && a.GetRobot != null)
        //        .Select(a => a.GetRobot));

        //    __result = new HashSet<Pawn>(pawns).AsEnumerable();

        //    return false;
        //}
    }
}
