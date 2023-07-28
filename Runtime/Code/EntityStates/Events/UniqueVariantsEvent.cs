using Moonstorm;
using Moonstorm.Config;
using RiskOfOptions.OptionConfigs;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VAPI;
using VAPI.Components;

namespace EntityStates.Events.VariantEvents
{
    public class UniqueVariantsEvent : VariantEventState
    {
        public static ConfigurableFloat globalSpawnRate = VAPIConfig.MakeConfigurableFloat(75f, f =>
        {
            f.ConfigFile = VAPIConfig.generalConfig;
            f.Section = "Events";
            f.Key = $"Event - Unique Variants :: Global Spawn Rate";
            f.Description = "The Global Spawn Rate that's used during the Unique Variant Event";
            f.UseStepSlider = false;
            f.SliderConfig = new SliderConfig
            {
                min = 0,
                max = 100,
                checkIfDisabled = () =>
                {
                    var card = VAPIAssets.LoadAsset<EventCard>("ecUniqueVariants");
                    var cfg = VAPI.Modules.Events.IsCardEnabled(card);
                    return !VAPIConfig.addVariantEvents || !cfg;
                }
            };
        }).DoConfigure();
        public static List<VariantDef> blacklistedVariants = new List<VariantDef>();

        private bool[] uniquenessValues;
        private float[] spawnChanceValues;
        private int variantCount;
        private VariantDef[] variantDefs;
        public override void OnEnter()
        {
            base.OnEnter();
            variantDefs = VariantCatalog.registeredVariants;
            variantCount = variantDefs.Length;
            uniquenessValues = new bool[variantCount];
            spawnChanceValues = new float[variantCount];
        }
        public override void StartEvent()
        {
            base.StartEvent();
            for(int i = 0; i < variantCount; i++)
            {
                var def = variantDefs[i];
                uniquenessValues[i] = def.isUnique;
                def.isUnique = true;
                spawnChanceValues[i] = def.spawnRate;
                def.spawnRate = blacklistedVariants.Contains(def) || def.spawnRate == 0 ? 0f : globalSpawnRate;
            }
            BodyVariantDefProvider.FilterVariants(SceneInfo.instance, Run.instance);
        }

        public override void OnExit()
        {
            base.OnExit();
            for(int i = 0; i < variantCount; i++)
            {
                var def = variantDefs[i];
                def.isUnique = uniquenessValues[i];
                def.spawnRate = spawnChanceValues[i];
            }
            BodyVariantDefProvider.FilterVariants(SceneInfo.instance, Run.instance);
        }
    }
}
