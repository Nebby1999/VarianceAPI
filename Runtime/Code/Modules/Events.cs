/*using MSU;
using MSU.Components;
using MSU.Config;
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
        private static Dictionary<EventCard, ConfiguredBool> cardToEnabled = new Dictionary<EventCard, ConfiguredBool>();

        public static ConfiguredBool IsCardEnabled(EventCard card)
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

        private static ConfiguredBool ShouldAdd(EventCard card)
        {
            string nicified = card.name.Substring(2);
            nicified = MSUtil.NicifyString(nicified);

            var key = $"Event - {nicified} :: ";
            return VAPIConfig.MakeConfiguredBool(true, b =>
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
            public ConfiguredBool enabled;
            public ConfigurableInt cost;
            public ConfigurableInt minimumStageCompletions;
            public ConfigurableEnum<DirectorAPI.Stage> stages;
        }
    }
}
*/