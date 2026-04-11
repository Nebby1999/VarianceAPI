#nullable enable
using BepInEx;
using BepInEx.Configuration;
using RoR2.ContentManagement;
using System;
using UnityEngine;

namespace VAPI
{
    public readonly struct ReadOnlyVariantPack
    {
        private readonly VariantPack src;
        public bool isValid => src != null;
        public BepInPlugin ownerPlugin => src.ownerPlugin;
        public ConfigFile? tierConfig => src.tierConfig;
        public ConfigFile? variantConfig => src.variantConfig;
        public string identifier => src.identifier;
        public string nameToken => src.nameToken;
        public string tooltipToken => src.tooltipToken;
        public string descriptionToken => src.descriptionToken;
        public Sprite packIcon => src.packIcon;

        public ReadOnlyNamedAssetCollection<CharacterVariantTierDef> characterVariantTierDefs => src.characterVariantTierDefs;
        public ReadOnlyNamedAssetCollection<CharacterVariantDef> characterVariantDefs => src.characterVariantDefs;

        public VariantPackIndex variantPackIndex => src.variantPackIndex;
        internal readonly bool isHidden => src._isHidden;

        public ReadOnlyVariantPack(VariantPack src)
        {
            this.src = src ?? throw new ArgumentNullException(nameof(src));
        }
    }

    public sealed class VariantPack
    {
        public readonly BepInPlugin ownerPlugin;
        public ConfigFile? tierConfig;
        public ConfigFile? variantConfig;

        public readonly string identifier = "UNIDENTIFIED";
        public readonly string nameToken = "";
        public readonly string tooltipToken = "";
        public readonly string descriptionToken = "";
        public readonly Sprite packIcon;

        public readonly NamedAssetCollection<CharacterVariantTierDef> characterVariantTierDefs = new NamedAssetCollection<CharacterVariantTierDef>(ContentPack.getScriptableObjectName);
        public readonly NamedAssetCollection<CharacterVariantDef> characterVariantDefs = new NamedAssetCollection<CharacterVariantDef>(ContentPack.getScriptableObjectName);

        internal readonly bool _isHidden;
        public VariantPackIndex variantPackIndex => _packIndex;
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