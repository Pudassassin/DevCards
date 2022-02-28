using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;
using SoundImplementation;

using UnboundLib;
using UnboundLib.GameModes;
using Photon.Pun;
using ModdingUtils.MonoBehaviours;

using GearUpCards;
using GearUpCards.Extensions;
using GearUpCards.Utils;

namespace GearUpCards.MonoBehaviours
{
    internal class TacticalScannerStatus : ReversibleEffect
    {
        private static GameObject scanDataUIPrefab = GearUpCards.VFXBundle.LoadAsset<GameObject>("ScanDataUI");

        // to be assigned by TacticalScannerEffect Monobehavior
        private float scannerAmpFactor = .25f;
        private float scannerDuration = 10.0f;
        private bool isFriendly = false;
        // ===

        private const float procTime = .10f;

        internal float timer = 0.0f;
        internal bool statusEnable = false;
        internal int proc_count = 0;

        internal float previousHealth = 0.0f;
        internal float scannerAmpAmount = 0.0f;
        internal float healthDeltaTotal = 0.0f;

        internal GameObject scanDataUI = null;
        internal List<Vector3> scanDataUIOffsets;
        internal Text[] textArray;

        internal Image marker;
        internal Text dataDamage;
        internal Text dataBPS;
        internal Text dataBlockCooldown;
        internal Text dataBlockCount;
        internal Text dataHealth;
        internal Text dataHealthDelta;
        internal Text dataReloadSpeed;


        internal Player player;
        internal CharacterStatModifiers playerStats;
        internal Gun playerGun;
        internal Block playerBlock;
        internal GunAmmo playerGunAmmo;


        internal float healthDelta;

        override public void OnAwake()
        {
            // get the affected player data
            this.player = this.gameObject.GetComponent<Player>();
            this.playerStats = this.gameObject.GetComponent<CharacterStatModifiers>();
            this.playerGun = this.gameObject.GetComponent<WeaponHandler>().gun;
            this.playerBlock = this.gameObject.GetComponent<Block>();
            this.playerGunAmmo = this.gameObject.GetComponent<WeaponHandler>().gun.GetComponentInChildren<GunAmmo>();

            previousHealth = this.player.data.health;

            // GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);

            SpawnScanDataUI();
        }

        public override void OnStart()
        {
            UpdateScanDataUI();
        }

        override public void OnUpdate()
        {
            timer += Time.deltaTime;

            float healthDelta = player.data.health - previousHealth;
            previousHealth = player.data.health;
            healthDeltaTotal += healthDelta;

            if (isFriendly && healthDelta > 0)
            {
                scannerAmpAmount += healthDelta;
            }
            else if (!isFriendly && healthDelta < 0)
            {
                scannerAmpAmount += -healthDelta;
            }

            if (timer >= procTime)
            {
                proc_count++;

                if (statusEnable)
                {
                    // Resolve AMP
                    if (scannerAmpAmount > 0.0f)
                    {
                        if (isFriendly)
                        {
                            player.data.healthHandler.Heal(scannerAmpAmount * scannerAmpFactor);
                        }
                        else
                        {
                            player.data.healthHandler.RPCA_SendTakeDamage(new Vector2(scannerAmpAmount * scannerAmpFactor, 0.0f), player.transform.position);
                        }

                        scannerAmpAmount = scannerAmpAmount * scannerAmpFactor * -1.0f;
                    }
                    else
                    {
                        scannerAmpAmount = 0.0f;
                    }

                    // update ScanDataUI
                    UpdateScanDataUI();

                    if (proc_count >= 10)
                    {
                        // update ScanDataUI "Health Delta"
                        UpdateScanDataHealthDelta();

                        healthDeltaTotal = 0.0f;
                        proc_count = 0;
                    }
                }

                timer -= procTime;
            }

            scannerDuration -= Time.deltaTime;
            if (scannerDuration <= 0.0f)
            {
                PurgeStatus();
            }
        }

        private void SpawnScanDataUI()
        {
            if (scanDataUI == null)
            {
                scanDataUI = Instantiate(scanDataUIPrefab, this.player.transform.position, Quaternion.identity);
                scanDataUI.name = "ScanDataUICopy";
                scanDataUI.transform.SetParent(this.player.transform);
                scanDataUI.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f) * playerStats.sizeMultiplier * 1.2f;
                scanDataUI.transform.localPosition += new Vector3(0.0f, 0.0f, 50.0f);

                textArray = scanDataUI.GetComponentsInChildren<Text>();
                scanDataUIOffsets = new List<Vector3>();
                for (int i = 0; i < textArray.Length; i++)
                {
                    textArray[i].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f) * .85f * Mathf.Sqrt(playerStats.sizeMultiplier);
                    scanDataUIOffsets.Add(textArray[i].rectTransform.localPosition);
                    textArray[i].rectTransform.localPosition *= (.85f + Mathf.Sqrt(playerStats.sizeMultiplier));
                }

                this.dataDamage         = textArray[0];
                this.dataBPS            = textArray[1];
                this.dataBlockCooldown  = textArray[2];
                this.dataBlockCount     = textArray[3];
                this.dataHealth         = textArray[4];
                this.dataHealthDelta    = textArray[5];
                this.dataReloadSpeed    = textArray[6];
            }
        }

        private void UpdateScanDataUI()
        {
            // Prefab Adjustment
            scanDataUI.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f) * Mathf.Sqrt(playerStats.sizeMultiplier) * .75f;
            for (int i = 0; i < textArray.Length; i++)
            {
                textArray[i].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f) * .85f * Mathf.Sqrt(playerStats.sizeMultiplier);
                textArray[i].rectTransform.localPosition = scanDataUIOffsets[i] * (1 - Mathf.Sqrt(playerStats.sizeMultiplier));
            }

            // Data part
            this.dataDamage.text = $"[{this.playerGun.damage * 55.0f:f2}] < DMG";

            float bps = GearUpCalc.GetGunBPS(this.playerGun);
            this.dataBPS.text = $"BPS > [{bps:f2}]";

            this.dataReloadSpeed.text = $"RLD > [{GearUpCalc.GetGunAmmoReloadTime(gunAmmo):f2}]";

            this.dataBlockCooldown.text = $"BlkCD > [{this.playerBlock.Cooldown():f2}]";

            this.dataBlockCount.text = $"x[{this.playerBlock.additionalBlocks + 1}] < Blocks";

            this.dataHealth.text = $"[{this.player.data.health:f2} / {this.player.data.maxHealth:f2}]";
        }

        private void UpdateScanDataHealthDelta()
        {
            this.dataHealthDelta.text = $"[{this.healthDeltaTotal:f3}]";
        }

        public void ApplyStatus(float ampFactor, float duration, bool isFriendly)
        {
            if (ampFactor > this.scannerAmpFactor)  this.scannerAmpFactor = ampFactor;
            if (duration > this.scannerDuration)    this.scannerDuration = duration;
            this.isFriendly = isFriendly;
            this.statusEnable = true;
        }

        private void PurgeStatus()
        {
            UnityEngine.Debug.Log($"Purging 'Scanned' [{this.player.playerID}]");

            Destroy(this);

            UnityEngine.Debug.Log($"Purged 'Scanned' [{this.player.playerID}]");
        }

        private IEnumerator OnPointEnd(IGameModeHandler gm)
        {
            // This status effect should be cleared out at point end, if they survived
            PurgeStatus();

            yield break;
        }

        override public void OnOnDisable()
        {
            // This status effect should be cleared out when they are dead, reviving or not
            PurgeStatus();
        }

        override public void OnOnDestroy()
        {
            // GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);
            /**/Destroy(scanDataUI);
        }
    }
}
