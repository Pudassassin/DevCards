using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using Photon.Pun;
using UnboundLib;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace GearUpCards.Utils
{
    internal class Miscs
    {
        public static bool debugFlag = true;

        public static void Log(object message)
        {
            if (debugFlag)
            {
                UnityEngine.Debug.Log(message);
            }
        }

        public static void LogWarn(object message)
        {
            if (debugFlag)
            {
                UnityEngine.Debug.LogWarning(message);
            }
        }

        public static void LogError(object message)
        {
            if (debugFlag)
            {
                UnityEngine.Debug.LogError(message);
            }
        }

		// String Utils
		public static List<string> StringSplit(string input, char splitAt)
		{
			List<string> result = new List<string>();
			string buffer = "";

			for (int i = 0; i < input.Length; i++)
			{
				if (input[i] != splitAt)
				{
					buffer += input[i];
				}
				else
				{
					result.Add(buffer);
					buffer = "";
				}
			}
			if (buffer != "")
			{
				result.Add(buffer);
			}

			return result;
		}

		public static int ValidateStringQuery(string targetString, string query)
        {
			// prioritize literal accuracy rather than get the first hit (that is just totally off-target match)
			int baseWeight = 15;
			int queryIndex = 0;
			int result = 0;
			bool flagPerfectMatch = true;

            for (int i = 0; i < targetString.Length; i++)
            {
                if (targetString[i] == query[queryIndex])
                {
					result += baseWeight;
					queryIndex++;
                }
                else
                {
					flagPerfectMatch = false;
					baseWeight--;
					queryIndex++;
				}

                if (queryIndex == query.Length)
                {
                    if (flagPerfectMatch)
                    {
						result += baseWeight * 10;
                    }
					break;
                }
            }

			return result;
        }

		// Credits to Pykess
		public static void CopyGunStats(Gun copyFromGun, Gun copyToGun)
		{
			Miscs.Log("CopyGunStats() : calling base");
			CopyGunStatsBase(copyFromGun, copyToGun);

			Miscs.Log("CopyGunStats() : cloning ShootPojectileAction");
            if (copyFromGun.ShootPojectileAction != null)
            {
				copyToGun.ShootPojectileAction = (Action<GameObject>)copyFromGun.ShootPojectileAction.Clone();
			}
			copyToGun.shootPosition = copyFromGun.shootPosition;

			// dealing with private fields
			Miscs.Log("CopyGunStats() : cloning attackAction");
			Action action = Traverse.Create(copyFromGun).Field("attackAction").GetValue<Action>();
			if (action != null)
			{
				Traverse.Create(copyToGun).Field("attackAction").SetValue((Action)action.Clone());
			}

			Miscs.Log("CopyGunStats() : finishing");
			// Traverse.Create(copyToGun).Field("gunAmmo").SetValue((GunAmmo)Traverse.Create(copyFromGun).Field("gunAmmo").GetValue());
			// Traverse.Create(copyToGun).Field("gunID").SetValue((int)Traverse.Create(copyFromGun).Field("gunID").GetValue());

			Traverse.Create(copyToGun).Field("spreadOfLastBullet").SetValue((float)Traverse.Create(copyFromGun).Field("spreadOfLastBullet").GetValue());
			Traverse.Create(copyToGun).Field("forceShootDir").SetValue((Vector3)Traverse.Create(copyFromGun).Field("forceShootDir").GetValue());
		}
		public static void CopyGunStatsNoActions(Gun copyFromGun, Gun copyToGun)
		{
			CopyGunStatsBase(copyFromGun, copyToGun);

			copyToGun.holdable = null;
			copyToGun.ShootPojectileAction = null;

			// dealing with private fields
			// Traverse.Create(copyToGun).Field("attackAction").SetValue((Action)Traverse.Create(copyFromGun).Field("attackAction").GetValue());
			Traverse.Create(copyToGun).Field("attackAction").SetValue(null);

			// Traverse.Create(copyToGun).Field("gunAmmo").SetValue(null);
			// Traverse.Create(copyToGun).Field("gunID").SetValue((int)Traverse.Create(copyFromGun).Field("gunID").GetValue());

			Traverse.Create(copyToGun).Field("spreadOfLastBullet").SetValue((float)Traverse.Create(copyFromGun).Field("spreadOfLastBullet").GetValue());
			Traverse.Create(copyToGun).Field("forceShootDir").SetValue((Vector3)Traverse.Create(copyFromGun).Field("forceShootDir").GetValue());
		}

		public static void CopyGunStatsBase(Gun copyFromGun, Gun copyToGun)
        {
			copyToGun.ammo = copyFromGun.ammo;
			copyToGun.ammoReg = copyFromGun.ammoReg;
			copyToGun.attackID = copyFromGun.attackID;
			copyToGun.attackSpeed = copyFromGun.attackSpeed;
			copyToGun.attackSpeedMultiplier = copyFromGun.attackSpeedMultiplier;
			copyToGun.bodyRecoil = copyFromGun.bodyRecoil;
			copyToGun.bulletDamageMultiplier = copyFromGun.bulletDamageMultiplier;
			copyToGun.bulletPortal = copyFromGun.bulletPortal;
			copyToGun.bursts = copyFromGun.bursts;
			copyToGun.chargeDamageMultiplier = copyFromGun.chargeDamageMultiplier;
			copyToGun.chargeEvenSpreadTo = copyFromGun.chargeEvenSpreadTo;
			copyToGun.chargeNumberOfProjectilesTo = copyFromGun.chargeNumberOfProjectilesTo;
			copyToGun.chargeRecoilTo = copyFromGun.chargeRecoilTo;
			copyToGun.chargeSpeedTo = copyFromGun.chargeSpeedTo;
			copyToGun.chargeSpreadTo = copyFromGun.chargeSpreadTo;
			copyToGun.cos = copyFromGun.cos;
			copyToGun.currentCharge = copyFromGun.currentCharge;
			copyToGun.damage = copyFromGun.damage;
			copyToGun.damageAfterDistanceMultiplier = copyFromGun.damageAfterDistanceMultiplier;
			copyToGun.defaultCooldown = copyFromGun.defaultCooldown;
			copyToGun.dmgMOnBounce = copyFromGun.dmgMOnBounce;
			copyToGun.dontAllowAutoFire = copyFromGun.dontAllowAutoFire;
			copyToGun.drag = copyFromGun.drag;
			copyToGun.dragMinSpeed = copyFromGun.dragMinSpeed;
			copyToGun.evenSpread = copyFromGun.evenSpread;
			copyToGun.explodeNearEnemyDamage = copyFromGun.explodeNearEnemyDamage;
			copyToGun.explodeNearEnemyRange = copyFromGun.explodeNearEnemyRange;
			copyToGun.forceSpecificAttackSpeed = copyFromGun.forceSpecificAttackSpeed;
			copyToGun.forceSpecificShake = copyFromGun.forceSpecificShake;
			copyToGun.gravity = copyFromGun.gravity;
			copyToGun.hitMovementMultiplier = copyFromGun.hitMovementMultiplier;
			// copyToGun.holdable = copyFromGun.holdable;
			copyToGun.ignoreWalls = copyFromGun.ignoreWalls;
			copyToGun.isProjectileGun = copyFromGun.isProjectileGun;
			copyToGun.isReloading = copyFromGun.isReloading;
			copyToGun.knockback = copyFromGun.knockback;
			copyToGun.lockGunToDefault = copyFromGun.lockGunToDefault;
			copyToGun.multiplySpread = copyFromGun.multiplySpread;
			copyToGun.numberOfProjectiles = copyFromGun.numberOfProjectiles;
			// copyToGun.objectsToSpawn = copyFromGun.objectsToSpawn;
			copyToGun.overheatMultiplier = copyFromGun.overheatMultiplier;
			copyToGun.percentageDamage = copyFromGun.percentageDamage;
			copyToGun.player = copyFromGun.player;
			copyToGun.projectielSimulatonSpeed = copyFromGun.projectielSimulatonSpeed;
			copyToGun.projectileColor = copyFromGun.projectileColor;
			copyToGun.projectiles = copyFromGun.projectiles;
			copyToGun.projectileSize = copyFromGun.projectileSize;
			copyToGun.projectileSpeed = copyFromGun.projectileSpeed;
			copyToGun.randomBounces = copyFromGun.randomBounces;
			copyToGun.recoil = copyFromGun.recoil;
			copyToGun.recoilMuiltiplier = copyFromGun.recoilMuiltiplier;
			copyToGun.reflects = copyFromGun.reflects;
			copyToGun.reloadTime = copyFromGun.reloadTime;
			copyToGun.reloadTimeAdd = copyFromGun.reloadTimeAdd;
			copyToGun.shake = copyFromGun.shake;
			copyToGun.shakeM = copyFromGun.shakeM;
			//copyToGun.ShootPojectileAction = copyFromGun.ShootPojectileAction;
			//copyToGun.shootPosition = copyFromGun.shootPosition;
			copyToGun.sinceAttack = copyFromGun.sinceAttack;
			copyToGun.size = copyFromGun.size;
			copyToGun.slow = copyFromGun.slow;
			copyToGun.smartBounce = copyFromGun.smartBounce;
			//copyToGun.soundDisableRayHitBulletSound = copyFromGun.soundDisableRayHitBulletSound;
			//copyToGun.soundGun = copyFromGun.soundGun;
			//copyToGun.soundImpactModifier = copyFromGun.soundImpactModifier;
			//copyToGun.soundShotModifier = copyFromGun.soundShotModifier;
			copyToGun.spawnSkelletonSquare = copyFromGun.spawnSkelletonSquare;
			copyToGun.speedMOnBounce = copyFromGun.speedMOnBounce;
			copyToGun.spread = copyFromGun.spread;
			copyToGun.teleport = copyFromGun.teleport;
			copyToGun.timeBetweenBullets = copyFromGun.timeBetweenBullets;
			copyToGun.timeToReachFullMovementMultiplier = copyFromGun.timeToReachFullMovementMultiplier;
			copyToGun.unblockable = copyFromGun.unblockable;
			copyToGun.useCharge = copyFromGun.useCharge;
			copyToGun.waveMovement = copyFromGun.waveMovement;

			// duping objectsToSpawn
			copyToGun.objectsToSpawn = copyFromGun.objectsToSpawn;

			// List<ObjectsToSpawn> listObjects = new List<ObjectsToSpawn>();
			// ObjectsToSpawn objectsToSpawn;
			// 
			// foreach (ObjectsToSpawn item in copyFromGun.objectsToSpawn)
			// {
			// 	objectsToSpawn = new ObjectsToSpawn();
			// 
			// 	if (objectsToSpawn.AddToProjectile != null)
			// 	{
			// 		objectsToSpawn.AddToProjectile = GameObject.Instantiate(item.AddToProjectile);
			// 	}
			// 	if (objectsToSpawn.effect != null)
			// 	{
			// 		objectsToSpawn.effect = GameObject.Instantiate(item.effect);
			// 	}
			// 	objectsToSpawn.direction = item.direction;
			// 	objectsToSpawn.spawnOn = ObjectsToSpawn.SpawnOn.notPlayer;
			// 	objectsToSpawn.spawnAsChild = item.spawnAsChild;
			// 	objectsToSpawn.numberOfSpawns = item.numberOfSpawns;
			// 	objectsToSpawn.normalOffset = item.normalOffset;
			// 	objectsToSpawn.stickToBigTargets = item.stickToBigTargets;
			// 	objectsToSpawn.stickToAllTargets = item.stickToAllTargets;
			// 	objectsToSpawn.zeroZ = item.zeroZ;
			// 	objectsToSpawn.removeScriptsFromProjectileObject = item.removeScriptsFromProjectileObject;
			// 	objectsToSpawn.scaleStacks = item.scaleStacks;
			// 	objectsToSpawn.scaleStackM = item.scaleStackM;
			// 	objectsToSpawn.scaleFromDamage = item.scaleFromDamage;
			// 	objectsToSpawn.stacks = item.stacks;
			// 
			// 	listObjects.Add(objectsToSpawn);
			// }
			// 
			// copyToGun.objectsToSpawn = listObjects.ToArray();

			// dealing with private fields
			// Traverse.Create(copyToGun).Field("attackAction").SetValue((Action)Traverse.Create(copyFromGun).Field("attackAction").GetValue());
			// Traverse.Create(copyToGun).Field("gunAmmo").SetValue((GunAmmo)Traverse.Create(copyFromGun).Field("gunAmmo").GetValue());
			// Traverse.Create(copyToGun).Field("gunID").SetValue((int)Traverse.Create(copyFromGun).Field("gunID").GetValue());
			// Traverse.Create(copyToGun).Field("spreadOfLastBullet").SetValue((float)Traverse.Create(copyFromGun).Field("spreadOfLastBullet").GetValue());
			// Traverse.Create(copyToGun).Field("forceShootDir").SetValue((Vector3)Traverse.Create(copyFromGun).Field("forceShootDir").GetValue());
		}
	}
}
