#nullable enable
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A read only representation of a <see cref="VariantPack"/>
    /// </summary>
    public readonly struct ReadOnlyVariantPack
    {
        /// <summary>
        /// An <see cref="IEqualityComparer{T}"/> implementation for <see cref="ReadOnlyVariantPack"/> that compares <see cref="identifier"/>s
        /// </summary>
        public struct IdentifierComparer : IEqualityComparer<ReadOnlyVariantPack>
        {
            /// <summary>
            /// Returns true if the Identifiers on <paramref name="x"/> and <paramref name="y"/> are equal.
            /// <br></br>
            /// If either <paramref name="x"/> or <paramref name="y"/> point to an invalid VariantPack, it returns false
            /// </summary>
            /// <param name="x">A Read only variant pack</param>
            /// <param name="y">A read only variant pack</param>
            public bool Equals(ReadOnlyVariantPack x, ReadOnlyVariantPack y)
            {
                if (!x.isValid || !y.isValid)
                {
                    return false;
                }

                string xIdentifier = x.identifier;
                string yIdentifier = y.identifier;

                return string.Equals(xIdentifier, yIdentifier);
            }

            /// <summary>
            /// Returns the HashCode for <paramref name="obj"/>, by calling the GetHashCode() method of <see cref="identifier"/>
            /// <br></br>
            /// If <paramref name="obj"/> points to an invalid VariantPack, it returns -1
            /// </summary>
            /// <param name="obj">The ReadOnlyVariantPack to obtain it's hash code</param>
            public int GetHashCode(ReadOnlyVariantPack obj)
            {
                if (!obj.isValid)
                {
                    return -1;
                }
                return obj.identifier.GetHashCode();
            }
        }
        //Access only to us.
        internal readonly VariantPack src;

        /// <summary>
        /// Returns true if the ReadOnlyVariantPack is valid.
        /// </summary>
        public bool isValid => src != null;

        /// <summary>
        /// Access to the plugin that added the pack
        /// </summary>
        public BaseUnityPlugin ownerPlugin => src.ownerPlugin;

        /// <summary>
        /// Access to the pack's tier config, may be null.
        /// </summary>
        public ConfigFile? tierConfig => src.tierConfig;

        /// <summary>
        /// Access to the pack's variant config, may be null
        /// </summary>
        public ConfigFile? variantConfig => src.variantConfig;

        /// <summary>
        /// Access to the pack's identifier
        /// </summary>
        public string identifier => src.identifier;

        /// <summary>
        /// Access to the pack's name token, used in the rulebook
        /// </summary>
        public string nameToken => src.nameToken;

        /// <summary>
        /// Access to the pack's tooltip token, used in the rulebook
        /// </summary>
        public string tooltipToken => src.tooltipToken;

        /// <summary>
        /// Access to the pack's description token, used in the rulebook
        /// </summary>
        public string descriptionToken => src.descriptionToken;

        /// <summary>
        /// Access to this pack's icon, used in the rulebook
        /// </summary>
        public Sprite packIcon => src.packIcon;

        /// <summary>
        /// Access to this pack's enabled choice, this is null up until VarianceAPI's Rulebook Class initializes
        /// </summary>
        public RuleChoiceDef? packEnabledChoice => src.packEnabledChoice;

        /// <summary>
        /// Access to this pack's <see cref="CharacterVariantTierDef"/>s
        /// </summary>
        public ReadOnlyNamedAssetCollection<CharacterVariantTierDef> characterVariantTierDefs => src.characterVariantTierDefs;

        /// <summary>
        /// Access to this pack's <see cref="CharacterVariantDef"/>
        /// </summary>
        public ReadOnlyNamedAssetCollection<CharacterVariantDef> characterVariantDefs => src.characterVariantDefs;

        /// <summary>
        /// Access to this pack's index
        /// </summary>
        public VariantPackIndex variantPackIndex => src.variantPackIndex;

        /// <summary>
        /// Returns if this pack is hidden from the rulebook
        /// </summary>
        internal readonly bool isHidden => src.isHidden;

        /// <summary>
        /// Constructor. Throws a <see cref="ArgumentNullException"/> if <paramref name="src"/> is null.
        /// </summary>
        /// <param name="src">The VariantPack to encapsulate in a <see cref="ReadOnlyVariantPack"/></param>
        public ReadOnlyVariantPack(VariantPack src)
        {
            this.src = src ?? throw new ArgumentNullException(nameof(src));
        }
    }

    /// <summary>
    /// The <see cref="VariantPack"/> is a class used to store and specify which <see cref="CharacterVariantDef"/>s and which <see cref="CharacterVariantTierDef"/>s are from your mod.
    /// <br></br>
    /// A ReadOnly version of this can be exposed via the <see cref="ReadOnlyVariantPack"/>
    /// </summary>
    public sealed class VariantPack
    {
        /// <summary>
        /// The plugin that added this pack
        /// </summary>
        public readonly BaseUnityPlugin ownerPlugin;

        /// <summary>
        /// The config file for this pack's tiers, can be null
        /// </summary>
        public ConfigFile? tierConfig;

        /// <summary>
        /// The config file for this pack's variants, can be null
        /// </summary>
        public ConfigFile? variantConfig;

        /// <summary>
        /// An identifier for this pack, must be unique.
        /// </summary>
        public readonly string identifier = "UNIDENTIFIED";

        /// <summary>
        /// The name token for this pack, this is used in the rulebook for enabling/disabling variant packs.
        /// </summary>
        public readonly string nameToken = "";

        /// <summary>
        /// The tooltip token for this pack, this is used in the rulebook for enabling/disabling variant packs.
        /// </summary>
        public readonly string tooltipToken = "";

        /// <summary>
        /// The description token for this pack, this is used in the rulebook for enabling/disabling variant packs.
        /// </summary>
        public readonly string descriptionToken = "";

        /// <summary>
        /// The icon for this pack, this is used in the rulebook for enabling/disabling variant packs.
        /// </summary>
        public readonly Sprite packIcon;

        /// <summary>
        /// A <see cref="NamedAssetCollection{TAsset}"/> that contains your pack's <see cref="CharacterVariantTierDef"/>
        /// </summary>
        public readonly NamedAssetCollection<CharacterVariantTierDef> characterVariantTierDefs = new NamedAssetCollection<CharacterVariantTierDef>(ContentPack.getScriptableObjectName);

        /// <summary>
        /// A <see cref="NamedAssetCollection{TAsset}"/> that contains your pack's <see cref="CharacterVariantDef"/>
        /// </summary>
        public readonly NamedAssetCollection<CharacterVariantDef> characterVariantDefs = new NamedAssetCollection<CharacterVariantDef>(ContentPack.getScriptableObjectName);

        /// <summary>
        /// Wether this pack is hidden from the rulebook
        /// </summary>
        public readonly bool isHidden;

        /// <summary>
        /// Your VariantPack's unique index, used for networking purposes and general housekeeping
        /// </summary>
        public VariantPackIndex variantPackIndex => _packIndex;
        internal VariantPackIndex _packIndex;

        /// <summary>
        /// Your VariantPack's enabled choice in the rulebook, this is null up until VarianceAPI's rulebook class initializes
        /// </summary>
        public RuleChoiceDef? packEnabledChoice { get; internal set; }

        /// <summary>
        /// Constructor for creating a Variant Pack
        /// </summary>
        /// <param name="_identifier">A unique identifier for this variant pack, this is not user facing</param>
        /// <param name="_nameToken">The token utilized on the lobby to display this VariantPack.</param>
        /// <param name="_tooltipToken">The token utilized as the tooltip on the lobby to display this VariantPack.</param>
        /// <param name="_descriptionToken">The token utilized as the description on the lobby to display this VariantPack</param>
        /// <param name="_packIcon">The icon for this pack, utilized in the built in Risk of Options generator and in the lobby for the "Pack Enabled" icon</param>
        /// <param name="_ownerPlugin">The plugin that's adding this VariantPack</param>
        /// <param name="_tierConfig">The ConfigFile for configuring this VariantPack's tiers. If left null, the Tiers in this VariantPack cannot be configured</param>
        /// <param name="_variantConfig">The ConfigFile for configuring this VariantPack's Variants. If left null, the Variants in this VariantPack cannot be configured</param>
        /// <param name="_isHidden">Wether the VariantPack is hidden from the rulebook display, this shouldnt really be set to true, unless you're creating a Pack that contains exclusively tiers</param>
        /// <exception cref="ArgumentNullException">When any of the non-nullable arguments are null</exception>
        public VariantPack(string _identifier, string _nameToken, string _tooltipToken, string _descriptionToken, Sprite _packIcon, BaseUnityPlugin _ownerPlugin, ConfigFile? _tierConfig, ConfigFile? _variantConfig, bool _isHidden = false)
        {
            identifier = _identifier ?? throw new ArgumentNullException(nameof(_identifier));

            nameToken = _nameToken ?? throw new ArgumentNullException(nameof(_nameToken));
            tooltipToken = _tooltipToken ?? throw new ArgumentNullException(nameof(_tooltipToken));
            descriptionToken = _descriptionToken ?? throw new ArgumentNullException(nameof(_descriptionToken));
            packIcon = _packIcon ?? throw new ArgumentNullException(nameof(_packIcon));

            ownerPlugin = _ownerPlugin ?? throw new ArgumentNullException(nameof(_ownerPlugin));
            tierConfig = _tierConfig;
            variantConfig = _variantConfig;
            isHidden = _isHidden;
        }
    }
}