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

	- Custom Equipment (NYI)

	- The Ability to use Equipments (NYI)

	- Can spawn with Buffs/Debuffs (NYI)

	- DeathState replacements (NYI)

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


### Official Variant Packs (Variant Packs made by Nebby)

---

* The Original 30

	* The original 30 is a complete port of Rob's 30 original MonsterVariants.

	* It's incomplete, but currently re-adds some of the Variants

	* Get it here! (just click the icon!)

	[![TO30 Icon](https://cdn.discordapp.com/attachments/850538397647110145/850546340403478528/icon.png)](https://thunderstore.io/package/Nebby/VariantPack_TheOriginal30/)

* Nebby still suggests checking out other VariantPacks made by the community! (Currently none :c)

## Todo's

	- Use KomradeSpectre's ItemModCreationBoilerplate to create a boilerplate for Variant Items.

		* Create said VariantItems so variants dont use Alienheads/Critglasses, and instead use these custom items

	- Re-Implement the Artifact of Variance

	- Create a system to replace variants' DeathState states.

	- Continue development of the API


## Changelog
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