using HarmonyLib;

using UnityEngine;

using UnboundLib;

using GearUpCards.MonoBehaviours;

namespace GearUpCards.Patches
{
    [HarmonyPatch(typeof(Empower))]
    class Empower_Patch
    {
        // private static GameObject empowerShotVFX = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_EmpowerShot");

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("Attack")]
        static void AddBulletVisual (bool ___empowered, ref GameObject projectile)
        {
            SpawnedAttack component = projectile.GetComponent<SpawnedAttack>();
            if (!component)
            {
                return;
            }

            // float bulletSize = 0.5f;
            // ProjectileHit component2 = projectile.transform.root.gameObject.GetComponentInChildren<ProjectileHit>();
            // if (!component2)
            // {
            //     bulletSize = component2.damage * component2.dealDamageMultiplierr / 55.0f;
            //     bulletSize = Mathf.Max(0.5f, bulletSize);
            // }
            // Utils.Miscs.Log($"Bullet Size: {bulletSize}");

            if (___empowered)
            {
                projectile.transform.root.gameObject.AddComponent<CustomEmpowerVFX>();
                // GameObject VFX = UnityEngine.Object.Instantiate(empowerShotVFX, projectile.transform);
                // VFX.transform.localEulerAngles = new Vector3(270.0f, 180.0f, 0.0f);
                // // VFX.transform.up = projectile.transform.forward;
                // VFX.transform.localScale = Vector3.one * bulletSize;
            }
        }
    }
}