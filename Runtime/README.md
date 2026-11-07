# VarianceAPI

VarianceAPI is a library designed to add "Character Variants" to existing Characters in the game.

The original concept can be found [here]() by Rob, the mod's features plus this [addon]()'s features merged together to become VarianceAPI.

Want a Lemurian that's twice the size and spits out lasers? Want a golem that's blazing fast and spawns mini golems when clapping? All of this and more are possible with VarianceAPI.

## Features

### Flexible Development Environment

VarianceAPI has support for developing new Variants either inside the Unity Editor utilizing Thunderkit, or entirely at Runtime.

Support for creating variants entirely at runtime is via the utilization of the ``CharacterVariantDefBuilder``.

Regardless if you utilize Thunderkit or not, VarianceAPI can be used in your workflow.

### Persistent CharacterVariantDefs

``CharacterVariantDefs`` are persistent for a body, once a Body spawns, and has available variants. The ``VariantSpawnManager`` will roll ``CharacterVariantDefs`` to apply to the body.

Once the CharacterVariantDefs are applied, if the body has a master they'll be stored on the ``CharacterMasterVariantStorage``, and they'll be set in stone until the Master is destroyed.

This allows for Devoted Lemurians to keep the CharacterVariantDefs assigned to them, alongside more interactions with other mods.

### Lobby Support

When you create new Variants, they're added to ``VariantPacks``. An end user can afterwards disable or enable your pack at will, similar to ExpansionDefs.

![GIF]

Furthermore, individual variants can be toggled on and off further if enabled in the config file

![gif]


### Powered by Serialize Reference

VarianceAPI 3.0.0 utilizes the [SerializeReference](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/SerializeReference.html) attribute to its fullest potential. While VarianceAPI comes bundled with existing implementations of SerializableReferences, you can create your own implementations and use them on your own VariantPacks.

* ``IVariantMasterModifier``: Interface for modifying a ``CharacterMaster`` when the variant is applied. __Built in modifiers:__
    * ``UnstableAIModifier``: Forces the AI  to use it's "Low Health Attacks" regardless of their actual health fraction
    * ``AlwaysSprintAIModifier``: Forces the AI to sprint always
    * ``AISkillDriverAimVectorModifier``: Allows the modification of a specific ``AISkillDriver``'s aim vector damp
    * ``BaseAIDampModifier``: Allows the modification of an entire AI's aim vector damp
    * ``ReplaceSkillDrivers``: Allows for the complete replacement of a ``CharacterMaster``'s ``AISkillDriver``s
* ``IVariantBodyModifier``: Interface for modifying a CharacterBody when the variant is applied. __Built in Modifiers:__
    * ``LoadCharacterSoundbank``: Interface for loading a Character's Soundbank when the variant is applied
* ``IVariantNameProvider``: Interface for modifying a CharacterBody's display name while the variant is applied. __Built in Providers:__
    * ``IVariantNameFormatter``: Uses a format token to retrieve the variant's name
    * ``VariantNamePrefix``: Appends a token at the beginning of the character's name
    * ``VariantNameSuffix``: Appends a token at the end of the character's name
    * ``VariantNameOverride``: Replaces a character's name
* ``IVariantStatModifier``: Interface for creating stat modifications for the CharacterBody while the variant is applied. These stat modifications are driven by R2API's RecalculatestatsAPI. __Built in Modifiers:__
    * ``BasicStatModifiers``: Reimplementation of VarianceAPI 2.0's stat modifiers

### Scripted Variant Spawns

VarianceAPI comes bundled with three different ways to simplify the spawns of monsters, you can utilize these to ensure a Monster being summoned or spawned is a variant or not.

* ``VariantMasterSummon`` - Wraps an existing ``MasterSummon`` instance and modifies the spawned instance to become a variant
* ``VariantDirectorSpawnRequest`` - Wraps an existing ``DirectorSpawnRequest`` instance and modifies the spawned instance to become a variant
* ``VAPIUtils.TransformSpawnedInstanceIntoVariant()`` - Used to transform any spawned instance into a variant

### Console Commands

The following console commands can be used to test your variants in development:

* ``vapi_list_bodies``: Lists all the CharacterBodies that have Variants
* ``vapi_list_body_variants``: Lists all the CharacterVariantDefs assigned to a specific CharacterBody
* ``vapi_spawn_ai``: Summons a CharacterMaster with the specified Variants.
* ``vapi_spawn_as``: Respawns the sender as the specified CharacterBody with the specified Variants.


[![ko-fi](MarkdownFiles~/Readme/KofiButton.png)](https://ko-fi.com/nebby1999)

## Official Variant Packs

(Note: click the icon to open a new tab to the variant pack)
| Icon/URL | Name | Description |
|--|--|--|
| [![TO30Icon](MarkdownFiles~/Readme/TO30Icon.png)](https://thunderstore.io/c/riskofrain2/p/Nebby/VariantPack_TheOriginal30) | The Original 30 | The Original Thirty Variants developed by Rob, ported and modified to VarianceAPI. |
| [![NWIcon](MarkdownFiles~/Readme/NWIcon.png)](https://thunderstore.io/c/riskofrain2/p/Nebby/VariantPack_NebbysWrath/) | Nebby's Wrath | 87 Variants spread across Base Game, Survivors of the Void, Seekers of the Storm and Alloyed Collective enemies. |

## Donations

VarianceAPI is made with no AI assistance and is completely free.

If you wish to support the work I do, consider buying me a Ko-fi, button below.

[![ko-fi](KOFI_BUTTON)](https://ko-fi.com/nebby1999)

## Special Thanks

* KevinFromHPCustomerService for teaching me how to use Thunderkit way back in the day.
* IDeathHD and Harb, for making DebugToolkit and it's ``spawn_ai`` and ``spawn_as`` commands (used for ``vapi_spawn_ai`` and ``vapi_spawn_as`` commands)
* IDeathHD and Chinchi for general networking help
* Aaron, Gaforb, ``come on and SLAM``, EarthZebra and especially TheTimeSweeper ~~ly habibi~~ for helping me with networking issues.
* Aaron for helping with the creation of the WeightedSelection roll for unique variants
* Dotflare for making the Variance Artifact assets
* PassivePicasso for the development of Thunderkit
* Rob for creating Monster Variants.
* Papa zach for creating the Artifact & Expansion Icons
* Mackysoft for letting me use and redistribute his "SerializeReferenceExtensions" package.
* GrooveSalad for his ExpansionManager code, which was used as a base to create the variant rulebook categories
* MysticalChicken and Zenithrium for feedback regarding tokens