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
         * Target is to put the cursor right after we call "GetUserName", and store it's value on the local variable.
         * 
         * characterBody = bodyObject.GetComponent<CharacterBody>();
		 * if ((bool)characterBody)
		 * {
		 *      text = characterBody.GetUserName();
		 *      <---- ILHook goes here
		 * }
		 * 
		 * I think a better polace would be _after_ we get the text, and check if the body exists. That's where stuff like elite buffs, gummy clone and drone upgrade tiers are computed. But i have no ide ahow to match against that.
		 * string text2 = text;
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

            var success = cursor.TryGotoNext(x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.GetUserName)),
                x => x.MatchStloc(0));

            if (!success)
            {
                VAPILog.Fatal("Failed to hook RoR2.Util.GetBestBodyName! VariantNameProviders will not work!");
                return;
            }

            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate<Func<CharacterBody, string, string>>(FormatVariantName);
            cursor.Emit(OpCodes.Stloc_1);
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