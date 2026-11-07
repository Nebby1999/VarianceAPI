# '3.0.0' - Long Term Support

* This release marks the first LTS support for VarianceAPI.
* A complete rewrite of the codebase has been done to address issues and improve the health of the project
* There are _a lot_ of changes that are very likely undocumented (given that it was a complete rewrite), but below should be the key points.
* Regardless, VAPI comes with built in documentation using XML, so that should help out :)

## General Changes

* __Thunderkit is no longer required to make new Variants.__
* Added dependency to "Mackysoft's SerializeReferenceExtensions"
  * This dependency is used to replace the plethora of ScriptableObjects VAPI required, the amount went from 7 to just 3
  * This dependency is required to be installed from the github repository if using thunderkit, otherwise the runtime redistribution marked as VAPI's dependency can be used for code only Variants.
* all classes are marked as sealed, except for the ones that are intended to be inherited from.
* The following ScriptableObjects have been replaced/deleted
    * ``VariantDef`` -> ``CharacterVariantDef`` (Scriptable Object)
    * ``VariantTierDef`` -> ``CharacterVariantTierDef`` (ScriptableObject)
    * ``VariantInventory`` -> (Items and Equipment: ``VariantInventoryDefintion`` (C# class)) - (Buffs: ``VariantBuffStorage`` (C# class))
    * ``VariantPackDef`` -> ``VariantPack`` (C# class)
    * ``VariantSizeModifier`` -> ``float`` field in ```CharacterVariantDef``
    * ``VariantSpawnCondition`` -> ``IVariantSpawnCondition`` (SerializableReference interface)
    * ``VariantVisuals`` -> ``CharacterVariantVisualModifier`` (ScriptableObject)
* Referencing base game assets (such as ``EquipmentDef``, ``ItemDef``, ``CharacterBody`` is done via R2API's AddressReferencedAssets)

## Runtime Changes

* ``CharacterVariantDefs`` are now __persistent__ on a body
    * Once the Body rolls the ``CharacterVariantDefs``, they're stored in the ``CharacterMaster`` object.
    * Next time the Body spawns, the ``CharacterVariantDefs`` stored in the ``CharacterMaster`` object are used instead of rolling for new ones
    * Modifications applied to the master are unapplied if the Body's index changes
* Added support for creating ``CharacterVariantDef``s at runtime utilizing the ``CharacterVariantDefBuilder``
    * Contains chainable methods for defining new character variants
* Added basic cloning support for ``CharacterVariantDef``s
    * Creating a Devoted Lemurian variant from an existing Lemurian variant is as simple as calling ``VariantCloning.CloneIntoDevotedLemurian``

## Editor Changes

* VariantPacks are no longer ScriptableObjects and instead are C# classes similar to ContentPacks
* There's Wizards for upgrading your old scriptable objects to the new ones:
    * ``Tools/VAPI/VariantPack ScriptableObject Migration Wizard``: Used to migrate from VAPI 2.0 to VAPI 3.0 Scriptable Objects
    * ``Tools/VAPI/Migrate AddressReferencedAsset Catalog Loads to GUID Loads``: Used to migrate from referencing items/buffs/equipments/characters from Catalog names to GUIDS.
* Mackysoft's SerializeReferenceExtensions is required to utilize the new interface setups explained on the Runtime Changes section.