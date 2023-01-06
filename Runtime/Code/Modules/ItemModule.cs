using Moonstorm;
using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace VAPI.Modules
{
    /// <summary>
    /// VarianceAPI's ItemModule
    /// </summary>
    public class ItemModule : ItemModuleBase
    {
        /// <summary>
        /// VarianceAPI's ContentPack
        /// </summary>
        public override R2APISerializableContentPack SerializableContentPack => VAPIContent.Instance.SerializableContentPack;

        /// <summary>
        /// VarianceAPI's itemModule
        /// </summary>
        public static ItemModule Instance { get; private set; }

        /// <summary>
        /// Do not call this twice.
        /// </summary>
        public override void Initialize()
        {
            Instance = this;
            VAPILog.Info($"Initializing Items...");
            base.Initialize();
            GetItemBases();
        }

        protected override IEnumerable<ItemBase> GetItemBases()
        {
            base.GetItemBases()
                .ToList()
                .ForEach(itemBase => AddItem(itemBase));
            return null;
        }
    }
}