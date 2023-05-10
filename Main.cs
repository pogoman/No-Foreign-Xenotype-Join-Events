using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace No_Foreign_Xenotype_Wanderer_Join_Events
{
    public class Main : Mod
    {
        [HarmonyPatch(typeof(QuestNode_Root_WandererJoin_WalkIn), "GeneratePawn")]
        static class QuestNode_Root_WandererJoin_WalkIn_GeneratePawn
        {
            public static bool Prefix()
            {
                Slate slate = QuestGen.slate;
                Gender? fixedGender = null;
                PawnGenerationRequest request;
                if (!slate.TryGet("overridePawnGenParams", out request, false))
                {
                    request = new PawnGenerationRequest(PawnKindDefOf.Villager, null, PawnGenerationContext.NonPlayer, -1, true, false, false, true, false, 
                        20f, false, true, true, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, 
                        fixedGender, null, null, null, null, false, false, false, false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, true);
                }

                Map map = Find.CurrentMap;
                var playerPawn = map.PlayerPawnsForStoryteller.Where(x => x.IsColonist && x.RaceProps.Humanlike).RandomElement();
                if (playerPawn.genes?.CustomXenotype != null)
                {
                    request.ForcedCustomXenotype = playerPawn.genes.CustomXenotype;
                }
                else if (playerPawn.genes?.xenotype != null)
                {
                    request.ForcedXenotype = playerPawn.genes.xenotype;
                }

                slate.Set("overridePawnGenParams", request);

                return true;
            }
        }

        public Main(ModContentPack contentPack) : base(contentPack)
        {
            var harm = new Harmony("pogo.nfxwje");
            var methodBase = typeof(QuestNode_Root_WandererJoin_WalkIn).GetMethod("GeneratePawn");
            var harmMethod = new HarmonyMethod(typeof(QuestNode_Root_WandererJoin_WalkIn_GeneratePawn).GetMethod("Prefix"));
            harm.Patch(methodBase, harmMethod);
        }
    }
}