#nullable enable
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2BepInExPack.GameAssetPathsBetter;

namespace VAPI
{
    public static partial class VAPIUtils
    {
        public static CharacterVariantDef? CloneIntoDevotedLemurian(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneIntoAllyVariantArgs args = new CloneIntoAllyVariantArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Devoted",
                allyGUIDGetter = () => new CloneIntoAllyVariantReferencePair<string>(RoR2_Base_Lemurian.LemurianBody_prefab, RoR2_CU8.DevotedLemurianBody_prefab),
                allyCatalogReferenceGetter = () => new CloneIntoAllyVariantReferencePair<string>(nameof(RoR2Content.BodyPrefabs.LemurianBody), nameof(CU8Content.BodyPrefabs.DevotedLemurianBody)),
                allyDirectReferenceGetter = () => new CloneIntoAllyVariantReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Lemurian.LemurianBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_CU8.DevotedLemurianBody_prefab).WaitForCompletion())
            };
            return CloneIntoAllyVariant(args);
        }

        public static CharacterVariantDef? CloneIntoDevotedLemurianBruiser(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneIntoAllyVariantArgs args = new CloneIntoAllyVariantArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Devoted",
                allyGUIDGetter = () => new CloneIntoAllyVariantReferencePair<string>(RoR2_Base_LemurianBruiser.LemurianBruiserBody_prefab, RoR2_CU8.DevotedLemurianBruiserBody_prefab),
                allyCatalogReferenceGetter = () => new CloneIntoAllyVariantReferencePair<string>(nameof(RoR2Content.BodyPrefabs.LemurianBruiserBody), nameof(CU8Content.BodyPrefabs.DevotedLemurianBruiserBody)),
                allyDirectReferenceGetter = () => new CloneIntoAllyVariantReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_LemurianBruiser.LemurianBruiserBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_CU8.DevotedLemurianBruiserBody_prefab).WaitForCompletion())
            };
            return CloneIntoAllyVariant(args);
        }

        public static CharacterVariantDef? CloneIntoBeetleGuardGland(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneIntoAllyVariantArgs args = new CloneIntoAllyVariantArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Gland",
                allyGUIDGetter = () => new CloneIntoAllyVariantReferencePair<string>(RoR2_Base_BeetleGuard.BeetleGuardBody_prefab, RoR2_Base_BeetleGland.BeetleGuardAllyBody_prefab),
                allyCatalogReferenceGetter = () => new CloneIntoAllyVariantReferencePair<string>(nameof(RoR2Content.BodyPrefabs.BeetleGuardBody), nameof(RoR2Content.BodyPrefabs.BeetleGuardAllyBody)),
                allyDirectReferenceGetter = () => new CloneIntoAllyVariantReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_BeetleGuard.BeetleGuardBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_BeetleGland.BeetleGuardAllyBody_prefab).WaitForCompletion())
            };
            return CloneIntoAllyVariant(args);
        }

        public static CharacterVariantDef? CloneIntoRoboBallGreenBuddy(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneIntoAllyVariantArgs args = new CloneIntoAllyVariantArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "EmpathyGreen",
                allyGUIDGetter = () => new CloneIntoAllyVariantReferencePair<string>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab, RoR2_Base_RoboBallBuddy.RoboBallGreenBuddyBody_prefab),
                allyCatalogReferenceGetter = () => new CloneIntoAllyVariantReferencePair<string>(nameof(RoR2Content.BodyPrefabs.RoboBallMiniBody), nameof(RoR2Content.BodyPrefabs.RoboBallGreenBuddyBody)),
                allyDirectReferenceGetter = () => new CloneIntoAllyVariantReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBuddy.RoboBallGreenBuddyBody_prefab).WaitForCompletion())
            };
            return CloneIntoAllyVariant(args);
        }
        public static CharacterVariantDef? CloneIntoRoboBallRedBuddy(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneIntoAllyVariantArgs args = new CloneIntoAllyVariantArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "EmpathyRed",
                allyGUIDGetter = () => new CloneIntoAllyVariantReferencePair<string>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab, RoR2_Base_RoboBallBuddy.RoboBallRedBuddyBody_prefab),
                allyCatalogReferenceGetter = () => new CloneIntoAllyVariantReferencePair<string>(nameof(RoR2Content.BodyPrefabs.RoboBallMiniBody), nameof(RoR2Content.BodyPrefabs.RoboBallRedBuddyBody)),
                allyDirectReferenceGetter = () => new CloneIntoAllyVariantReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBuddy.RoboBallRedBuddyBody_prefab).WaitForCompletion())
            };
            return CloneIntoAllyVariant(args);
        }

        public static CharacterVariantDef? CloneIntoMinorConstructOnKill(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneIntoAllyVariantArgs args = new CloneIntoAllyVariantArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Nucleus",
                allyGUIDGetter = () => new CloneIntoAllyVariantReferencePair<string>(RoR2_DLC1_MajorAndMinorConstruct.MinorConstructBody_prefab, RoR2_DLC1_MajorAndMinorConstruct.MinorConstructOnKillBody_prefab),
                allyCatalogReferenceGetter = () => new CloneIntoAllyVariantReferencePair<string>(nameof(DLC1Content.BodyPrefabs.MinorConstructBody), nameof(DLC1Content.BodyPrefabs.MinorConstructOnKillBody)),
                allyDirectReferenceGetter = () => new CloneIntoAllyVariantReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_MajorAndMinorConstruct.MinorConstructBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_MajorAndMinorConstruct.MinorConstructOnKillBody_prefab).WaitForCompletion())
            };
            return CloneIntoAllyVariant(args);
        }

        public static CharacterVariantDef? CloneIntoVoidMegaCrabAlly(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneIntoAllyVariantArgs args = new CloneIntoAllyVariantArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Zoea",
                allyGUIDGetter = () => new CloneIntoAllyVariantReferencePair<string>(RoR2_DLC1_VoidMegaCrab.VoidMegaCrabBody_prefab, RoR2_DLC1_VoidMegaCrab.VoidMegaCrabAllyBody_prefab),
                allyCatalogReferenceGetter = () => new CloneIntoAllyVariantReferencePair<string>(nameof(DLC1Content.BodyPrefabs.VoidMegaCrabBody), nameof(DLC1Content.BodyPrefabs.VoidMegaCrabAllyBody)),
                allyDirectReferenceGetter = () => new CloneIntoAllyVariantReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidMegaCrab.VoidMegaCrabBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidMegaCrab.VoidMegaCrabAllyBody_prefab).WaitForCompletion())
            };
            return CloneIntoAllyVariant(args);
        }

        public static CharacterVariantDef? CloneIntoVoidJailerAlly(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneIntoAllyVariantArgs args = new CloneIntoAllyVariantArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Zoea",
                allyGUIDGetter = () => new CloneIntoAllyVariantReferencePair<string>(RoR2_DLC1_VoidJailer.VoidJailerBody_prefab, RoR2_DLC1_VoidJailer.VoidJailerAllyBody_prefab),
                allyCatalogReferenceGetter = () => new CloneIntoAllyVariantReferencePair<string>(nameof(DLC1Content.BodyPrefabs.VoidJailerBody), nameof(DLC1Content.BodyPrefabs.VoidJailerAllyBody)),
                allyDirectReferenceGetter = () => new CloneIntoAllyVariantReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidJailer.VoidJailerBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidJailer.VoidJailerAllyBody_prefab).WaitForCompletion())
            };
            return CloneIntoAllyVariant(args);
        }

        public static CharacterVariantDef? CloneIntoNullifierAlly(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneIntoAllyVariantArgs args = new CloneIntoAllyVariantArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Zoea",
                allyGUIDGetter = () => new CloneIntoAllyVariantReferencePair<string>(RoR2_Base_Nullifier.NullifierBody_prefab, RoR2_Base_Nullifier.NullifierAllyBody_prefab),
                allyCatalogReferenceGetter = () => new CloneIntoAllyVariantReferencePair<string>(nameof(RoR2Content.BodyPrefabs.NullifierBody), nameof(RoR2Content.BodyPrefabs.NullifierAllyBody)),
                allyDirectReferenceGetter = () => new CloneIntoAllyVariantReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Nullifier.NullifierBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Nullifier.NullifierAllyBody_prefab).WaitForCompletion())
            };
            return CloneIntoAllyVariant(args);
        }

        public struct CloneIntoAllyVariantArgs
        {
            public CharacterVariantDef src;
            public Func<CloneIntoAllyVariantReferencePair<GameObject>> allyDirectReferenceGetter;
            public Func<CloneIntoAllyVariantReferencePair<string>> allyGUIDGetter;
            public Func<CloneIntoAllyVariantReferencePair<string>> allyCatalogReferenceGetter;
            public string cloneSuffix;
            public bool? makeHidden;
            public bool? resultAvailableOnlyIfSrcAvailable;
        }

        public struct CloneIntoAllyVariantReferencePair<T>
        {
            public T? original;
            public T? allyVariant;

            public CloneIntoAllyVariantReferencePair(T? _original, T? _allyVariant)
            {
                original = _original;
                allyVariant = _allyVariant;
            }
        }

        public static CharacterVariantDef? CloneIntoAllyVariant(CloneIntoAllyVariantArgs args)
        {
            CharacterVariantDef src = args.src;
            Func<CloneIntoAllyVariantReferencePair<GameObject>> allyDirectReferenceGetter = args.allyDirectReferenceGetter;
            Func<CloneIntoAllyVariantReferencePair<string>> allyGUIDGetter = args.allyGUIDGetter;
            Func<CloneIntoAllyVariantReferencePair<string>> allyCatalogReferenceGetter = args.allyCatalogReferenceGetter;
            string cloneSuffix = args.cloneSuffix;
            bool makeHidden = args.makeHidden ?? true;
            bool resultAvailableOnlyIfSrcAvailable = args.resultAvailableOnlyIfSrcAvailable ?? true;

            var characterVariantTarget = src.targetBody;
            if (!characterVariantTarget.IsKeyValid())
            {
                VAPILog.Warning($"Cannot Clone {src} into an Ally Variant, the target body is not valid.");
                return null;
            }

            if (characterVariantTarget.targetBody.AssetExists)
            {
                GameObject newDirectReferenceValue = null;
                var originalDirectReferenceValue = characterVariantTarget.targetBody.Asset;

                CloneIntoAllyVariantReferencePair<GameObject> retrievedDirectReferencePair = allyDirectReferenceGetter.Invoke();
                if (originalDirectReferenceValue != retrievedDirectReferencePair.original)
                {
                    VAPILog.Warning($"Cannot clone {src} into an Ally Variant. The target body has a direct asset reference, but said asset does not match the \"original\" value of the allyDirectReferenceGetter function.");
                    return null;
                }
                else if (retrievedDirectReferencePair.allyVariant == null)
                {
                    VAPILog.Warning($"Cannot clone {src} into an Ally Variant. The allyDirectReferenceGetter function returned null for it's \"allyVariant\" value.");
                    return null;
                }
                else
                {
                    newDirectReferenceValue = retrievedDirectReferencePair.allyVariant;
                }

                VAPILog.Info($"Creating an Ally Variant from {src} (originalBody={characterVariantTarget.targetBody.Asset}|newBody={newDirectReferenceValue}). Be sure to add it to your VariantPack!");
                return CreateAllyClone(src, cloneSuffix, new CharacterVariantTarget(newDirectReferenceValue), makeHidden, resultAvailableOnlyIfSrcAvailable);
            }
            //At this point the asset doesnt exist, so lets check by address. We'll create a dummy asset reference and call it's RuntimeKeyIsValid property, if it returns true, we'll do the matching via the GUIDs.
            AssetReferenceT<GameObject> assetReferenceGameObject = new AssetReferenceT<GameObject>(characterVariantTarget.targetBody.Address);
            if (assetReferenceGameObject.RuntimeKeyIsValid())
            {
                string newAddressValue = "";
                string originalAddressValue = characterVariantTarget.targetBody.Address;

                CloneIntoAllyVariantReferencePair<string> retrievedGUIDReferencePair = allyGUIDGetter.Invoke();

                if (!string.Equals(originalAddressValue, retrievedGUIDReferencePair.original))
                {
                    VAPILog.Warning($"Cannot clone {src} into an Ally Variant. The target body has a valid GUID reference, but said GUID does not match the \"original\" value of the allyGUIDGetter Function.");
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(retrievedGUIDReferencePair.allyVariant))
                {
                    VAPILog.Warning($"Cannot clone {src} into an Ally Variant. The allyGUIDGetter function returned null for it's \"allyVariant\" value.");
                }
                else
                {
                    newAddressValue = retrievedGUIDReferencePair.allyVariant;
                }

                VAPILog.Info($"Creating an Ally Variant from {src} (originalBody={characterVariantTarget.targetBody.Address}|newBody={newAddressValue}). Be sure to add it to your VariantPack!");
                return CreateAllyClone(src, cloneSuffix, new CharacterVariantTarget(newAddressValue), makeHidden, resultAvailableOnlyIfSrcAvailable);
            }
            else //Runtime key not valid, using catalog loading.
            {
                string newBodyName = "";
                string originalNameValue = characterVariantTarget.targetBody.Address;

                CloneIntoAllyVariantReferencePair<string> retrievedCatalogReferencePair = allyCatalogReferenceGetter.Invoke();

                if (!string.Equals(originalNameValue, retrievedCatalogReferencePair.original))
                {
                    VAPILog.Warning($"Cannot clone {src} into an Ally Variant. The target body has a valid GUID reference, but said GUID does not match the \"original\" value of the allyGUIDGetter Function.");
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(retrievedCatalogReferencePair.allyVariant))
                {
                    VAPILog.Warning($"Cannot clone {src} into an Ally Variant. The allyGUIDGetter function returned null for it's \"allyVariant\" value.");
                }
                else
                {
                    newBodyName = retrievedCatalogReferencePair.allyVariant;
                }

                VAPILog.Info($"Creating an Ally Variant from {src} (originalBody={characterVariantTarget.targetBody.Address}|newBody={newBodyName}). Be sure to add it to your VariantPack!");
                return CreateAllyClone(src, cloneSuffix, new CharacterVariantTarget(newBodyName), makeHidden, resultAvailableOnlyIfSrcAvailable);
            }
        }

        private static CharacterVariantDef CreateAllyClone(CharacterVariantDef src, string cloneSuffix, CharacterVariantTarget newTarget, bool makeHidden, bool resultAvailableOnlyIfSrcAvailable)
        {
            if (string.IsNullOrWhiteSpace(cloneSuffix))
            {
                VAPILog.Warning($"An AllyClone for {src} is being made, but the cloneSuffix is null or whitespace! Appending current date time ticks as the suffix.");
                cloneSuffix = DateTime.Now.Ticks.ToString();
            }
            CharacterVariantDef copy = CharacterVariantDef.CreateInstance(src, $"{cloneSuffix}{src.cachedName}");
            copy.targetBody = newTarget;
            copy.isHidden = makeHidden;

            if (resultAvailableOnlyIfSrcAvailable)
            {
                copy.spawnCondition = new VariantDependantCondition(src);
            }
            return copy;
        }
    }
}