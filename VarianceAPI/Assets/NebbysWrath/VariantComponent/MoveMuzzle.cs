using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VarianceAPI.Components;

namespace NebbysWrath.VariantComponents
{
    public class MoveMuzzle : VariantComponent
    {
        private CharacterModel model;
        private ChildLocator childLocator;
        private ParticleSystem particleSystem;
        private ParticleSystemRenderer particleSystemRenderer;

        private void Start()
        {
            this.model = base.GetComponent<CharacterModel>();
            this.childLocator = base.GetComponentInChildren<ChildLocator>();
            var muzzle = childLocator.FindChild("MuzzleMouth");
            muzzle.transform.localPosition = new Vector3(-1f, 2f, 0f);

            FixShit();
        }
        private void FixShit()
        {
            if(this.model)
            {
                GameObject lightPrefab = Instantiate<GameObject>(MainClass.nebbysWrathAssets.LoadAsset<GameObject>("SilicateLemmyLight"), childLocator.FindChild("Head"));
                lightPrefab.transform.localPosition = new Vector3(-1, 2f, 0);

                GameObject stoneFlamePrefab = Instantiate<GameObject>(MainClass.nebbysWrathAssets.LoadAsset<GameObject>("SilicateLemmyFire"), childLocator.FindChild("Hip").Find("tail1/tail2/tail3/tail4/tail4_end"));
                stoneFlamePrefab.transform.localPosition = new Vector3(0f, -2f, 0f);
                stoneFlamePrefab.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            }
        }
    }
}
