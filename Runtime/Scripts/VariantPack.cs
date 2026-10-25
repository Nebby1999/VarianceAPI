#nullable enable
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.ContentManagement;
using System;
using UnityEngine;

namespace VAPI
{
    /// <summary>
    /// A read only representation of a <see cref="VariantPack"/>
    /// </summary>
    public readonly struct ReadOnlyVariantPack
    {
        //Access only to us.
        internal readonly VariantPack src;

        /// <summary>
        /// Returns true if the ReadOnlyVariantPack is valid.
        /// </summary>
        public bool isValid => src != null;

        /// <summary>
        /// Access to the plugin that added the pack
        /// </summary>
        public BepInPlugin ownerPlugin => src.ownerPlugin;

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
        /// Returns if this pack is hidden, internal use only
        /// </summary>
        internal readonly bool isHidden => src._isHidden;

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
        public readonly BepInPlugin ownerPlugin;

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
        /// Wether this pack is hidden from the rulebook, internal use only
        /// </summary>
        internal readonly bool _isHidden;

        /// <summary>
        /// Your VariantPack's unique index, used for networking purposes and general housekeeping
        /// </summary>
        public VariantPackIndex variantPackIndex => _packIndex;

        /// <summary>
        /// Your VariantPack's enabled choice in the rulebook, this is null up until VarianceAPI's rulebook class initializes
        /// </summary>
        public RuleChoiceDef? packEnabledChoice { get; internal set; }

        internal VariantPackIndex _packIndex;

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
        /// <exception cref="ArgumentNullException">When any of the non-nullable arguments are null</exception>
        public VariantPack(string _identifier, string _nameToken, string _tooltipToken, string _descriptionToken, Sprite _packIcon, BaseUnityPlugin _ownerPlugin, ConfigFile? _tierConfig, ConfigFile? _variantConfig)
        {
            identifier = _identifier ?? throw new ArgumentNullException(nameof(_identifier));

            nameToken = _nameToken ?? throw new ArgumentNullException(nameof(_nameToken));
            tooltipToken = _tooltipToken ?? throw new ArgumentNullException( nameof(_tooltipToken));
            descriptionToken = _descriptionToken ?? throw new ArgumentNullException(nameof(_descriptionToken));
            packIcon = _packIcon ?? throw new ArgumentNullException(nameof(_packIcon));

            ownerPlugin = _ownerPlugin?.Info?.Metadata ?? throw new ArgumentNullException(nameof(_ownerPlugin));
            tierConfig = _tierConfig;
            variantConfig = _variantConfig;
        }

        /// <summary>
        /// Internal constructor for the Base API's VariantPack, which includes the Common, Uncommon, Rare and Legendary Tiers.
        /// <br></br>
        /// If you decided to publicize VAPI, DO NOT utilize this constructor!
        /// </summary>
        /// <exception cref="ArgumentNullException">When any of the non nullable arguments are null</exception>
        internal VariantPack(string _identifier, Sprite _packIcon, ConfigFile _tierConfig, BepInPlugin _ownerPlugin)
        {
            identifier = _identifier ?? throw new ArgumentNullException(nameof(_identifier), "Nebby is kinda silly");
            packIcon = _packIcon ?? throw new ArgumentNullException(nameof(_packIcon), "Nebby is kinda silly");
            tierConfig = _tierConfig ?? throw new ArgumentNullException(nameof(_tierConfig), "Nebby is kinda silly");
            ownerPlugin = _ownerPlugin ?? throw new ArgumentNullException(nameof(_ownerPlugin), "Nebby is kinda silly");
            _isHidden = true;
        }
    }
}