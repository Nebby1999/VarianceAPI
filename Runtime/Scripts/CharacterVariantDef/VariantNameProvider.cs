#nullable enable
using RoR2;
using System;

namespace VAPI
{
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