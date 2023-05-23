using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using UnityEngine;
using Verse;

namespace No_Foreign_Xenotype_Join_Events
{
    public class Settings : ModSettings
    {
        public bool excludeSlaves = true;
        public bool questRewards = true;
        public bool wandererJoins = true;
        public bool transportCrash = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref excludeSlaves, "excludeSlaves", true, true);
            Scribe_Values.Look(ref questRewards, "questRewards", true, true);
            Scribe_Values.Look(ref wandererJoins, "wandererJoins", true, true);
            Scribe_Values.Look(ref transportCrash, "transportCrash", true, true);
        }
    }

    public class Main : Mod
    {
        public static Settings settings;

        [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) })]
        static class PawnGenerator_PawnGenerationRequest
        {
            public static void Prefix(ref PawnGenerationRequest request)
            {
                var methodInfo = new StackTrace().GetFrame(2).GetMethod();
                var methodClass = methodInfo.ReflectedType;
                if ((settings.questRewards && (methodClass == typeof(QuestNode_GeneratePawn) || methodClass == typeof(Reward_Pawn))) ||
                    (settings.wandererJoins && methodClass == typeof(QuestNode_Root_WandererJoin_WalkIn)) ||
                    (settings.transportCrash && methodClass == typeof(ThingSetMaker_RefugeePod)))
                {
                    Map map = Find.CurrentMap;
                    var playerPawn = map.PlayerPawnsForStoryteller.Where(x => x.IsColonist && x.RaceProps.Humanlike && (!x.IsSlave || !settings.excludeSlaves)).RandomElement();
                    if (playerPawn.genes?.CustomXenotype != null)
                    {
                        request.ForcedCustomXenotype = playerPawn.genes.CustomXenotype;
                    }
                    else if (playerPawn.genes?.xenotype != null)
                    {
                        request.ForcedXenotype = playerPawn.genes.xenotype;
                    }
                }
            }
        }

        public Main(ModContentPack contentPack) : base(contentPack)
        {
            var harm = new Harmony("pogo.nfxwje");
            var methodBase = typeof(PawnGenerator).GetMethod("GeneratePawn", new Type[] { typeof(PawnGenerationRequest) });
            var harmMethod = new HarmonyMethod(typeof(PawnGenerator_PawnGenerationRequest).GetMethod("Prefix"));
            settings = GetSettings<Settings>();
            harm.Patch(methodBase, harmMethod);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Include wanderer join events", ref settings.wandererJoins);
            listingStandard.CheckboxLabeled("Include quest reward pawns", ref settings.questRewards);
            listingStandard.CheckboxLabeled("Include transport pod crashes", ref settings.transportCrash);
            listingStandard.CheckboxLabeled("Exclude slave xenotypes from random selection", ref settings.excludeSlaves);
            listingStandard.End();
            settings.Write();
        }

        public override string SettingsCategory()
        {
            return "No Foreign Xenotype Join Events";
        }
    }
}