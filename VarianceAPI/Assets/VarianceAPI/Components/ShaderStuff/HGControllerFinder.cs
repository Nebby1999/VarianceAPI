using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VarianceAPI.Components
{
    public class HGControllerFinder : MonoBehaviour
    {
        public Renderer renderer;
        public Material material;

        public void OnEnable()
        {
            if (renderer && material)
            {
                renderer.material = material;
                renderer.sharedMaterials[0] = material;
                switch (material.shader.name)
                {
                    case "Hopoo Games/Deferred/Standard":
                        var standardController = gameObject.AddComponent<MaterialControllerComponents.HGStandardController>();
                        standardController.material = material;
                        standardController.renderer = renderer;
                        break;
                    case "Hopoo Games/FX/Cloud Remap":
                        var cloudController = gameObject.AddComponent<MaterialControllerComponents.HGCloudRemapController>();
                        cloudController.material = material;
                        cloudController.renderer = renderer;
                        break;
                    case "Hopoo Games/FX/Cloud Intersection Remap":
                        var intersectionController = gameObject.AddComponent<MaterialControllerComponents.HGIntersectionController>();
                        intersectionController.material = material;
                        intersectionController.renderer = renderer;
                        break;
                    case "Hopoo Games/FX/Solid Parallax":
                        var parallaxController = gameObject.AddComponent<MaterialControllerComponents.HGSolidParallaxController>();
                        parallaxController.material = material;
                        parallaxController.renderer = renderer;
                        break;
                    case "Hopoo Games/Deferred/Wavy Cloth":
                        var clothController = gameObject.AddComponent<MaterialControllerComponents.HGWavyClothController>();
                        clothController.material = material;
                        clothController.renderer = renderer;
                        break;
                    default:
                        enabled = false;
                        return;
                }
                Destroy(this);
            }
            else
                enabled = false;
        }
    }
}