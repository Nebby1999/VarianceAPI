#nullable enable
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2BepInExPack.GameAssetPathsBetter;
using HG;

namespace VAPI
{
    public static partial class VAPIUtils
    {
        public static CharacterVariantDef? CloneIntoDevotedLemurian(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Devoted",
                originalToOtherGUID = new OriginalToOtherReferencePair<string>(RoR2_Base_Lemurian.LemurianBody_prefab, RoR2_CU8.DevotedLemurianBody_prefab),
                originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.LemurianBody), nameof(CU8Content.BodyPrefabs.DevotedLemurianBody)),
                originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Lemurian.LemurianBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_CU8.DevotedLemurianBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoDevotedLemurianBruiser(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Devoted",
                originalToOtherGUID = new OriginalToOtherReferencePair<string>(RoR2_Base_LemurianBruiser.LemurianBruiserBody_prefab, RoR2_CU8.DevotedLemurianBruiserBody_prefab),
                originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.LemurianBruiserBody), nameof(CU8Content.BodyPrefabs.DevotedLemurianBruiserBody)),
                originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_LemurianBruiser.LemurianBruiserBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_CU8.DevotedLemurianBruiserBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoBeetleGuardGland(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Gland",
                originalToOtherGUID = new OriginalToOtherReferencePair<string>(RoR2_Base_BeetleGuard.BeetleGuardBody_prefab, RoR2_Base_BeetleGland.BeetleGuardAllyBody_prefab),
                originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.BeetleGuardBody), nameof(RoR2Content.BodyPrefabs.BeetleGuardAllyBody)),
                originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_BeetleGuard.BeetleGuardBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_BeetleGland.BeetleGuardAllyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoRoboBallGreenBuddy(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "EmpathyGreen",
                originalToOtherGUID = new OriginalToOtherReferencePair<string>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab, RoR2_Base_RoboBallBuddy.RoboBallGreenBuddyBody_prefab),
                originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.RoboBallMiniBody), nameof(RoR2Content.BodyPrefabs.RoboBallGreenBuddyBody)),
                originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBuddy.RoboBallGreenBuddyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }
        public static CharacterVariantDef? CloneIntoRoboBallRedBuddy(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "EmpathyRed",
                originalToOtherGUID = new OriginalToOtherReferencePair<string>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab, RoR2_Base_RoboBallBuddy.RoboBallRedBuddyBody_prefab),
                originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.RoboBallMiniBody), nameof(RoR2Content.BodyPrefabs.RoboBallRedBuddyBody)),
                originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBoss.RoboBallMiniBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_RoboBallBuddy.RoboBallRedBuddyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoMinorConstructOnKill(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Nucleus",
                originalToOtherGUID = new OriginalToOtherReferencePair<string>(RoR2_DLC1_MajorAndMinorConstruct.MinorConstructBody_prefab, RoR2_DLC1_MajorAndMinorConstruct.MinorConstructOnKillBody_prefab),
                originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string>(nameof(DLC1Content.BodyPrefabs.MinorConstructBody), nameof(DLC1Content.BodyPrefabs.MinorConstructOnKillBody)),
                originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_MajorAndMinorConstruct.MinorConstructBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_MajorAndMinorConstruct.MinorConstructOnKillBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoVoidMegaCrabAlly(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Zoea",
                originalToOtherGUID = new OriginalToOtherReferencePair<string>(RoR2_DLC1_VoidMegaCrab.VoidMegaCrabBody_prefab, RoR2_DLC1_VoidMegaCrab.VoidMegaCrabAllyBody_prefab),
                originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string>(nameof(DLC1Content.BodyPrefabs.VoidMegaCrabBody), nameof(DLC1Content.BodyPrefabs.VoidMegaCrabAllyBody)),
                originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidMegaCrab.VoidMegaCrabBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidMegaCrab.VoidMegaCrabAllyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoVoidJailerAlly(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Zoea",
                originalToOtherGUID = new OriginalToOtherReferencePair<string>(RoR2_DLC1_VoidJailer.VoidJailerBody_prefab, RoR2_DLC1_VoidJailer.VoidJailerAllyBody_prefab),
                originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string>(nameof(DLC1Content.BodyPrefabs.VoidJailerBody), nameof(DLC1Content.BodyPrefabs.VoidJailerAllyBody)),
                originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidJailer.VoidJailerBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_DLC1_VoidJailer.VoidJailerAllyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoNullifierAlly(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Zoea",
                originalToOtherGUID = new OriginalToOtherReferencePair<string>(RoR2_Base_Nullifier.NullifierBody_prefab, RoR2_Base_Nullifier.NullifierAllyBody_prefab),
                originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.NullifierBody), nameof(RoR2Content.BodyPrefabs.NullifierAllyBody)),
                originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Nullifier.NullifierBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Nullifier.NullifierAllyBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoLemurianPolymorph(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Polymorph",
                originalToOtherGUID = new OriginalToOtherReferencePair<string>(RoR2_Base_Lemurian.LemurianBody_prefab, RoR2_DLC4_ShrineCompulsion.PolymorphLemurianBody_prefab),
                originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string>(nameof(RoR2Content.BodyPrefabs.LemurianBody), nameof(DLC4Content.BodyPrefabs.PolymorphLemurianBody)),
                originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject>(Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Lemurian.LemurianBody_prefab).WaitForCompletion(), Addressables.LoadAssetAsync<GameObject>(RoR2_DLC4_ShrineCompulsion.PolymorphLemurianBody_prefab).WaitForCompletion())
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoWispSoul(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Soul",
                originalToOtherGUID = new OriginalToOtherReferencePair<string> { original = RoR2_Base_Wisp.WispBody_prefab, other = RoR2_Base_WispOnDeath.WispSoulBody_prefab },
                originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string> { original = nameof(RoR2Content.BodyPrefabs.WispBody), other = nameof(RoR2Content.BodyPrefabs.WispSoulBody)},
                originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject> { original = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Wisp.WispBody_prefab).WaitForCompletion(), other = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_WispOnDeath.WispSoulBody_prefab).WaitForCompletion() }
            };

            return CloneForOtherBody(args);
        }

        private struct TwistedScavData
        {
            public string cloneSuffix;
            public string address;
            public string bodyName;
        }

        private static TwistedScavData[] _twistedScavDatas = new TwistedScavData[4]
        {
            new TwistedScavData
            {
                cloneSuffix = "Kipkip",
                address = RoR2_Base_ScavLunar.ScavLunar1Body_prefab,
                bodyName = nameof(RoR2Content.BodyPrefabs.ScavLunar1Body),
            },
            new TwistedScavData
            {
                cloneSuffix = "Wipwip",
                address = RoR2_Base_ScavLunar.ScavLunar2Body_prefab,
                bodyName = nameof(RoR2Content.BodyPrefabs.ScavLunar2Body),
            },
            new TwistedScavData
            {
                cloneSuffix = "Twiptwip",
                address = RoR2_Base_ScavLunar.ScavLunar3Body_prefab,
                bodyName = nameof(RoR2Content.BodyPrefabs.ScavLunar3Body),
            },
            new TwistedScavData
            {
                cloneSuffix = "Guragura",
                address = RoR2_Base_ScavLunar.ScavLunar4Body_prefab,
                bodyName = nameof(RoR2Content.BodyPrefabs.ScavLunar4Body)
            }
        };

        public static CharacterVariantDef[] CloneIntoLunarScavs(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            using var _ = ListPool<CharacterVariantDef>.RentCollection(out var results);

            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable
            };

            for(int i = 0; i < _twistedScavDatas.Length; i++)
            {
                CloneForOtherBodyArgs copy = args;
                copy.cloneSuffix = _twistedScavDatas[i].cloneSuffix;
                copy.originalToOtherGUID = new OriginalToOtherReferencePair<string> { original = RoR2_Base_Scav.ScavBody_prefab, other = _twistedScavDatas[i].address };
                copy.originalToOtherBodyCatalogReference = new OriginalToOtherReferencePair<string> { original = nameof(RoR2Content.BodyPrefabs.ScavBody), other = _twistedScavDatas[i].bodyName };
                copy.originalToOtherDirectReference = new OriginalToOtherReferencePair<GameObject> { original = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Scav.ScavBody_prefab).WaitForCompletion(), other = Addressables.LoadAssetAsync<GameObject>(_twistedScavDatas[i].address).WaitForCompletion() };

                var cloned = CloneForOtherBody(copy);
                if(cloned != null)
                {
                    results.Add(cloned);
                }
            }

            return results.ToArray();
        }

        public struct CloneForOtherBodyArgs
        {
            public CharacterVariantDef src;
            public OriginalToOtherReferencePair<GameObject> originalToOtherDirectReference;
            public OriginalToOtherReferencePair<string> originalToOtherGUID;
            public OriginalToOtherReferencePair<string> originalToOtherBodyCatalogReference;
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

        public static CharacterVariantDef? CloneForOtherBody(CloneForOtherBodyArgs args)
        {
            CharacterVariantDef src = args.src;
            OriginalToOtherReferencePair<GameObject> originalToOtherDirectReference = args.originalToOtherDirectReference;
            OriginalToOtherReferencePair<string> originalToOtherGUID = args.originalToOtherGUID;
            OriginalToOtherReferencePair<string> originalToOtherBodyCatalogReference = args.originalToOtherBodyCatalogReference;
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

                if (originalDirectReferenceValue != originalToOtherDirectReference.original)
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The target body has a direct asset reference, but said asset does not match the \"original\" value of the originalToOtherDirectReference value.");
                    return null;
                }
                else if (originalToOtherDirectReference.other == null)
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The originalToOtherDirectReference's value for \"other\" is null.");
                    return null;
                }
                else
                {
                    newDirectReferenceValue = originalToOtherDirectReference.other;
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

                if (!string.Equals(originalAddressValue, originalToOtherGUID.original))
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The target body has a valid GUID reference, but said GUID does not match the \"original\" value of the originalToOtherGUID value.");
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(originalToOtherGUID.other))
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The originalToOtherGUIDGetter's value for \"other\" is null.");
                }
                else
                {
                    newAddressValue = originalToOtherGUID.other;
                }

                VAPILog.Info($"Creating an clone Variant from {src} (originalBody={characterVariantTarget.targetBody.Address}|newBody={newAddressValue}). Be sure to add it to your VariantPack!");
                return CreateClone(src, cloneSuffix, new CharacterVariantTarget(newAddressValue), makeHidden, resultAvailableOnlyIfSrcAvailable);
            }
            else //Runtime key not valid, using catalog loading.
            {
                string newBodyName = "";
                string originalNameValue = characterVariantTarget.targetBody.Address;

                if (!string.Equals(originalNameValue, originalToOtherBodyCatalogReference.original))
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The target body has a valid catalog name reference, but said catalog name does not match the \"original\" value of the originalToOtherBodyCatalogReference's value.");
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(originalToOtherBodyCatalogReference.other))
                {
                    VAPILog.Warning($"Cannot clone {src} for another body. The originalToOtherBodyCatalogReferenceGetter's value for \"other\" is null.");
                }
                else
                {
                    newBodyName = originalToOtherBodyCatalogReference.other;
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