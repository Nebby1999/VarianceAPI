## Changelog
'0.9.0'

* Changes to VariantInfo & Variant Handler:

	- VariantInfos with no VariantConfig assigned now properly register instead of crashing the mod

	- Enabled VariantMeshReplacements. Currently undocumented, guide will appear eventually since theyre difficult to implement.

	- Enabled VariantBuffs.

		- Variants can be given a buff when they spawn.

		- Buff can be permanent, or expiring on a timer.

	- SpawnRate and IsUnique are now hidden from the inspector when using unity, since theyre set on the VariantConfig.

	- Arrival Messages now apply for both Rare and Legendary variants.

		- If no custom arrival message is given, it uses a generic spawn string.

	- The VariantHandler component now catches when certain mistakes happens and lets you know on the console screen

	- Moved basically all the code from start to its separate method, so it can be called from other places.

* Changes to CustomVariantReward & VariantRewardHandler

	- Cleaned up code

	- Added ItemList for VariantRewardHandler

		- Custom variant reward now can specify what items can be droped from a variant.

* Changes to VariantBuff:

	- VariantBuff now actually works

	- Variants can now be given a buff that lasts permanently, or lasts a certain amount of time

* Changes to VariantHandler component:

	- The component now catches when certain mistakes happens and lets you know on the Console screen.

* Legendary Variant's XP Multiplier is now configurable.

* Fixed bugs that would cause VariantHandler components to be in certain character mods. such as Playable Templar or Tymmey's Lemurian/Imp/Exploder

* Added MeshType Enum, used on Mesh Replacements

* Added Documentation on ScriptableObjects in the Github's Wiki.

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

	* As listed above, VarianceAPI comes now with Intrinsic variant items that are used in VariantCreation

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