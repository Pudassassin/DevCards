using HarmonyLib;

using UnityEngine;

using UnboundLib;

using GearUpCards.MonoBehaviours;

namespace GearUpCards.Patches
{
    [HarmonyPatch(typeof(Empower))]
    class Empower_Patch
    {
        private static GameObject empowerShotVFX = GearUpCards.VFXBundle.LoadAsset<GameObject>("VFX_EmpowerShot");

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

            if (___empowered)
            {
                GameObject VFX = UnityEngine.Object.Instantiate(empowerShotVFX, projectile.transform);
                VFX.transform.localEulerAngles = new Vector3(270.0f, 180.0f, 0.0f);
                // VFX.transform.up = projectile.transform.forward;
                VFX.transform.localScale = VFX.transform.localScale * 5f/8f;
            }
        }
    }
}