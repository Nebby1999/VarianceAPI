using Moonstorm;
using Moonstorm.Config;
using RiskOfOptions.OptionConfigs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPI;

namespace EntityStates.Events.VariantEvents
{
    public class IncreaseSpawnRateEvent : VariantEventState
    {
        public static ConfigurableFloat spawnRateMultiplier = VAPIConfig.MakeConfigurableFloat(75f, f =>
        {
            f.ConfigFile = VAPIConfig.generalConfig;
            f.Section = "Events";
            f.Key = $"Event - Increased Spawn Rate :: Spawn Rate Multiplier";
            f.Description = "The Spawn rate multiplier thats applied to variants";
            f.UseStepSlider = false;
            f.SliderConfig = new SliderConfig
            {
                formatString = "{0:0.0}",
                min = 0,
                max = 100,
                checkIfDisabled = () =>
                {
                    var card = VAPIAssets.LoadAsset<EventCard>("ecIncreasedSpawnRate");
                    var cfg = VAPI.Modules.Events.IsCardEnabled(card);
                    return !VAPIConfig.addVariantEvents || !cfg;
                }
            };
        }).DoConfigure();

        public override void StartEvent()
        {
            base.StartEvent();
            var num = SpawnManager.DefaultSpawnRateMultiplier;
            num += spawnRateMultiplier;
            SpawnManager.DefaultSpawnRateMultiplier = num;
        }

        public override void OnExit()
        {
            base.OnExit();
            var num = SpawnManager.DefaultSpawnRateMultiplier;
            num -= spawnRateMultiplier;
            SpawnManager.DefaultSpawnRateMultiplier = num;
        }
    }
}
