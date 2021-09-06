
# VarianceAPI

* A complete Rewrite of Rob's original MonsterVariants concept.

* VarianceAPI aims to combine the Variants Features from MonsterVariants and MonsterVariantsPlus into a single mod.

* The API by itself doesnt do much, it only works as a base for others to create their own variants.

* After being in development for a few months, VarianceAPI has now released its 1.0 version, including greatly different Workflow with less editor clutter and more smooth experience. For both variant creators and players alike.

## Features

### Thunderkit Ease of Development.

---

* VarianceAPI was made with Thunderkit in mind, and a lot of its code has been written to be used inside it. (Scriptable objects, editor only code, SO in Assetbundles, etc.) As such, it is extremely recommended to use Thunderkit when developing VariantPacks

* ~~Despite this, Variants can still be added via code.~~ Sadly there is no longer a class for making Scriptable objects and helpers in code, this is mainly due to difficulties testing said methods due to myself prefering to work in Thunderkit. However, *Anyone* is free to create their own methods to make these, and if you'd like, you can even send the class my way and I'll gladly add it to the official DLL.

![](https://cdn.discordapp.com/attachments/850538397647110145/882995193631096902/c19625fdbe2ab1289e55a35742a4696c.png)

* Easily create Variants using a *Single* scriptable object! No longer do you need 4 to make a simple Variant. Thanks to FixPluginsTypeSerialization mod, VariantInfo now uses structs! allowing for a more smooth experience.

![](https://cdn.discordapp.com/attachments/850538397647110145/882995736097210449/ad7618b09f2c1a07dcd63a79d55cc75d.png)

* Register your Variants easily now by accesing the VariantRegister static class! no longer do you need to do some black magic bullshit to register your variants, simply access the class and give it your AssetBundle or a List with VariantInfos. Give it your mod's ConfigFile and it'll automatically create Config for each variant's spawn rate and wether theyre Unique or not.

        VarianceAPI.VariantRegister.AddVariant(nebbysWrathAssets, Config);

* The same applies now to the VariantMaterialGrabber, you can no longer inherit from it but now its static, and all you need to do is give it a List with a tuple for the identifier, and the material itself! an example can be found [here](https://github.com/Nebby1999/VarianceAPI/blob/VarianceAPI2.0/VarianceAPI/Assets/NebbysWrath/Modules/MaterialGrabber.cs)

* Oh, did i forget to mention that _The Variant Register now Registers Variants when the Game Loads, And that the IsUnique mechanic now properly works?_ No more shall a playable monster suddenly turn into a variant or isUnique not work anymore!

* Also, VarianceAPI now comes with the VAPIEditor assembly. an Editor only assembly that, when loaded into thunderkit, will modify your editor for ease of use on creating variants. including:

- A custom Drawer for entity states made by KevinFromHPCustomerService

- A custom Drawer for VariantComponents.

- A migrator tool that'll easily let you migrate from the old scriptable object system to the new one.

### Documentation

---

* A lot of internal changes have been done to VarianceAPI on the 1.0.0 update, as such, most of the documentation found in the Github's wiki page no longer holds true.

* Nebby is hard at work writing the new documentation of the scriptable objects.

### Better Variant Overlapping methods

---

* The confusing and sometimes obscure VariantOverlap system has been expanded and now can be either Avoided, or Encouraged.

    - Each Variant has a Unique boolean, which determines wether the variant can participate in VariantOverlapping shenanigans.

    - The IsUnique boolean is part of the ConfigFile, just set it to true if you dont want this feature with a specific variant.

    ![](https://cdn.discordapp.com/attachments/850538397647110145/850541041084792862/6fb62ee5e6874be18105adf94d46a0a1.png)

    - Each Variant can either add Suffixes, Prefixes or Completely override an enemy's name. this allows you to easily identify a Variant on the spot by pinging, and getting an idea in your head on what they can do.

    ![](https://cdn.discordapp.com/attachments/850538397647110145/850540614377799750/b0e937810c9b97f6093f88bb99957bc4.png)

    - Since version 1.0, Variant name overrides expects a Language token.

### New VariantSpawnHandler component.

---

All the logic behind how Variants spawns have been re-written into the VariantSpawnHandler component. thanks to this separate component calculating the variants spawned, now the IsUnique system properly works. by first rolling for weighted unique variant infos, and then rolling for the not unique ones.

After the variants have been chosen, the Spawn handler gives the enabled infos to the variant handler component and the variant reward handler component, and runs Modify(). officially turning the body into a variant.

### New Improved VariantHandler component

---

Now a single VariantHandler component exists per variant. the component calculates the new stats, skills, materials, size all in the spot depending on what VariantInfos are fed thru.

* Variants made in VarianceAPI have:

    - All the original features of the VariantHandler component, alongside...

	- Custom name prefixes and suffixes

	- New Tiers

	- Ability to add Completely custom Components to the model, body, or master.

	- DeathState replacements

	- LightRendererInfos replacements

	- The Ability to use Equipments

	- Can spawn with Buffs/Debuffs

	- Complex MeshSwaps

### VariantRewardHandler component.

---

* All of MonsterVariantsPlus's Rewards systems are now handled by this component that's added to each body on load time.

* Only one VariantRewardHandler component is given to each variant. The mod automatically calculates the new rewards by the VariantInfos given by the VariantSpawnHandler component.

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

* It also comes with an Artifact Code for use with ArtifactCodeAPI. the code can be found [here](https://cdn.discordapp.com/attachments/753709254803980296/883012753839771678/unknown.png)

### Intrinsic Items for Variants

---

* VarianceAPI comes bundled with Intrinsic items that've been created for exclusive use for Variants.

* These items are AI Blacklisted, meaning that Enemies in the void fields will never get these items, and they should never appear in a normal run (if otherwise please contact me!)

* These items are...

		GlobalCDR: Reduces all cooldowns by 1% linearly per stack

		PrimaryCDR: Reduces the Primary's cooldown by 1% linearly per stack
		SecondaryCDR: Reduces the Secondary's cooldown by 1% linearly per stack
		UtilityCDR: Reduces the Utility's cooldown by 1% linearly per stack
		SpecialCDR: Reduces the Special's cooldown by 1% linearly per stack

		ExtraPrimary: Adds an extra primary use per stack
		ExtraSecondary: Adds an extra secondary use per stack
		ExtraUtility: Adds an extra utility use per stack
		ExtraSpecial: Adds an extra utility use per stack

		PurpleHealthbar: Makes the healthbar purple, automatically given to any variant who's tier is Uncommon or higher.

		Plus1Crit: Increases critical strike chance by 1% per stack linearly.

* These items now come with icons to easily identify them on the spot, the icon textures where made by Wonda, publisher of [Refightilization](https://thunderstore.io/package/Wonda/Refightilization/)

![](https://i.gyazo.com/fb550e23f46fdd99142271796feb8465.png)

### Console Commands

---

If you have [DebugToolKit](https://thunderstore.io/package/IHarbHD/DebugToolkit/) Installed, you'll have access to developer commands for testing out variants. these commands are:

*list_modified_bodies*. Lists all the bodies that have variants.

*list_variants_from_body*. Lists all the variantInfos that are inside a body, gives out the VariantInfo's name alongside the identifier.

        Argument 1: The internal name of the body prefab

*spawn_variant*. Spawns an enemy with the desired variantInfos attached. to get the possible variantInfos, use the command *list_variants_from_body.

        Argument 1: The internal name of the characterBody's master prefab.
        Argument 2 - Infinity: The VariantInfos to use. must be the VariantInfo's identifier.

*spawn_as_variants*. Spawns the person who wrote the command as a desired variant. to get the possible variantInfos, use the command *list_variants_from_body*

        Argument 1: The internal name of the body prefab.
        Argument 2 - Infinity: The VariantInfos to use. must be the Variantinfo's identifier.
        

## Official Variant Packs (Variant Packs made by Nebby)

### The Original 30

* The original 30 is a complete port of Rob's 30 original MonsterVariants.

* The original 30 includes QoL changes to variants, such as using VarianceAPI's intrinsic items, and having new features such as slight rebalancing and better override names.

* Get it here! (just click the icon!)

[![TO30 Icon](https://cdn.discordapp.com/attachments/850538397647110145/850546340403478528/icon.png)](https://thunderstore.io/package/Nebby/VariantPack_TheOriginal30/)

### Nebby's Wrath

* Nebby's Wrath is a complete port of all the non-"OtherVariants" of MonsterVariantsPlus.

* All the non "OtherVariants" encompass all variants except the ones for Squid Turrets, Empathy Cores & the Beetle Guards from the Queen's Gland.

* Get it here! (just click the icon!)

[![NW Icon](https://cdn.discordapp.com/attachments/850538397647110145/854535054658895892/icon.png)](https://thunderstore.io/package/Nebby/VariantPack_NebbysWrath/)

### Nebby's New Friends

* Nebby's New Friends is a complete port of all the "OtherVariants" of MonsterVariantsPlus

* Currently the pack is nonexistent, but it will eventually come out.

* Get it here! (just click  the icon!)

[[REDACTED]](https://findtheinvisiblecow.com)

* Bellow is a list of community made VariantPacks

	- [HIFUVariants](https://thunderstore.io/package/HIFU/HIFUVariants/)

---

### Official Nebby's Mods discord server.

* If you wish to contact me about my risk of rain mods, you can do so here in this Discord server.

https://discord.gg/kWn8T4fM5W

---

### Special thanks.

* Kevin for the EntityStateDrawer, which was used as a base for the component drawer. (And making me not use thunderkit like an ape)

* IDeathHD and Harb, for making DebugToolkit and it's spawn_ai and spawn_as commands (used for the spawn_variant and spawn_as_variant)

* IDeathHD for helping me point towards a general direction with networking.

* Aaron, Gaforb, "come on and SLAM", & especially TheTimeSweeper ~~Love you habibi~~ for helping me with networking issues.

* Aaron for creating a weighted selection for the Unique variants.

* Dotflare for making the Variance artifact token and other tidbits from the official variant packs.

* Twiner for Thunderkit and helping me a lot with certain editor scripts.

* Rob for creating MonsterVariants.

# Changelog

## '1.1.1' - Too tired

* Fixed a null ref exception caused by a mistake in the spawn handler.

## '1.1.0' - Bugfixing Galore

* Updated Website

* VariantSpawnHandler:
	
	- Rewritten from the ground up with networking in mind
	- Fixed an issue where the handler would assign wrong variant infos, potentially causing certain variants to "ignore" their spawn rates or certain variants being incredibly common.
	- VariantInfos array is now get, internal set.
	- Added a bool called "customSpawning". setting this to true will return the moment Start() runs.
	- This can be used to later set specific variantInfos for specific spawning of variants on entity states.

* VariantHandler
	
	- NetworkServer.Active checks to avoid clogging the client's log with messages regarding client trying to run server only code.
	- This check includes a check on setting the variant back to max health. clients running this line of code caused the variants on their side to have its hurtboxes destroyed.
	- (Basically, Variants should no longer be immune to clients and only take damage from host's attacks / projectiles)
	- Added a config option to enable/disable variant arrival announcements.
	- No longer merges all the variantInfos. this is done to avoid any kind of issue regarding merging.


* VariantRegister

	- Variant register now sends a message to the log when its material dictionary or registered variants dictionary is empty. saying that no changes will be made

## '1.0.0' - First Complete Release

* Complete rewrite of the variants system.

* Deprecated the following scriptable objects.

     - Custom Variant Reward
	- Equipment Info
	- BuffInfo
	- VariantConfig
	- Variant Extra Component
	- Variant Info (Old)
	- Variant Inventory
	- Variant Light Replacement
	- Variant Material Replacement
	- Variant Mesh Replacement
	- Variant Override Name
	- Variant Skill Replacement

- Added new Scriptable objects to replace the old ones

	- VariantInfo:
		- Variant Override Names are now stored directly here, as an Array Struct.
		- Variant Override names now expect a language token.
		-  _
		- Variant Skill Replacements are now stored directly here, as an Array Structs.
		- _
		- Variant Extra Components are now stored directly here, as an Array Struct.
		- Component is now a Serializable Variant Component Type (The component's fully qualified name)
		- Components can now be added to the Master, Body or Model game objects.
		- the Spawn rate and the IsUnique configs are made directly with the information inside the Variant Info.
		- Arrival messages now expect a languafge token
		- Death state replacements now are Serializable Entity State Type.
	- VariantInventoryInfo:
		- Merge of BuffInfo, EquipmentInfo and VariantInventory scriptable objects.
		- Variant item inventories are now a struct of string (itemDef name) and int (amount)
		- VariantBuffs are now a struct of string (buff def name), float (time, setting this to 0 makes it permanent) and int (amount)
		- EquipmentInfo is unchanged, still made with just an Equipment string and Animation curve.
	* VariantVisualModifier
		* Merge of VariantLightReplacement, MaterialReplacement and MeshReplacement
		* Each of these are held as an array of struct.
		* Light replacements can now change the type of light.

* Deprecated the original VariantHandler component, split the spawning logic into VariantSpawnHandler.
* Deprecated VariantInfoHandler, replaced by VariantRegister.
* Deprecated VariantMaterialGrabber, replaced by a new version with the same name
* Added icons for the Items
* Added Console Commands
* Prolly a lot of things I Forgot

(Pre 1.0.0 changelog can be found [here](https://github.com/Nebby1999/VarianceAPI/blob/main/VarianceAPI/Assets/VarianceAPI/ChangelogOld.md))