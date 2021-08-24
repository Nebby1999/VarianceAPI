using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VarianceAPI.Modules
{
    internal static class Interfaces
    {
        internal static void Initialize()
        {
            IL.RoR2.GlobalEventManager.OnCharacterDeath += IL_ServerKilledOtherPatch;
            IL.RoR2.HealthComponent.TakeDamage += IL_OnIncomingDamageOther;
        }

        private static void IL_ServerKilledOtherPatch(ILContext il)
        {
            var c = new ILCursor(il);

            //This gets the code on line 729 in dnspy. 
            ILLabel endIfLabel = null;
            bool flag = c.TryGotoNext(
                x => x.MatchLdloc(12),
                x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit"),
                x => x.MatchBrfalse(out endIfLabel));
            if (flag)
            {
                c.Emit(OpCodes.Ldc_I4_0);
                c.Emit(OpCodes.Brfalse, endIfLabel);
            }
            else
                VAPILog.LogE("Errors: IL Instruction Not found. Skipping.");

            // This changes that line of code from
            // if (attacker)
            // to be
            // if (false == true && attacker)
        }


        ///<summary>Adds the IOnIncomingDamageOtherServerReciever interface.</summary>
        private static void IL_OnIncomingDamageOther(ILContext il)
        {
            var c = new ILCursor(il);


            //This takes us to line 492 in dnspy, the first mention of IOnIncomingDamageServerReciever
            bool flag = c.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>("onIncomingDamageReceivers"));

            if (flag)
            {
                ILLabel label = c.DefineLabel();
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Action<HealthComponent, DamageInfo>>(RunOnIncomingDamageOther);
            }
            else
                VAPILog.LogE("Errors: IL Instruction Not found. Skipping.");
        }

        private static void RunOnIncomingDamageOther(HealthComponent healthComponent, DamageInfo damageInfo)
        {
            if (!damageInfo.attacker)
            {
                return;
            }
            IOnIncomingDamageOtherServerReciever[] interfaces = damageInfo.attacker.GetComponents<IOnIncomingDamageOtherServerReciever>();
            for (int i = 0; i < interfaces.Length; i++)
            {
                interfaces[i].OnIncomingDamageOther(damageInfo);
            }
        }
    }
}
