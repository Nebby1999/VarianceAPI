using System.Collections.Generic;
using UnityEngine;

namespace VarianceAPI
{
    public static class VariantMaterialGrabber
    {
        public static Dictionary<string, Material> vanillaMaterials = new Dictionary<string, Material>();
        public static void SwapMaterials()
        {
            VAPILog.LogI("Swapping Materials...");
            foreach (var kvp in VariantRegister.RegisteredVariants)
            {
                var currentList = VariantRegister.RegisteredVariants[kvp.Key];
                foreach (var vi in currentList)
                {
                    if (vi.visualModifier)
                    {
                        if (vi.visualModifier.MaterialReplacements.Length != 0)
                        {
                            for (int i = 0; i < vi.visualModifier.MaterialReplacements.Length; i++)
                            {
                                var currentMaterial = vi.visualModifier.MaterialReplacements[i];
                                if (currentMaterial.identifier == string.Empty)
                                {
                                    continue;
                                }
                                if (vanillaMaterials.TryGetValue(currentMaterial.identifier, out var replacement))
                                {
                                    if (replacement)
                                    {
                                        currentMaterial.material = replacement;
                                        vi.visualModifier.MaterialReplacements[i] = currentMaterial;
                                        VAPILog.LogD($"Replaced {vi.identifier}'s {currentMaterial.identifier}'s material with {replacement.name}");
                                    }
                                }
                                else
                                {
                                    VAPILog.LogW($"Could not find a material with the identifier {currentMaterial.identifier} inside {vanillaMaterials}");
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void StoreMaterial(string identifier, Material material)
        {
            if (vanillaMaterials.ContainsKey(identifier))
            {
                VAPILog.LogW($"A key with the identifier of {identifier} has already been added. aboirting adding to the material replacement dictionary.");
                return;
            }
            vanillaMaterials.Add(identifier, material);
        }
        public static void StoreMaterials(IEnumerable<(string, Material)> MaterialsAndIdentifiers)
        {
            foreach ((string identifier, Material material) in MaterialsAndIdentifiers)
            {
                if (vanillaMaterials.ContainsKey(identifier))
                {
                    VAPILog.LogW($"A key with the identifier of {identifier} has already been added. aboirting adding to the material replacement dictionary.");
                    continue;
                }
                vanillaMaterials.Add(identifier, material);
            }
        }
    }
}
