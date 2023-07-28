using Moonstorm;
using Moonstorm.Components;
using Moonstorm.Config;
using R2API;
using RiskOfOptions.OptionConfigs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPI.Modules
{
    internal static class Events
    {
        private static Dictionary<EventCard, ConfigurableBool> cardToEnabled = new Dictionary<EventCard, ConfigurableBool>();

        public static ConfigurableBool IsCardEnabled(EventCard card)
        {
            return cardToEnabled.TryGetValue(card, out var val) ? val : null;
        }

        public static void Init()
        {
            EventDirector.AddNewEntityStateMachine("VariantEvent");
            var cards = VAPIAssets.LoadAllAssetsOfType<EventCard>();
            foreach(var card in cards)
            {
                var shouldAdd = ShouldAdd(card);
                cardToEnabled.Add(card, shouldAdd);
                if (!shouldAdd)
                {
                    continue;
                }
                EventCatalog.AddCard(card);
            }
        }

        private static ConfigurableBool ShouldAdd(EventCard card)
        {
            string nicified = card.name.Substring(2);
            nicified = MSUtil.NicifyString(nicified);

            var key = $"Event - {nicified} :: ";
            return VAPIConfig.MakeConfigurableBool(true, b =>
            {
                b.Section = "Events";
                b.Key = key + "Enabled";
                b.Description = "Wether this event can play";
                b.ConfigFile = VAPIConfig.generalConfig;
                b.CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !VAPIConfig.addVariantEvents,
                    restartRequired = true
                };
            }).DoConfigure();
        }

        public class EventCardConfig
        {
            public string key;
            public ConfigurableBool enabled;
            public ConfigurableInt cost;
            public ConfigurableInt minimumStageCompletions;
            public ConfigurableEnum<DirectorAPI.Stage> stages;
        }
    }
}
