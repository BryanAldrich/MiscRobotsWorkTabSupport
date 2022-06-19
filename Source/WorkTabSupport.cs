using AIRobot;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using UnityEngine.UIElements;
using WhatTheHack;

namespace MiscRobotsWorkTabSupport
{
    public class MainTabWindow_WorkTabMod_Tabs : MainTabWindow
    {
        private const int TopMargin = 12;

        private MainTabWindow_PawnTable pawnTab;

        protected IEnumerable<Pawn> colonists => PlayerPawnsDisplayOrderUtility.InOrder(Find.CurrentMap.mapPawns.FreeColonists);
        
        protected IEnumerable<Pawn> robots
        {
            get
            {
                foreach (X2_Building_AIRobotRechargeStation item in Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<X2_Building_AIRobotRechargeStation>())
                {
                    if (item != null && item.Spawned && !item.Destroyed && item.GetRobot != null)
                    {
                        yield return item.GetRobot;
                    }
                }
            }
        }
        protected IEnumerable<Pawn> animals
        {
            get
            {
                return Find.CurrentMap.mapPawns.AllPawns.Where(p => p.training != null && p.Faction == Faction.OfPlayer && p.skills != null);
            }
        }

        //protected IEnumerable<Pawn> mechanoidsVFE
        //{
        //    get
        //    {
        //        return Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(p => p.def.defName == "MAO_Mechanoids_Mender" || p.def.defName == "MAO_Mechanoids_Assembler");
        //    }
        //}

        public static Type InnerTabType { get; set; }

        public const int ColonistsTabIndex = 0;
        public int RobotsTabIndex = -1;
        public int AnimalsTabIndex = -1;
        //public int MechanoidsTabIndex = -1;

        private int currentTabIndex = 0;
        private int lastTabIndex = 0;

        public MainTabWindow_WorkTabMod_Tabs()
        {
            Type workTab = typeof(MainTabWindow_Work);
            try
            {
                ((Action)(() =>
                {
                    workTab = typeof(WorkTab.MainTabWindow_WorkTab);
                }))();
            }
            catch (TypeLoadException) { }
            catch (Exception e) { Log.Error($"MiscRobotsWorkTabSupport: Error when trying to load Fluffy's WorkTab - {AllErrorMessages(e)}"); }

            pawnTab = Activator.CreateInstance(workTab) as MainTabWindow_PawnTable;
        }

        public override void DoWindowContents(Rect rect)
        {
            string[] tabs;
            {
                List<string> tabList = new List<string>();
                int curTab = ColonistsTabIndex + 1;
                tabList.Add("MRWTS_ColonistsOnlyShort".Translate());
                
                if (robots.Any())
                {
                    tabList.Add("MRWTS_AIRobotsOnlyShort".Translate());
                    RobotsTabIndex = curTab++;
                }
                else
                    RobotsTabIndex = -1;

                if (animals.Any())
                {
                    tabList.Add("Animals".Translate());
                    AnimalsTabIndex = curTab++;
                }
                else
                    AnimalsTabIndex = -1;

                //if (mechanoidsVFE.Any())
                //{
                //    tabList.Add("VFE Mechanoids".Translate());
                //    MechanoidsTabIndex = curTab++;
                //}
                //else
                //    MechanoidsTabIndex = -1;

                tabs = tabList.ToArray();

                if (currentTabIndex >= tabs.Length)
                    currentTabIndex = tabs.Length - 1;
            }

            Text.Font = GameFont.Small;
            UIWidgets.BeginTabbedView(rect, tabs, ref currentTabIndex);
            rect.height -= UIWidgets.HorizontalSpacing - TopMargin;
            GUI.BeginGroup(new Rect(0, TopMargin, rect.width, rect.height));
            if (currentTabIndex != lastTabIndex)
            {
                CreatePawnTable();
                lastTabIndex = currentTabIndex;

                var setDirtyMethod = typeof(MainTabWindow_PawnTable).GetMethod("SetDirty", BindingFlags.Instance | BindingFlags.NonPublic);
                setDirtyMethod.Invoke(pawnTab, new object[] { });
            }
            pawnTab.DoWindowContents(rect);

            GUI.EndGroup();
            UIWidgets.EndTabbedView();
        }

        private bool IsTableNull()
        {
            var tableField = typeof(MainTabWindow_PawnTable).GetField("table", BindingFlags.Instance | BindingFlags.NonPublic);
            var colonistTable = tableField.GetValue(pawnTab);
            return colonistTable == null;
        }

        private void CreatePawnTable()
        {
            var tableField = typeof(MainTabWindow_PawnTable).GetField("table", BindingFlags.Instance | BindingFlags.NonPublic);

            if (currentTabIndex == ColonistsTabIndex)
                tableField.SetValue(pawnTab, CreateTable(pawnTab, colonists));
            else if (currentTabIndex == RobotsTabIndex)
                tableField.SetValue(pawnTab, CreateTable(pawnTab, robots));
            else if (currentTabIndex == AnimalsTabIndex)
                tableField.SetValue(pawnTab, CreateTable(pawnTab, animals));
            //else if (currentTabIndex == MechanoidsTabIndex)
            //    tableField.SetValue(pawnTab, CreateTable(pawnTab, mechanoidsVFE ));
        }

        private PawnTable CreateTable(MainTabWindow_PawnTable pawnTable, IEnumerable<Pawn> pawnsFunc)
        {
            var tableDef = pawnTable.GetType().GetProperty("PawnTableDef", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pawnTable, null) as PawnTableDef;
            var bottomSpace = (float)pawnTable.GetType().GetProperty("ExtraBottomSpace", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pawnTable, null);
            var topSpace = (float)pawnTable.GetType().GetProperty("ExtraTopSpace", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pawnTable, null);
            
            return new PawnTable(tableDef, () => pawnsFunc, UI.screenWidth - (int)(this.Margin * 2f), (int)(UI.screenHeight - 35 - bottomSpace - topSpace - this.Margin * 2f));
        }

        public override void Notify_ResolutionChanged()
        {
            CreatePawnTable();
            base.Notify_ResolutionChanged();
        }

        public override void PreOpen()
        {
            base.PreOpen();

            if (IsTableNull())
            {
                CreatePawnTable();
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();

            var setDirtyMethod = typeof(MainTabWindow_PawnTable).GetMethod("SetDirty", BindingFlags.Instance | BindingFlags.NonPublic);
            setDirtyMethod.Invoke(pawnTab, new object[] { });

            Find.World.renderer.wantedMode = WorldRenderMode.None;
        }

        public override Vector2 RequestedTabSize
        {
            get
            {
                var cSize = pawnTab.RequestedTabSize;
                return new Vector2(Math.Max(cSize.x, 0), Math.Max(cSize.y, 0) + TopMargin + UIWidgets.TabHeight);
            }
        }

        protected override float Margin
        {
            get
            {
                return 6f;
            }
        }

        static string AllErrorMessages(Exception e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Message);

            while (e.InnerException != null)
            {
                e = e.InnerException;
                sb.AppendLine(e.Message);
            }

            return sb.ToString();
        }
    }
}
