#nullable enable
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2BepInExPack.GameAssetPathsBetter;
using HG;

namespace VAPI
{
    /// <summary>
    /// Static class with methods for Cloning Variants from one body to "common" variation bodies.
    /// <br></br>
    /// For example, <see cref="CloneIntoDevotedLemurian(CharacterVariantDef, bool, bool)"/> can be used to clone a CharacterVariantDef for a LemurianBody into a CharacterVariantDef for a Devoted Lemurian
    /// </summary>
    public static class VariantCloning
    {
        public static CharacterVariantDef? CloneIntoDevotedLemurian(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Devoted",
                requiredSrcBodyName = nameof(RoR2Content.BodyPrefabs.LemurianBody),
                targetBodyName = nameof(CU8Content.BodyPrefabs.DevotedLemurianBody)
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
                requiredSrcBodyName = nameof(RoR2Content.BodyPrefabs.LemurianBruiserBody),
                targetBodyName = nameof(CU8Content.BodyPrefabs.DevotedLemurianBruiserBody)
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
                requiredSrcBodyName = nameof(RoR2Content.BodyPrefabs.BeetleGuardBody),
                targetBodyName = nameof(RoR2Content.BodyPrefabs.BeetleGuardAllyBody)
            };
            return CloneForOtherBody(args);
        }

        public static CharacterVariantDef? CloneIntoRoboBallGreenBuddy(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true) => CloneIntoRoboBallBuddyInternal(def, "Green", nameof(RoR2Content.BodyPrefabs.RoboBallGreenBuddyBody), makeHidden, resultAvailableOnlyIfSrcAvailable);
        public static CharacterVariantDef? CloneIntoRoboBallRedBuddy(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true) => CloneIntoRoboBallBuddyInternal(def, "Red", nameof(RoR2Content.BodyPrefabs.RoboBallRedBuddyBody), makeHidden, resultAvailableOnlyIfSrcAvailable);

        private static CharacterVariantDef? CloneIntoRoboBallBuddyInternal(CharacterVariantDef def, string suffixAppend, string targetBodyName, bool makeHidden, bool resultAvailableOnlyIfSrcAvailable)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Empathy" + suffixAppend,
                requiredSrcBodyName = nameof(RoR2Content.BodyPrefabs.RoboBallMiniBody),
                targetBodyName = targetBodyName
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
                requiredSrcBodyName = nameof(DLC1Content.BodyPrefabs.MinorConstructBody),
                targetBodyName = nameof(DLC1Content.BodyPrefabs.MinorConstructOnKillBody)
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
                requiredSrcBodyName = nameof(DLC1Content.BodyPrefabs.VoidMegaCrabBody),
                targetBodyName = nameof(DLC1Content.BodyPrefabs.VoidMegaCrabAllyBody)
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
                requiredSrcBodyName = nameof(DLC1Content.BodyPrefabs.VoidJailerBody),
                targetBodyName = nameof(DLC1Content.BodyPrefabs.VoidJailerAllyBody)
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
                requiredSrcBodyName = nameof(RoR2Content.BodyPrefabs.NullifierBody),
                targetBodyName = nameof(RoR2Content.BodyPrefabs.NullifierAllyBody)
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
                requiredSrcBodyName = nameof(RoR2Content.BodyPrefabs.LemurianBody),
                targetBodyName = nameof(DLC4Content.BodyPrefabs.PolymorphLemurianBody)
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
                requiredSrcBodyName = nameof(RoR2Content.BodyPrefabs.WispBody),
                targetBodyName = nameof(RoR2Content.BodyPrefabs.WispSoulBody),
            };

            return CloneForOtherBody(args);
        }


        /// <summary>
        /// Adds a new Lunar Scavenger to clone when an end user calls <see cref="CloneIntoLunarScavs(CharacterVariantDef, bool, bool)"/>
        /// </summary>
        /// <param name="cloneSuffix">The suffix to append to the <see cref="CharacterVariantDef"/>. By convention this is usually the "name" of the Twisted Scavenger, for example "Kipkip"</param>
        /// <param name="bodyName">The body prefab name of the twisted scavenger</param>
        /// <returns>True if the lunar scav was added to the clone array, false if there's a clone suffix conflict.</returns>
        public static bool AddLunarScavToClone(string cloneSuffix, string bodyName)
        {
            LunarScavData scavData = new LunarScavData
            {
                cloneSuffix = cloneSuffix,
                bodyName = bodyName,
            };

            for(int i = 0; i < _lunarScavDatas.Length; i++)
            {
                if (_lunarScavDatas[i].cloneSuffix == cloneSuffix)
                {
                    VAPILog.Warning($"Cannot add a new Lunar Scav to clone with suffix {cloneSuffix} as it conflicts with the pre-existing entry at index {i}");
                    return false;
                }
            }

            HG.ArrayUtils.ArrayAppend(ref _lunarScavDatas, scavData);
            return true;
        }

        private struct LunarScavData
        {
            public string cloneSuffix;
            public string bodyName;
        }

        private static LunarScavData[] _lunarScavDatas = new LunarScavData[4]
        {
            new LunarScavData
            {
                cloneSuffix = "Kipkip",
                bodyName = nameof(RoR2Content.BodyPrefabs.ScavLunar1Body),
            },
            new LunarScavData
            {
                cloneSuffix = "Wipwip",
                bodyName = nameof(RoR2Content.BodyPrefabs.ScavLunar2Body),
            },
            new LunarScavData
            {
                cloneSuffix = "Twiptwip",
                bodyName = nameof(RoR2Content.BodyPrefabs.ScavLunar3Body),
            },
            new LunarScavData
            {
                cloneSuffix = "Guragura",
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
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                requiredSrcBodyName = nameof(RoR2Content.BodyPrefabs.ScavBody)
            };

            for(int i = 0; i < _lunarScavDatas.Length; i++)
            {
                CloneForOtherBodyArgs copy = args;
                copy.cloneSuffix = _lunarScavDatas[i].cloneSuffix;
                copy.targetBodyName = _lunarScavDatas[i].bodyName;

                var clone = CloneForOtherBody(copy);
                if(clone != null)
                {
                    results.Add(clone);
                }
            }

            return results.ToArray();
        }

        public struct CloneForOtherBodyArgs
        {
            public CharacterVariantDef? src;
            public string? requiredSrcBodyName;
            public string? targetBodyName;
            public string? cloneSuffix;
            public bool? makeHidden;
            public bool? resultAvailableOnlyIfSrcAvailable;
        }

        public static CharacterVariantDef? CloneForOtherBody(CloneForOtherBodyArgs args)
        {
            CharacterVariantDef? src = args.src;
            string? targetBodyName = args.targetBodyName;
            string? requiredSrcBodyName = args.requiredSrcBodyName;
            string? cloneSuffix = args.cloneSuffix;
            bool makeHidden = args.makeHidden ?? true;
            bool resultAvailableOnlyIfSrcAvailable = args.resultAvailableOnlyIfSrcAvailable ?? true;

            if(src == null || string.IsNullOrWhiteSpace(targetBodyName) || string.IsNullOrWhiteSpace(cloneSuffix) || string.IsNullOrWhiteSpace(requiredSrcBodyName))
            {
                VAPILog.Warning($"Unable to begin cloning process, src, targetBodyName, requiredSrcBodyName or cloneSuffix values within CloneForOtherBodyArgs are invalid.");
                return null;
            }

            if(TryGetBodyNameFromSrc(src, out string? bodyName) == false)
            {
                VAPILog.Warning($"Unable to clone {src} into a variant for {targetBodyName} as VAPI was unable to retrieve {src}'s targetBody's name!");
                return null;
            }

            if(string.Equals(bodyName, requiredSrcBodyName) == false)
            {
                VAPILog.Warning($"Unable to clone {src} into a variant for {targetBodyName}. as the source's body name does not match the required src body name name (srcBodyName:{bodyName}|requiredSrcBodyName:{args.requiredSrcBodyName}");
                return null;
            }

            VAPILog.Info($"Creating a clone Variant from {src} (originalBody={bodyName}|newBody={targetBodyName}). Be sure to add it to your VariantPack!");
            return CreateClone(src, cloneSuffix, new CharacterVariantTarget(targetBodyName), makeHidden, resultAvailableOnlyIfSrcAvailable);
        }

        private static bool TryGetBodyNameFromSrc(CharacterVariantDef src, out string? bodyName)
        {
            bodyName = null;
            var targetBody = src.targetBody.targetBody;

            if(targetBody.IsInvalid)
            {
                return false;
            }

            if(targetBody.AssetExists)
            {
                bodyName = targetBody.Asset.name;
                return true;
            }
            else if(new AssetReferenceT<GameObject>(targetBody.Address).RuntimeKeyIsValid())
            {
                bodyName = Addressables.LoadAssetAsync<GameObject>(targetBody.Address).WaitForCompletion().name;
                return true;
            }
            bodyName = targetBody.Address;
            return true;
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