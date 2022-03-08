using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace GearUpCards.Utils
{
    internal class StatsMath
    {
        public static float GetGunBPS(Gun gun)
        {
            if (gun == null) return 0.0f;

			// UnityEngine.Debug.Log($"BPS CHECK");

			float attackTime = gun.attackSpeed * gun.attackSpeedMultiplier;
			int projectileCount = gun.numberOfProjectiles;

			float burstTime = gun.timeBetweenBullets;
			int burstCount = gun.bursts;

			float bps;
			if (burstCount > 1)
			{
				attackTime += (burstCount - 1) * burstTime;
				bps = burstCount * projectileCount / attackTime;
			}
			else
			{
				bps = projectileCount / attackTime;
			}

			// UnityEngine.Debug.Log($"BPS = [{bps}]");
			return bps;
		}

		public static float GetGunAmmoReloadTime(GunAmmo gunAmmo)
        {
			return (gunAmmo.reloadTime + gunAmmo.reloadTimeAdd) * gunAmmo.reloadTimeMultiplier;
        }

		public static bool ApproxEqual(float numA, float numB, float precision = 10e-3f)
        {
			float diff = numA - numB;
			return Mathf.Abs(diff) <= precision;
        }
    }
}
