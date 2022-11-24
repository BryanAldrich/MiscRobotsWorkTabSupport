using AIRobot;
using RimWorld;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MiscRobotsWorkTabSupport
{
	[HarmonyPatch(typeof(X2_Building_AIRobotCreator), "CreateRobot", new Type[] { typeof(string), typeof(IntVec3), typeof(Map), typeof(Faction) })]
    class X2_Building_AIRobotCreator_CreateRobot2
    {
		[HarmonyPrefix]
        static bool Prefix(string pawnDefName, IntVec3 position, Map map, Faction faction, ref X2_AIRobot __result)
		{
			PawnKindDef named = DefDatabase<PawnKindDef>.GetNamed(pawnDefName);
			PawnGenerationRequest request = new PawnGenerationRequest(named
				, faction: faction
				, context: PawnGenerationContext.NonPlayer
				, forceGenerateNewPawn: true
				, canGeneratePawnRelations: false
				, colonistRelationChanceFactor: 0f
				, allowGay: false
				, allowAddictions: false
				, relationWithExtraPawnChanceFactor: 0f
				, fixedBiologicalAge: 0f
				, fixedChronologicalAge: 0f
				, fixedGender: Gender.None
				);

			X2_AIRobot x2_AIRobot = (X2_AIRobot)PawnGenerator.GeneratePawn(request);
			if (x2_AIRobot.inventory == null)
				x2_AIRobot.inventory = new Pawn_InventoryTracker(x2_AIRobot);

			if (x2_AIRobot.equipment == null)
				x2_AIRobot.equipment = new Pawn_EquipmentTracker(x2_AIRobot);

			if (x2_AIRobot.apparel == null)
				x2_AIRobot.apparel = new Pawn_ApparelTracker(x2_AIRobot);

			x2_AIRobot.workSettings = new Pawn_WorkSettings(x2_AIRobot);
			
			if (x2_AIRobot.story == null)
				x2_AIRobot.story = new Pawn_StoryTracker(x2_AIRobot);

			__result = (X2_AIRobot)X2_Building_AIRobotCreator.Spawn(x2_AIRobot, position, map);

			x2_AIRobot.story.traits.GainTrait(new Trait(DefOfs.AIRobot_BaseTrait, 1, true));

			if (x2_AIRobot.workSettings == null || x2_AIRobot.workSettings is X2_AIRobot_Pawn_WorkSettings)
				x2_AIRobot.workSettings = new Pawn_WorkSettings(x2_AIRobot);
			x2_AIRobot.workSettings.EnableAndInitializeIfNotAlreadyInitialized();

			return false;
		}
    }
}
