using AIRobot;
using HarmonyLib;
using PrisonLabor.CompatibilityPatches;
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

            try
            {
                ((Action)(() =>
                {
                    harmony.Patch(typeof(MainTabWindow_WorkTabMod_Tweak).GetConstructor(Type.EmptyTypes), new HarmonyMethod(typeof(PrisonLabor_WorkTab_Patches), "Prefix"));
                }))();
            }
            catch { }
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
        }
    }

    public class StartupDebug : GameComponent
    {
        public StartupDebug(Game game)
        {
        }

        public override void LoadedGame()
        {
            Log.Message($"Enabled workTypeDefs: {DefDatabase<WorkTypeDef>.AllDefs.Join(a => a.defName)}");
        }
    }
}
