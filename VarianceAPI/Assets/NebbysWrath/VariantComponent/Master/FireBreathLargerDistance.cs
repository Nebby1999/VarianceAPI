using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VarianceAPI.Components;

namespace NebbysWrath.VariantComponents
{
    public class FireBreathLargerDistance : VariantComponent
    {
        public CharacterMaster master;

        public void Awake()
        {
            master = GetComponent<CharacterMaster>();
            AISkillDriver flameBreath = GetComponents<AISkillDriver>().Where(x => x.customName == "Flamebreath").First();

            flameBreath.maxDistance *= 4;

            Destroy(this);
        }
    }
}
