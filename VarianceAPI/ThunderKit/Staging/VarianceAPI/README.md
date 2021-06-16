# Variance API

* A Complete Re-Write, altho heavily based off Rob's original MonsterVariants code.

* VarianceAPI aims to combine the features from MonsterVariants and MonsterVariantsPlus into a single mod.

* The API by itself doesnt do much, it only works as a base for others to create their own Variants

## Features

### Thunderkit support

---

* VarianceAPI was made with Thunderkit, and it's systems are made to be used with Thunderkit. it is Extremely recommended to use Thunderkit when Developing VariantPacks

* Despite this, Variants can still be added via code. Only examples currently lay on the Deprecated ExampleVariantPack. The mod comes with a [Helpers.cs](https://github.com/Nebby1999/VarianceAPI/blob/main/VarianceAPI/Assets/Scripts/Modules/Helpers.cs) class that has methods on creating the ScriptableObjects that VarianceAPI requires

![](https://cdn.discordapp.com/attachments/850538397647110145/850538428600549406/3f27f0ee908f6a7fbbe5cb37bf008e52.png)

* Easily create Variants using the plethora of ScriptableObjects VarianceAPI has!

![](https://cdn.discordapp.com/attachments/850538397647110145/850539440274145290/db4b9fe789a08d6816dd03ca70bb0cd2.png)

* Plus, VarianceAPI contains base classes for easing the creation of variants.

	- Automatic ConfigCreation using the VariantConfig scriptable object.

	- Semi-Automatic fetching for Ingame materials, no more AssetBundle bloating!

### Better VariantOverlapping methods

---

* The confusing and sometimes obscure VariantOverlap system has been expanded and now can be either Avoided, or encouraged.

	- Each Variant has a Unique boolean, Which determine wether the Variant can participate in VariantOverlapping shenanigans

	- The IsUnique boolean is part of the ConfigFile, so if you do not want a variant type to overlap with others, it's as simple as setting it to true.

	![](https://cdn.discordapp.com/attachments/850538397647110145/850541041084792862/6fb62ee5e6874be18105adf94d46a0a1.png)

	- Each Variant can either add Suffixes or Preffixes to an Enemy, this allows you to easily identify Variants on the spot with a simple ping, and getting an idea in your head on what they can do.

	![](https://cdn.discordapp.com/attachments/850538397647110145/850540614377799750/b0e937810c9b97f6093f88bb99957bc4.png)

### New Improved VariantHandler component/Variant Features

---

* Variants made in VarianceAPI have:

	- All the original features of the VariantHandler component, alongside...

	- Custom name prefixes and affixes

	- Wether the variant gives rewards

	- Configurable Rewards

	- New Tiers

	- Ability to add Completely custom Components

	- DeathState replacements

	- LightRendererInfos replacements

	- The Ability to use Equipments

	- Can spawn with Buffs/Debuffs (NYI)

### New VariantRewardHandler component.

---

* All of MonsterVariantsPlus' Rewards systems are now handled by this nifty little component that's added to each Variant that has the "GivesReward" boolean set to true.

* Only one VariantRewardHandler component is given to each Variant. the mod does it by checking how many VariantHandler components are active at the time.

* Just like in MonsterVariantsPlus, Variants by default now:

	- Drop extra Gold on death

	- Drop extra XP on death

	- Have a chance to drop items, Red, Green or White respectively

* Also just like in the original MonsterVariantsPlus, all of the rewards are calculated based off the Variant's Tier, and can be modified

* If youre not a fan or rewards, the VariantHandlerReward system can be easily turned off with a simple boolean in VarianceAPI's config file.

![](https://cdn.discordapp.com/attachments/850538397647110145/850543234753101894/6d175bdf30f1ce8784c994e20f60873d.png)

### Artifact of Variance

---

![](https://cdn.discordapp.com/attachments/850538397647110145/851543740484419604/VarianceEnabled.png)

* The Artifact of Variance, one of the Features from MonsterVariantsPlus, is now a core component of VarianeAPI.

* The Artifact of Variance, when enabled, multiplies all Variant's spawn rates by the amount specified in the Config file.

* Just like in the Original MonsterVariantsPlus, the Artifact of Variance can be enabled or disabled in the Config file.

### Intrinsic Items for Variants

---

* VarianceAPI comes bundled with KomradeSpectre's ItemModCreationBoilerplate.

* Due to this, VarianceAPI comes bundled with Intrinsic items that've been created for exclusive use for Variants.

* These items are AI Blacklisted, meaning that Enemies in the void fields will never get this item, and they should never appear in a normal run (if otherwise please contact me!)

* These items are...

		VAPI_GlobalCDR: Reduces all cooldowns by 1% linearly per stack

		VAPI_PrimaryCDR: Reduces the Primary's cooldown by 1% linearly per stack
		VAPI_SecondaryCDR: Reduces the Secondary's cooldown by 1% linearly per stack
		VAPI_UtilityCDR: Reduces the Utility's cooldown by 1% linearly per stack
		VAPI_SpecialCDR: Reduces the Special's cooldown by 1% linearly per stack

		VAPI_ExtraPrimary: Adds an extra primary use per stack
		VAPI_ExtraSecondary: Adds an extra secondary use per stack
		VAPI_ExtraUtility: Adds an extra utility use per stack
		VAPI_ExtraSpecial: Adds an extra utility use per stack

		VAPI_PurpleHealthbar: Makes the healthbar purple, automatically given to any variant who's tier is Uncommon or higher.

		VAPI_Plus1Crit: Increases critical strike chance by 1% per stack linearly.

## Official Variant Packs (Variant Packs made by Nebby)

---

### The Original 30

* The original 30 is a complete port of Rob's 30 original MonsterVariants.

* The original 30 includes QoL changes to variants, such as using VarianceAPI's intrinsic items, and having new features such as slight rebalancing and better override names.

* Get it here! (just click the icon!)

[![TO30 Icon](https://cdn.discordapp.com/attachments/850538397647110145/850546340403478528/icon.png)](https://thunderstore.io/package/Nebby/VariantPack_TheOriginal30/)

### Nebby's Wrath

* Nebby's Wrath is a complete port of all the non-"OtherVariants" of MonsterVariantsPlus.

* All the non "OtherVariants" encompass all variants except the ones for Squid Turrets, Empathy Cores & the Beetle Guards froms the Queen's Gland.

* Get it here! (just click the icon!)

[![NW Icon](https://cdn.discordapp.com/attachments/850538397647110145/854535054658895892/icon.png)](https://thunderstore.io/package/Nebby/VariantPack_NebbysWrath/)

### Nebby's New Friends

* Nebby's New Friends is a complete port of all the "OtherVariants" of MonsterVariantsPlus

* Currently the pack is nonexistent, but it will eventually come out.

* Get it here! (just click  the icon!)

[[REDACTED]](https://findtheinvisiblecow.com)

* Nebby still suggests checking out other VariantPacks made by the community! (Currently none :c)

---

### Official VarianceAPI discord server.

* VarianceAPI related discord server, you can join in here to meet other variant pack creators or learn how to create your own.

* Currently barebones, will start getting some love soon

https://discord.gg/kWn8T4fM5W


## Todo's

	- Continue development of the API


## Changelog
'0.8.0'

* Added Functionality to Legendary Variants (They'll announce their arrival in Chat.)

* Fixed bug that caused Variants with no VariantInventory to not recieve their purple healthbar if the tier was greater than common.

* Added Support for Modded Variants.

	- VariantConfig now has modGUID string.

	- This string MUST match the mod's internal GUID.

* VariantInfoHandler now has a failsafe when you attempt to add a Variant without the mod installed.

* Removed completely ItemInfos

'0.7.1'

* Changed how inventories work.

	* Inventories are no longer an Array of ItemInfos, instead, inventories are stored in the VariantInventory scriptable object.

		- a VariantInventory scriptable object consists of a itemStrings array, and an itemCount array.

		- The itemString's index must match the itemCount's index.

		- The lengths of both arrays MUST be the same.

	* Removed helpers that created ItemInfo Arrays, new helpers comming soon.

	* Due to this switch, ItemInfo[] is deprecated, but it will remain in VariantInfo so that people can switch to the VariantInventory scriptable Object.

	* ItemInfo will be removed on the next major update (0.8.0)

'0.7.0'

*  Added PrefabBase, a very simple prefab creation system used for creating Projectiles based off existing ones.

* Added missing R2API Submodule dependencies.

* Changes to the VariantInfo scriptable Object:

	- usesEquipment no longer exists.

* Internal changes to how VariantHandler is structured.

* Added a new Component, VariantEquipmentHandler.

	- Component is added automatically to the Variant when it detects that EquipmentInfo has an Equipment and is not null.

	- Component is used for Variants so they can use Equipment.

	- How the variant uses the Equipment is based off an Animation Curve

	- Special Thanks to TheMysticSword, since he helped me in using the AnimationCurve and most of the code is based off his code from AspectAbilities

* Changes to the EquipmentInfo scriptable object:

	- Now requires an AnimationCurve which tells when to use the Equipment.

'0.6.0'

* Added missing methods for Helpers.cs

	- Added method for creating VariantOverrideNames

	- Added methods for creating CustomVariantRewards

* Added the first iteration of the MyVariantPack boilerplate

	- Boilerplate is installed thru a unity package that you can install when youre developping in Thunderkit

	- Boilerplate code fully documented.

	- Boilerplate includes the following examples:

		- Creation of a Variant in code

		- Creating a Variant in Thunderkit

		- Communicating with VarianceAPI

		- Using the VariantInfoHandler to register variants

		- Using the VariantMaterialGrabber to grab vanilla materials.

* All logger messages now use the Bepinex Logger instead of the Unity Logger.

* Added KomradeSpectre's ItemModCreationBoilerplate to VarianceAPI.

	* Added a version for creating items in thunderkit, alongside the default one that uses R2API.

	* As listed above, VariantAPI comes now with Intrinsic variant items that are used in VariantCreation

* Fixed the VariantRewardHandler not being as close as possible as the original rewards system

* Added the Ability to replace Light colors in BaseLightRenderer infos of a CharacterModel.

* Changed config creation process, only the section version.

	- Now each config section follows the following format:

		*variantInfo.bodyName + " variants"

* Uncommon Variants now use a Purple healthbar instead of a Red healthbar thanks to the new intrinsic items.

'0.5.0'

* Added back the Artifact of Variance

* Fixed issue in VariantHandler causing certain stat multipliers not applying

* Added a PreventRecursion system. Variants may not recieve extra Dio's Best Friends when resurrecting.

* Added Custom Death States

	- Custom Death State can be specified in the VariantInfo Scriptable Object.

	- Leave it null unless you know how to get the required string to make it work.

* Removed no longer needed classes

* Added an identifier to VariantMaterialReplacement scriptable object.

	- It's main goal is to help with creating VariantMaterials in Thunderkit.

* Added the base class VariantMaterialGrabber.

	- Works by loading all the "incomplete" VariantMaterialReplacements in your AssetBundle.

		- An incomplete VariantMaterialReplacement has it's material set to null, and has it's identifier filled.

	- Proceeds to then compare the incomplete versions with complete ones made in code. if it matches one, it'll replace the material with the correct one.

	- TL;DR: This class helps reduce bloated AssetBundle sizes by allowing the player to fetch ingame materials instead of copying them and placing them in their AssetBundle.

'0.4.0'

* Changes to the OverrideName system

	- Added a new enum which enables the OverrideName to completely override the variant's baseName.

	- System now works with a switch

	- Renamed overrideOrder to overrideType

* Added a new Array in VariantInfo for VariantExtraComponents

* Added a new ScriptableObject called VariantExtraComponents.

	- VariantExtraComponents can be used to add a custom component to a specific variant when it spawns

	- This component *must* inherit from the new VariantComponent component found in the api (VariantComponent inherits from MonoBehaviour)

	Has the following settings:

		- string componentToAdd: The component to add to the Variant. this must be the combination of the Namespace of the component, alongside the class name. For example: TheOriginal30.VariantComponents.AddMissileLauncherToLemurian

		- bool isAesthetic: Wether the component to add just adds a mesh to the original body.

		- Non aesthetic components support haven't been added yet.

	- Used in TheOriginal30's Badass lemurian to attach the Missile Launcher.

* Added discord server to the ReadMe

* Hopefully fixed broken icon.

'0.3.0'

* All of VarianceAPI's ScriptableObjects have Headers and Tooltips, making it easier to create the objects in the UnityEditor

* Complete Rewrite of the Variant Overridename feature. now supporting VariantOverlaps.

* Did a Facelift of the Thunderstore page.

'0.2.0'

* VariantInfo now contains VariantConfig scriptable object, VariantConfig is used to create the config entries for your Variants.

	- VariantConfig allows you to:
		
		- Set the spawn chance of a Variant.

		- Wether the variant is unique or not.

* Removed VariantRegisterBase

* Added VariantInfoHandler, use this now to register your variants, as it streamlines the process.

* Added Helpers for creating VariantConfig Scriptable Objects in code, one for Vanilla entities and another one for Modded entities.

'0.1.1'

* Forgot to call the method that makes the config, whoops.

'0.1.0'

* Added the VariantRewardHandler Component, officially porting a good chunk of MonsterVariantPlus' Features.

* Added VariantRegisterBase, a helper for easily register variants made in Thunderkit.

* Added Config file with a lot of config entries for the VariantRewardHandler and global settings.

* Started working on a helper for creating Variant's Spawn Chances via config

* Determination++ After learning rob likes what i'm doing.

'0.0.2'

* Added Github Link.

* Made changes to the scriptable objects, now they can be made in Thunderkit instead of on RunTime.

'0.0.1'

* Initial Release