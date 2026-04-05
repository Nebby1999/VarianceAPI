#nullable enable
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;

namespace VAPI
{
    internal static class VariantNameProviderHooks
    {
        [SystemInitializer]
        private static void Init()
        {
            IL.RoR2.Util.GetBestBodyName += GetBestBodyVariantName;
        }

        /*
         * We want to modify the name of the variant without directly overriding the character name string, so we're hooking GetBestBodyName.
         * 
         * Target is to put the cursor before the characterBody.isElite statement.
	     * if ((bool)characterBody)
	     * {
	     * <---- ILHook goes here
		 *    if (characterBody.isElite)
		 *    {
		 *       BuffIndex[] eliteBuffIndices = BuffCatalog.eliteBuffIndices;
	     *       foreach (BuffIndex buffIndex in eliteBuffIndices)
		 *       {
		 *   	    if (characterBody.HasBuff(buffIndex))
		 *		    {
		 *              text2 = Language.GetStringFormatted(BuffCatalog.GetBuffDef(buffIndex).eliteDef.modifierToken, text2);
		 *          }
		 *       }
		 *    }
         */
        private static void GetBestBodyVariantName(MonoMod.Cil.ILContext il)
        {
            var cursor = new ILCursor(il);

            //We first deduce the text2 variable from the code.
            //text2 = Language.GetStringFormatted(...);
            int text2LocIndex = -1;
            bool text2LocIndexObtained = cursor.TryGotoNext(x => x.MatchCallOrCallvirt<Language>(nameof(Language.GetStringFormatted))) &&
                cursor.TryGotoNext(x => x.MatchStloc(out text2LocIndex));
            if (!text2LocIndexObtained)
            {
                VAPILog.Fatal($"Failed to ILHook RoR2.Util.GetBestBodyName()! Unable to deduce \"text2\" local variable index. VariantNameProviders will not work!");
                return;
            }

            //The we go to `if(characterBody.isElite)` to deduce the body variable, if we match, proceed with the rest of the hook.
            int characterBodyLocIndex = -1;
            cursor.Index = 0;
            bool characterBodyLocIndexObtained = cursor.TryGotoNext(x => x.MatchLdloc(out characterBodyLocIndex),
                x => x.MatchCallOrCallvirt<CharacterBody>($"get_{nameof(CharacterBody.isElite)}"));
            if (!characterBodyLocIndexObtained)
            {
                VAPILog.Fatal("Failed to hook RoR2.Util.GetBestBodyName()! Unable to deduce \"characterBody\" local variable index. VariantNameProviders will not work!");
                return;
            }

            //Emit CharacterBody and Text2 values.
            cursor.Emit(OpCodes.Ldloc, characterBodyLocIndex);
            cursor.Emit(OpCodes.Ldloc, text2LocIndex);

            //Emit delegate
            cursor.EmitDelegate<Func<CharacterBody, string, string>>(FormatVariantName);

            //Store new text in text2
            cursor.Emit(OpCodes.Stloc, text2LocIndex);
        }

        private static string FormatVariantName(CharacterBody body, string bodyName)
        {
            if(body.TryGetComponent<CharacterBodyVariantController>(out var characterBodyVariantController))
            {
                for(int i = 0; i < characterBodyVariantController.characterVariantDefs.Length; i++)
                {
                    CharacterVariantDef variantDef = characterBodyVariantController.characterVariantDefs[i];
                    bodyName = variantDef.variantNameProvider?.GetVariantName(bodyName) ?? bodyName;
                }
            }

            return bodyName;
        }
    }
    public interface IVariantNameProvider : IValidatable
    {
        public string GetVariantName(string input);
    }

    [Serializable]
    public struct VariantNameFormatter : IVariantNameProvider
    {
        public string? nameToken;

        public string GetVariantName(string input)
        {
            if(string.IsNullOrWhiteSpace(nameToken))
            {
                return input;
            }

            return Language.GetStringFormatted(nameToken, input);
        }

        public void Validate() { }
    }

    [Serializable]
    public struct VariantNamePrefix : IVariantNameProvider
    {
        public string? prefixToken;

        public string GetVariantName(string input)
        {
            if(string.IsNullOrWhiteSpace(prefixToken))
            {
                return input;
            }
            return string.Format("{0} {1}", Language.GetString(prefixToken), input);
        }
        public void Validate() { }
    }

    [Serializable]
    public struct VariantNameSuffix : IVariantNameProvider
    {
        public string? suffixToken;

        public string GetVariantName(string input)
        {
            if (string.IsNullOrWhiteSpace(suffixToken))
            {
                return input;
            }

            return string.Format("{0} {1}", input, Language.GetString(suffixToken));
        }
        public void Validate() { }
    }

    [Serializable]
    public struct VariantNameOverride : IVariantNameProvider
    {
        public string? overrideToken;
        public string GetVariantName(string input)
        {
            if(string.IsNullOrWhiteSpace(overrideToken))
            {
                return input;
            }
            return Language.GetString(overrideToken);
        }
        public void Validate() { }
    }
}