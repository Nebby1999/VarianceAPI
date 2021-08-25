using System;
using RoR2;
using UnityEngine;
using VarianceAPI.Components;

namespace TheOriginal30.VariantComponents
{
    public class AddMissileLauncherToLemurian : VariantComponent
    {
        private CharacterModel model;
        private ChildLocator childLocator;

        private void Start()
        {
            this.model = base.GetComponentInChildren<CharacterModel>();
            this.childLocator = base.GetComponentInChildren<ChildLocator>();

            this.AddMissileLauncher();
        }

        private void AddMissileLauncher()
        {
            if (this.model)
            {
                GameObject missileLauncher = UnityEngine.Object.Instantiate<GameObject>(MainClass.missileLauncherDisplayPrefab, this.childLocator.FindChild("Chest"));
                missileLauncher.transform.localPosition = new Vector3(0, 0, 1.75f);
                missileLauncher.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0, 0));
                missileLauncher.transform.localScale = Vector3.one * 8f;
            }
        }
    }
}