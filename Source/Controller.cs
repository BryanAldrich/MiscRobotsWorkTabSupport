using AIRobot;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MiscRobotsWorkTabSupport
{
    [StaticConstructorOnStartup]
    public class Controller : Mod
    {
        public Controller(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("Elf.MiscRobotsWorkTabSupport");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [StaticConstructorOnStartup]
    public static class Initializer
    {
        static Initializer()
        {
            var workTab = DefDatabase<MainButtonDef>.GetNamed("Work");
            MainTabWindow_WorkTabMod_Tabs.InnerTabType = workTab.tabWindowClass;
            workTab.tabWindowClass = typeof(MainTabWindow_WorkTabMod_Tabs);

            Log.Message($"Enabled WorkTypeDef's: {DefDatabase<WorkTypeDef>.AllDefs.Select(a => $"{a.defName} - {a.relevantSkills.Select(b => b.defName).Join(delimiter: "|")}").Join()}");
        }
    }

    public class StartupDebug : GameComponent
    {
        public StartupDebug(Game game)
        {
        }

        public override void LoadedGame()
        {
            
        }
    }
}
