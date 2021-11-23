using RoR2;
using RoR2.CharacterAI;
using System.Linq;
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
