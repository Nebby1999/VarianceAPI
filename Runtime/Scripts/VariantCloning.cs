#nullable enable
using HG;
using RoR2;
using RoR2BepInExPack.GameAssetPathsBetter;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VAPI
{
    /// <summary>
    /// Static class with methods for Cloning Variants from one body to "common" variation bodies.
    /// <br></br>
    /// For example, <see cref="CloneIntoDevotedLemurian(CharacterVariantDef, bool, bool)"/> can be used to clone a CharacterVariantDef for a LemurianBody into a CharacterVariantDef for a Devoted Lemurian
    /// </summary>
    public static class VariantCloning
    {
        /// <summary>
        /// Clones a Lemurian variant into a Devoted Lemurian variant. Returns null if <paramref name="def"/> is null or not a lemurian variant.
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a Lemurian Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
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

        /// <summary>
        /// Clones a LemurianBruiser variant into a Devoted Elder Lemurian variant. Returns null if <paramref name="def"/> is null or not an elder lemurian variant.
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a LemurianBruiser Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
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

        /// <summary>
        /// Clones a BeetleGuard variant into a Beetle Guard Ally from the Queen's Gland item. Returns null if <paramref name="def"/> is null or not a Beetle Guard variant.
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a BeetleGuard Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
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

        /// <summary>
        /// Clones a RoboBallMini variant into a Delighted Probe variant. Returns null if <paramref name="def"/> is null or not a RoboBallMini variant
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a RoboBallMini Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
        public static CharacterVariantDef? CloneIntoRoboBallGreenBuddy(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true) => CloneIntoRoboBallBuddyInternal(def, "Green", nameof(RoR2Content.BodyPrefabs.RoboBallGreenBuddyBody), makeHidden, resultAvailableOnlyIfSrcAvailable);

        /// <summary>
        /// Clones a RoboBallMini variant into a Quiet Probe variant. Returns null if <paramref name="def"/> is null or not a RoboBallMini variant
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a RoboBallMini Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
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

        /// <summary>
        /// Clones a MinorConstruct variant into a MinorConstructOnKill variant. Returns null if <paramref name="def"/> is null or not a MinorConstruct variant
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a MinorConstruct Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
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

        /// <summary>
        /// Clones a Void Devastator variant into a Void Devastator Ally variant from the Newly Hatched Zoea item. Returns null if <paramref name="def"/> is null or not a VoidMegaCrab variant
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a VoidMegaCrab Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
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

        /// <summary>
        /// Clones a Void Jailer variant into a Void Jailer Ally variant from the Newly Hatched Zoea item. Returns null if <paramref name="def"/> is null or not a VoidJailer variant
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a VoidJailer Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
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

        /// <summary>
        /// Clones a Void Reaver variant into a Void Reaver Ally variant from the Newly Hatched Zoea item. Returns null if <paramref name="def"/> is null or not a Nullifier variant
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a Nullifier Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
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

        /// <summary>
        /// Clones a Lemurian variant into a Lemurian Polymorph variant. Returns null if <paramref name="def"/> is null or not a lemurian variant.
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a Lemurian Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
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

        /// <summary>
        /// Clones a Lesser Wisp variant into a Soul Lesser Wisp variant. Returns null if <paramref name="def"/> is null or not a Lesser Wisp variant.
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a Wisp Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
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
        /// Clones a Solus Control Unit variant into an Alloy Worship Unit variant. Returns null if <paramref name="def"/> is null or not a Solus Control Unit variant.
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a RoboBallBoss Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
        public static CharacterVariantDef? CloneIntoSuperRoboBallBoss(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "AWU",
                requiredSrcBodyName = nameof(RoR2Content.BodyPrefabs.RoboBallBossBody),
                targetBodyName = nameof(RoR2Content.BodyPrefabs.SuperRoboBallBossBody)
            };

            return CloneForOtherBody(args);
        }

        /// <summary>
        /// Clones a MinorConstruct variant into a MinorConstructAttachable variant. Returns null if <paramref name="def"/> is null or not a MinorConstruct variant.
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a MinorConstruct Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
        public static CharacterVariantDef? CloneIntoMinorConstructAttachable(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "MinorConstructAttachable",
                requiredSrcBodyName = nameof(DLC1Content.BodyPrefabs.MinorConstructBody),
                targetBodyName = nameof(DLC1Content.BodyPrefabs.MinorConstructAttachableBody)
            };

            return CloneForOtherBody(args);
        }

        /// <summary>
        /// Clones a MagmaWorm variant into an OverloadingWorm variant. Returns null if <paramref name="def"/> is null or not a MagmaWorm variant.
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a MagmaWorm Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
        /// <returns></returns>
        public static CharacterVariantDef? CloneIntoOverloadingWorm(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true)
        {
            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                cloneSuffix = "Overloading",
                requiredSrcBodyName = nameof(RoR2Content.BodyPrefabs.MagmaWormBody),
                targetBodyName = nameof(RoR2Content.BodyPrefabs.ElectricWormBody)
            };

            return CloneForOtherBody(args);
        }

        private struct ScavData
        {
            public string cloneSuffix;
            public string bodyName;
        }

        private static ScavData[] _lunarScavDatas = new ScavData[4]
        {
            new ScavData
            {
                cloneSuffix = "Kipkip",
                bodyName = nameof(RoR2Content.BodyPrefabs.ScavLunar1Body),
            },
            new ScavData
            {
                cloneSuffix = "Wipwip",
                bodyName = nameof(RoR2Content.BodyPrefabs.ScavLunar2Body),
            },
            new ScavData
            {
                cloneSuffix = "Twiptwip",
                bodyName = nameof(RoR2Content.BodyPrefabs.ScavLunar3Body),
            },
            new ScavData
            {
                cloneSuffix = "Guragura",
                bodyName = nameof(RoR2Content.BodyPrefabs.ScavLunar4Body)
            }
        };
        private static ScavData[] _sanctumScavDatas = new ScavData[3]
        {
            new ScavData
            {
                bodyName = nameof(DLC4Content.BodyPrefabs.ScavSanctum1Body),
                cloneSuffix = "Sanctum1",
            },
            new ScavData
            {
                bodyName = nameof(DLC4Content.BodyPrefabs.ScavSanctum2Body),
                cloneSuffix = "Sanctum2",
            },
            new ScavData
            {
                bodyName = nameof(DLC4Content.BodyPrefabs.ScavSanctum3Body),
                cloneSuffix = "Sanctum3"
            }
        };

        /// <summary>
        /// Adds a new Lunar Scavenger to clone when an end user calls <see cref="CloneIntoLunarScavs(CharacterVariantDef, bool, bool)"/>
        /// </summary>
        /// <param name="cloneSuffix">The suffix to append to the <see cref="CharacterVariantDef"/>. By convention this is usually the "name" of the Twisted Scavenger, for example "Kipkip"</param>
        /// <param name="bodyName">The body prefab name of the twisted scavenger</param>
        /// <returns>True if the lunar scav was added to the clone array, false if there's a clone suffix conflict.</returns>
        public static bool AddLunarScavToClone(string cloneSuffix, string bodyName) => AddScavToClone(cloneSuffix, bodyName, ref _lunarScavDatas);

        /// <summary>
        /// Adds a new Sanctum Scavenger to clone when an end user calls <see cref="CloneIntoSanctumScavs(CharacterVariantDef, bool, bool)"/>
        /// </summary>
        /// <param name="cloneSuffix">The suffix to append to the <see cref="CharacterVariantDef"/>. By convention this currently is the string "Sanctum" followed by a number, such as "Sanctum1"</param>
        /// <param name="bodyName">The body prefab name of the sanctum scavenger</param>
        /// <returns>True if the Sanctum scav was added to the clone array, false if there's a clone suffix conflict.</returns>
        public static bool AddSanctumScavToClone(string cloneSuffix, string bodyName) => AddScavToClone(cloneSuffix, bodyName, ref _sanctumScavDatas);

        private static bool AddScavToClone(string cloneSuffix, string bodyName, ref ScavData[] scavDataTarget)
        {
            ScavData scavData = new ScavData
            {
                cloneSuffix = cloneSuffix,
                bodyName = bodyName,
            };

            for (int i = 0; i < scavDataTarget.Length; i++)
            {
                if (scavDataTarget[i].cloneSuffix == cloneSuffix)
                {
                    VAPILog.Warning($"Cannot add a new Scav to clone with suffix {cloneSuffix} as it conflicts with the pre-existing entry at index {i}");
                    return false;
                }
            }

            HG.ArrayUtils.ArrayAppend(ref scavDataTarget, scavData);
            return true;
        }

        /// <summary>
        /// Clones a Scavenger variant into an LunarScavenger variant. Returns null if <paramref name="def"/> is null or not a Scavenger variant.
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a Scav Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
        /// <returns></returns>
        public static CharacterVariantDef[] CloneIntoLunarScavs(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true) => CloneIntoScavVariants(def, _lunarScavDatas, makeHidden, resultAvailableOnlyIfSrcAvailable);

        /// <summary>
        /// Clones a Scavenger variant into an SanctumScavenger variant. Returns null if <paramref name="def"/> is null or not a Scavenger variant.
        /// </summary>
        /// <param name="def">The source VariantDef, must point to a Scav Variant</param>
        /// <param name="makeHidden">If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations</param>
        /// <param name="resultAvailableOnlyIfSrcAvailable">If true, the cloned variant will only be available if <paramref name="def"/> is also available.</param>
        /// <returns></returns>
        public static CharacterVariantDef[] CloneIntoSanctumScavs(CharacterVariantDef def, bool makeHidden = true, bool resultAvailableOnlyIfSrcAvailable = true) => CloneIntoScavVariants(def, _sanctumScavDatas, makeHidden, resultAvailableOnlyIfSrcAvailable);
        private static CharacterVariantDef[] CloneIntoScavVariants(CharacterVariantDef def, ScavData[] scavData, bool makeHidden, bool resultAvailableOnlyIfSrcAvailable)
        {
            using var _ = ListPool<CharacterVariantDef>.RentCollection(out var results);

            CloneForOtherBodyArgs args = new CloneForOtherBodyArgs
            {
                src = def,
                makeHidden = makeHidden,
                resultAvailableOnlyIfSrcAvailable = resultAvailableOnlyIfSrcAvailable,
                requiredSrcBodyName = nameof(RoR2Content.BodyPrefabs.ScavBody)
            };

            for(int i = 0; i < scavData.Length; i++)
            {
                CloneForOtherBodyArgs copy = args;
                copy.cloneSuffix = scavData[i].cloneSuffix;
                copy.targetBodyName = scavData[i].bodyName;

                var clone = CloneForOtherBody(copy);
                if(clone != null)
                {
                    results.Add(clone);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// Arguments for <see cref="CloneForOtherBody(CloneForOtherBodyArgs)"/>
        /// </summary>
        public struct CloneForOtherBodyArgs
        {
            /// <summary>
            /// The source CharacterVariantDef to clone. Cannot be null
            /// </summary>
            public CharacterVariantDef? src;

            /// <summary>
            /// The cloning will proceed if the target body from <see cref="src"/> has this name. Cannot be null
            /// </summary>
            public string? requiredSrcBodyName;

            /// <summary>
            /// The target body for the new variant def. Cannot be null
            /// </summary>
            public string? targetBodyName;

            /// <summary>
            /// A Suffix to append to the new variant def. Cannot be null
            /// </summary>
            public string? cloneSuffix;

            /// <summary>
            /// If true, the cloned variant will be hidden from the rulebook and will not recieve spawning and uniqueness configurations. <b>Defaults to true</b>
            /// </summary>
            public bool? makeHidden;

            /// <summary>
            /// If true, the cloned variant will only be available if <see cref="src"/> is also available. <b>Defaults to true</b>
            /// </summary>
            public bool? resultAvailableOnlyIfSrcAvailable;
        }

        /// <summary>
        /// Class used for cloning a variant into another variant for another body. It is recommended to use the built in methods instead of this.
        /// <para></para>
        /// See <see cref="CloneForOtherBodyArgs"/> for more information
        /// </summary>
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

            LogMessage($"Creating a clone Variant from {src} (originalBody={bodyName}|newBody={targetBodyName}). Be sure to add it to your VariantPack!");
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
                LogWarning($"A clone for {src} is being made, but the cloneSuffix is null or whitespace! Appending current date time ticks as the suffix.");
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogMessage(object data)
        {
#if DEBUG
            VAPILog.Message(data);
#endif
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogError(object data)
        {
#if DEBUG
            VAPILog.Error(data);
#endif
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void LogWarning(object data)
        {
#if DEBUG
            VAPILog.Warning(data);
#endif
        }
    }
}