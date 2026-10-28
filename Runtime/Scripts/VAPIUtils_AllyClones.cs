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
            CloneForOtherBodyARgs args = new CloneForOtherBodyARgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Devoted",
                originalToOtherGUIDGetter = () => new OriginalToOtherReferencePair<string>(RoR2_Base_Lemurian.LemurianBody_prefab, RoR2_CU8.DevotedLemurianBody_prefab),
                originalToOtherBodyCatalogReferenceGetter = () => new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.LemurianBody), nameof(CU8Content.BodyPrefabs.DevotedLemurianBody)),
                originalToOtherDirectReferenceGetter = () => new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Lemurian.LemurianBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_CU8.DevotedLemurianBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoDevotedLemurianBruiser(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyARgs args = new CloneForOtherBodyARgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Devoted",
                originalToOtherGUIDGetter = () => new OriginalToOtherReferencePair<string>(RoR2_Base_LemurianBruiser.LemurianBruiserBody_prefab, RoR2_CU8.DevotedLemurianBruiserBody_prefab),
                originalToOtherBodyCatalogReferenceGetter = () => new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.LemurianBruiserBody), nameof(CU8Content.BodyPrefabs.DevotedLemurianBruiserBody)),
                originalToOtherDirectReferenceGetter = () => new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_LemurianBruiser.LemurianBruiserBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_CU8.DevotedLemurianBruiserBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoBeetleGuardGland(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyARgs args = new CloneForOtherBodyARgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Gland",
                originalToOtherGUIDGetter = () => new OriginalToOtherReferencePair<string>(RoR2_Base_BeetleGuard.BeetleGuardBody_prefab, RoR2_Base_BeetleGland.BeetleGuardAllyBody_prefab),
                originalToOtherBodyCatalogReferenceGetter = () => new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.BeetleGuardBody), nameof(RoR2Content.BodyPrefabs.BeetleGuardAllyBody)),
                originalToOtherDirectReferenceGetter = () => new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_BeetleGuard.BeetleGuardBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_BeetleGland.BeetleGuardAllyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoRoboBallGreenBuddy(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyARgs args = new CloneForOtherBodyARgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "EmpathyGreen",
                originalToOtherGUIDGetter = () => new OriginalToOtherReferencePair<string>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab, RoR2_Base_RoboBallBuddy.RoboBallGreenBuddyBody_prefab),
                originalToOtherBodyCatalogReferenceGetter = () => new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.RoboBallMiniBody), nameof(RoR2Content.BodyPrefabs.RoboBallGreenBuddyBody)),
                originalToOtherDirectReferenceGetter = () => new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBuddy.RoboBallGreenBuddyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }
        public static CharacterVariantDef? CloneIntoRoboBallRedBuddy(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyARgs args = new CloneForOtherBodyARgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "EmpathyRed",
                originalToOtherGUIDGetter = () => new OriginalToOtherReferencePair<string>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab, RoR2_Base_RoboBallBuddy.RoboBallRedBuddyBody_prefab),
                originalToOtherBodyCatalogReferenceGetter = () => new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.RoboBallMiniBody), nameof(RoR2Content.BodyPrefabs.RoboBallRedBuddyBody)),
                originalToOtherDirectReferenceGetter = () => new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBuddy.RoboBallRedBuddyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoMinorConstructOnKill(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyARgs args = new CloneForOtherBodyARgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Nucleus",
                originalToOtherGUIDGetter = () => new OriginalToOtherReferencePair<string>(RoR2_DLC1_MajorAndMinorConstruct.MinorConstructBody_prefab, RoR2_DLC1_MajorAndMinorConstruct.MinorConstructOnKillBody_prefab),
                originalToOtherBodyCatalogReferenceGetter = () => new OriginalToOtherReferencePair<string>(nameof(DLC1Content.BodyPrefabs.MinorConstructBody), nameof(DLC1Content.BodyPrefabs.MinorConstructOnKillBody)),
                originalToOtherDirectReferenceGetter = () => new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_MajorAndMinorConstruct.MinorConstructBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_MajorAndMinorConstruct.MinorConstructOnKillBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoVoidMegaCrabAlly(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyARgs args = new CloneForOtherBodyARgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Zoea",
                originalToOtherGUIDGetter = () => new OriginalToOtherReferencePair<string>(RoR2_DLC1_VoidMegaCrab.VoidMegaCrabBody_prefab, RoR2_DLC1_VoidMegaCrab.VoidMegaCrabAllyBody_prefab),
                originalToOtherBodyCatalogReferenceGetter = () => new OriginalToOtherReferencePair<string>(nameof(DLC1Content.BodyPrefabs.VoidMegaCrabBody), nameof(DLC1Content.BodyPrefabs.VoidMegaCrabAllyBody)),
                originalToOtherDirectReferenceGetter = () => new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidMegaCrab.VoidMegaCrabBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidMegaCrab.VoidMegaCrabAllyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoVoidJailerAlly(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyARgs args = new CloneForOtherBodyARgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Zoea",
                originalToOtherGUIDGetter = () => new OriginalToOtherReferencePair<string>(RoR2_DLC1_VoidJailer.VoidJailerBody_prefab, RoR2_DLC1_VoidJailer.VoidJailerAllyBody_prefab),
                originalToOtherBodyCatalogReferenceGetter = () => new OriginalToOtherReferencePair<string>(nameof(DLC1Content.BodyPrefabs.VoidJailerBody), nameof(DLC1Content.BodyPrefabs.VoidJailerAllyBody)),
                originalToOtherDirectReferenceGetter = () => new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidJailer.VoidJailerBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidJailer.VoidJailerAllyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoNullifierAlly(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyARgs args = new CloneForOtherBodyARgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Zoea",
                originalToOtherGUIDGetter = () => new OriginalToOtherReferencePair<string>(RoR2_Base_Nullifier.NullifierBody_prefab, RoR2_Base_Nullifier.NullifierAllyBody_prefab),
                originalToOtherBodyCatalogReferenceGetter = () => new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.NullifierBody), nameof(RoR2Content.BodyPrefabs.NullifierAllyBody)),
                originalToOtherDirectReferenceGetter = () => new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Nullifier.NullifierBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Nullifier.NullifierAllyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoLemurianPolymorph(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyARgs args = new CloneForOtherBodyARgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Polymorph",
                originalToOtherGUIDGetter = () => new OriginalToOtherReferencePair<string>(RoR2_Base_Lemurian.LemurianBody_prefab, RoR2_DLC4_ShrineCompulsion.PolymorphLemurianBody_prefab),
                originalToOtherBodyCatalogReferenceGetter = () => new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.LemurianBody), nameof(DLC4Content.BodyPrefabs.PolymorphLemurianBody)),
                originalToOtherDirectReferenceGetter = () => new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Lemurian.LemurianBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_DLC4_ShrineCompulsion.PolymorphLemurianBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public struct CloneForOtherBodyARgs
        {
            public CharacterVariantDef src;
            public Func<OriginalToOtherReferencePair<GameObject>> originalToOtherDirectReferenceGetter;
            public Func<OriginalToOtherReferencePair<string>> originalToOtherGUIDGetter;
            public Func<OriginalToOtherReferencePair<string>> originalToOtherBodyCatalogReferenceGetter;
            public string cloneSuffix;
            public bool? makeHidden;
            public bool? resultAvailableOnlyIfSrcAvailable;
        }

        public struct OriginalToOtherReferencePair<T>
        {
            public T? original;
            public T? other;

            public OriginalToOtherReferencePair(T? _original, T? _allyVariant)
            {
                original = _original;
                other = _allyVariant;
            }
        }

        public static CharacterVariantDef? CloneForOtherBody(CloneForOtherBodyARgs args)
        {
            CharacterVariantDef src = args.src;
            Func<OriginalToOtherReferencePair<GameObject>> originalToOtherDirectReferenceGetter = args.originalToOtherDirectReferenceGetter;
            Func<OriginalToOtherReferencePair<string>> originalToOtherGUIDGetter = args.originalToOtherGUIDGetter;
            Func<OriginalToOtherReferencePair<string>> originalToOtherBodyCatalogReferenceGetter = args.originalToOtherBodyCatalogReferenceGetter;
            string cloneSuffix = args.cloneSuffix;
            bool makeHidden = args.makeHidden ?? true;
            bool resultAvailableOnlyIfSrcAvailable = args.resultAvailableOnlyIfSrcAvailable ?? true;

            var characterVariantTarget = src.targetBody;
            if (!characterVariantTarget.IsKeyValid())
            {
                VAPILog.Warning($"Cannot Clone {src} for another body, the target body is not valid.");
                return null;
            }

            if (characterVariantTarget.targetBody.AssetExists)
            {
                GameObject? newDirectReferenceValue = null;
                var originalDirectReferenceValue = characterVariantTarget.targetBody.Asset;

                OriginalToOtherReferencePair<GameObject> retrievedDirectReferencePair = originalToOtherDirectReferenceGetter.Invoke();
                if (originalDirectReferenceValue != retrievedDirectReferencePair.original)
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The target body has a direct asset reference, but said asset does not match the \"original\" value of the originalToOtherDirectReferenceGetter function.");
                    return null;
                }
                else if (retrievedDirectReferencePair.other == null)
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The originalToOtherDirectReferenceGetter function returned null for it's \"other\" value.");
                    return null;
                }
                else
                {
                    newDirectReferenceValue = retrievedDirectReferencePair.other;
                }

                VAPILog.Info($"Creating a clone Variant from {src} (originalBody={characterVariantTarget.targetBody.Asset}|newBody={newDirectReferenceValue}). Be sure to add it to your VariantPack!");
                return CreateClone(src, cloneSuffix, new CharacterVariantTarget(newDirectReferenceValue), makeHidden, resultAvailableOnlyIfSrcAvailable);
            }
            //At this point the asset doesnt exist, so lets check by address. We'll create a dummy asset reference and call it's RuntimeKeyIsValid property, if it returns true, we'll do the matching via the GUIDs.
            AssetReferenceT<GameObject> assetReferenceGameObject = new AssetReferenceT<GameObject>(characterVariantTarget.targetBody.Address);
            if (assetReferenceGameObject.RuntimeKeyIsValid())
            {
                string newAddressValue = "";
                string originalAddressValue = characterVariantTarget.targetBody.Address;

                OriginalToOtherReferencePair<string> retrievedGUIDReferencePair = originalToOtherGUIDGetter.Invoke();

                if (!string.Equals(originalAddressValue, retrievedGUIDReferencePair.original))
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The target body has a valid GUID reference, but said GUID does not match the \"original\" value of the originalToOtherGUIDGetter Function.");
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(retrievedGUIDReferencePair.other))
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The originalToOtherGUIDGetter function returned null for it's \"other\" value.");
                }
                else
                {
                    newAddressValue = retrievedGUIDReferencePair.other;
                }

                VAPILog.Info($"Creating an clone Variant from {src} (originalBody={characterVariantTarget.targetBody.Address}|newBody={newAddressValue}). Be sure to add it to your VariantPack!");
                return CreateClone(src, cloneSuffix, new CharacterVariantTarget(newAddressValue), makeHidden, resultAvailableOnlyIfSrcAvailable);
            }
            else //Runtime key not valid, using catalog loading.
            {
                string newBodyName = "";
                string originalNameValue = characterVariantTarget.targetBody.Address;

                OriginalToOtherReferencePair<string> retrievedCatalogReferencePair = originalToOtherBodyCatalogReferenceGetter.Invoke();

                if (!string.Equals(originalNameValue, retrievedCatalogReferencePair.original))
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The target body has a valid catalog name reference, but said catalog name does not match the \"original\" value of the originalToOtherBodyCatalogReferenceGetter Function.");
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(retrievedCatalogReferencePair.other))
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The originalToOtherBodyCatalogReferenceGetter function returned null for it's \"other\" value.");
                }
                else
                {
                    newBodyName = retrievedCatalogReferencePair.other;
                }

                VAPILog.Info($"Creating clone Variant from {src} (originalBody={characterVariantTarget.targetBody.Address}|newBody={newBodyName}). Be sure to add it to your VariantPack!");
                return CreateClone(src, cloneSuffix, new CharacterVariantTarget(newBodyName), makeHidden, resultAvailableOnlyIfSrcAvailable);
            }
        }

        private static CharacterVariantDef CreateClone(CharacterVariantDef src, string cloneSuffix, CharacterVariantTarget newTarget, bool makeHidden, bool resultAvailableOnlyIfSrcAvailable)
        {
            if (string.IsNullOrWhiteSpace(cloneSuffix))
            {
                VAPILog.Warning($"A clone for {src} is being made, but the cloneSuffix is null or whitespace! Appending current date time ticks as the suffix.");
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