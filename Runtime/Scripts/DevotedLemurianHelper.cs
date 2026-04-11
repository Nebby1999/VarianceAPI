#nullable enable
using RoR2;
using UnityEngine;

namespace VAPI
{
    public static class DevotedLemurianHelper
    {
        [SystemInitializer(typeof(CharacterVariantCatalog))]
        private static void Initialize()
        {
            //Devoted lemurians suck, technically speaking a single master is supposed to control both the Regular Lemurian and the Elder Lemurian. So as a result we need to be a bit funny with how we're doing variants for them.
            //Firstly, the main master's CharacterMasterVariantStorage will _not_ roll for variants. The Body controller will roll for the variants, and if the master storage exists, store it there. This will take into account the following facts:
            //  * The first time a devoted lemurian spawns, it'll roll it's variants and store them so they persist across stages.
            //  * Once the devoted lemurian "Evolves" to an Elder Lemurian, it'll just replace the body with the new one, the onBodyStart check there will clear the master's storage, so the Elder variants can roll.
            // For what it's worth, this Initialize method runs before the VariantSpawnManager even exists, so due to events being FIFE (First in, First Executed). the body will clear the master's variants before the CharacterBodyVariantController rolls for variants.
            CharacterBody.onBodyStartGlobal += RemoveVariants;
            MasterCatalog.MasterIndex devotedLemurianMasterIndex = MasterCatalog.FindMasterIndex("DevotedLemurianMaster");
            if(devotedLemurianMasterIndex != MasterCatalog.MasterIndex.none)
            {
                GameObject masterPrefab = MasterCatalog.GetMasterPrefab(devotedLemurianMasterIndex);
                if(masterPrefab.TryGetComponent<CharacterMasterVariantStorage>(out var variantStorage))
                {
                    variantStorage.doNotRollForVariants = true;
                }
            }
        }

        private static void RemoveVariants(CharacterBody obj)
        {
            if(obj.bodyIndex == CU8Content.BodyPrefabs.DevotedLemurianBruiserBody.bodyIndex && obj.master && obj.master.TryGetComponent<CharacterMasterVariantStorage>(out var variantStorage))
            {
                variantStorage.ClearVariantDefsForCharacterServer();
            }
        }
    }
}