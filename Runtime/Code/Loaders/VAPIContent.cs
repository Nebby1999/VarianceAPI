using Moonstorm.Loaders;
using R2API.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPI.Modules;
namespace VAPI
{
    public class VAPIContent : ContentLoader<VAPIContent>
    {
        public override string identifier => VAPIMain.GUID;
        public override R2APISerializableContentPack SerializableContentPack { get; protected set; } = VAPIAssets.LoadAsset<R2APISerializableContentPack>("VAPIContent");
        public override Action[] LoadDispatchers { get; protected set; }
        public override Action[] PopulateFieldsDispatchers { get; protected set; }

        public override void Init()
        {
            base.Init();
            LoadDispatchers = new Action[]
            {
                () => new ItemModule().Initialize(),
                () => VAPIAssets.Instance.SwapShaders(),
            };
        }
    }
}
