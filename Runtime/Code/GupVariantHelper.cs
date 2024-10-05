using System.Collections.Generic;
using EntityStates.Gup;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using VAPI.Components;
using System.Collections.ObjectModel;

namespace VAPI
{
    public static class GupVariantHelper
    {
        private static List<VariantDef> _blacklistedVariants = new List<VariantDef>();

        private static Dictionary<VariantDef, VariantDef> _gupToGeep = new Dictionary<VariantDef, VariantDef>(new VariantDefIndexComparer());
        private static Dictionary<VariantDef, VariantDef> _geepToGip = new Dictionary<VariantDef, VariantDef>(new VariantDefIndexComparer());

        public static void AddToBlacklist(VariantDef variantToBlacklist)
        {
            _blacklistedVariants.Add(variantToBlacklist);
        }

        public static void AddGupProgression(VariantDef gupVariant, VariantDef geepVariant, VariantDef gipVariant)
        {
            _gupToGeep.Add(gupVariant, geepVariant);
            _geepToGip.Add(geepVariant, gipVariant);
        }

        internal static void HandleDeathState(ILContext context)
        {
            var cursor = new ILCursor(context);

            var success = cursor.TryGotoNext(x => x.MatchDup(),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<BaseSplitDeath>(nameof(BaseSplitDeath.moneyMultiplier)),
                x => x.MatchStfld<BodySplitter>(nameof(BodySplitter.moneyMultiplier)));

            if (!success)
            {
                VAPILog.Fatal("Failed to reach specific destination for handling Gup's death states!");
                IL.EntityStates.Gup.BaseSplitDeath.FixedUpdate -= HandleDeathState;
                return;
            }

            cursor.Emit(OpCodes.Dup);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<BodySplitter, BaseSplitDeath>>(HandleDeath);

            void HandleDeath(BodySplitter splitter, BaseSplitDeath baseSplitDeath)
            {
                if (!baseSplitDeath.characterBody)
                    return;

                if (!baseSplitDeath.characterBody.TryGetComponent<BodyVariantManager>(out var manager))
                    return;

                var orig = splitter.masterSummon;
                var newSummon = new VariantSummon
                {
                    masterPrefab = orig.masterPrefab,
                    ignoreTeamMemberLimit = false,
                    useAmbientLevel = null,
                    teamIndexOverride = null
                };
                newSummon.variantDefs = FilterVariants(manager.variantsInBody);
                splitter.masterSummon = newSummon;
            }
        }

        private static VariantDef[] FilterVariants(ReadOnlyCollection<VariantDef> variantDefs)
        {
            List<VariantDef> filtered = new List<VariantDef>();
            foreach(var variantDef in variantDefs)
            {
                if (_blacklistedVariants.Contains(variantDef))
                    continue;

                if (_gupToGeep.TryGetValue(variantDef, out var chosen))
                {
                    filtered.Add(chosen);
                    continue;
                }

                if (_geepToGip.TryGetValue(variantDef, out chosen))
                {
                    filtered.Add(chosen);
                }
                continue;
            }
            return filtered.ToArray();
        }
    }
}