using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPI.Components;

namespace VAPI
{
    public class VariantSummon : MasterSummon
    {
        public struct VariantSummonReport
        {
            public CharacterMaster summonMasterInstance;
            public VariantDef[] summonInstanceVariants;
            public CharacterMaster leaderMasterInstance;
        }
        public VariantDef[] variantDefs;
        public bool applyOnStart = true;

        public static event Action<VariantSummonReport> onServerVariantSummonGlobal;
        public CharacterMaster Perform()
        {
            var master = base.Perform();
            var body = master.GetBodyObject();
            if (body)
            {
                body.AddComponent<DoNotTurnIntoVariant>();
                var bodyVariantManager = body.GetComponent<BodyVariantManager>();
                var bodyVariantReward = body.GetComponentInParent<BodyVariantReward>();

                if (bodyVariantManager)
                {
                    bodyVariantManager.AddVariants(variantDefs);
                    bodyVariantManager.applyOnStart = applyOnStart;
                }
                if (bodyVariantReward)
                {
                    bodyVariantReward.AddVariants(variantDefs);
                }
            }

            CharacterMaster leader = null;
            if (summonerBodyObject)
            {
                CharacterBody summonerBody = summonerBodyObject.GetComponent<CharacterBody>();
                if(summonerBody)
                {
                    leader = summonerBody.master;
                }
            }
            VariantSummonReport report = new VariantSummonReport
            {
                leaderMasterInstance = leader,
                summonInstanceVariants = variantDefs,
                summonMasterInstance = master,
            };
            onServerVariantSummonGlobal?.Invoke(report);

            return master;
        }
    }
}