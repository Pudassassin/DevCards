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
        private float scannerAmpFactor = .20f;
        private float scannerDuration = 6.0f;
        private bool isFriendly = false;
        // ===

        private const float procTime = .10f;

        internal float timer = 0.0f;
        internal bool statusEnable = false;
        internal int proc_count = 0;

        internal float timeApplied = 0.0f;

        internal float previousHealth = 0.0f;
        internal float previousMaxHealth = 0.0f;
        internal float scannerAmpAmount = 0.0f;
        internal float healthDeltaTotal = 0.0f;

        internal GameObject scanDataUI = null;
        internal List<Vector3> scanDataUIOffsets;
        internal List<float> scanDataUITextScales;
        internal Text[] textArray;
        internal Image[] imageArray;

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
            previousMaxHealth = this.player.data.maxHealth;

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

            float flagPristineLoss = (previousMaxHealth) / player.data.maxHealth;
            float flagPristineGain = (player.data.maxHealth) / previousMaxHealth;

            if (isFriendly && healthDelta > 0)
            {
                if (flagPristineGain >= 2.5f) healthDelta /= flagPristineGain;
                scannerAmpAmount += healthDelta;
            }
            else if (!isFriendly && healthDelta < 0)
            {
                if (flagPristineLoss >= 2.5f) healthDelta /= flagPristineLoss;
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
                        proc_count = 0;
                    }
                }

                timer -= procTime;
            }

            if (Time.time - this.timeApplied >= scannerDuration)
            {
                PurgeStatus();
            }

            scanDataUI.transform.position = this.player.transform.position;
        }

        private void SpawnScanDataUI()
        {
            if (scanDataUI == null)
            {
                scanDataUI = Instantiate(scanDataUIPrefab, this.player.transform.position, Quaternion.identity);
                scanDataUI.name = "ScanDataUICopy";

                scanDataUI.transform.localPosition += new Vector3(0.0f, 0.0f, 50.0f);
                scanDataUI.GetComponent<Canvas>().sortingLayerName = "MostFront";

                textArray = scanDataUI.GetComponentsInChildren<Text>();
                imageArray = scanDataUI.GetComponentsInChildren<Image>();

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
            scanDataUI.transform.localScale = Vector3.one * 0.04f;

            if (!isFriendly)
            {
                imageArray[0].color = new Color(1.0f, .25f, .25f, 1.0f);
            }
            else
            {
                imageArray[0].color = new Color(.25f, 1.0f, .25f, 1.0f);
            }

            // Data part
            int bulletBatch;
            if (this.playerGun.bursts > 1)
            {
                bulletBatch = this.playerGun.bursts * this.playerGun.numberOfProjectiles;
            }
            else
            {
                bulletBatch = this.playerGun.numberOfProjectiles;
            }

            this.dataDamage.text = $"{bulletBatch}x[{this.playerGun.damage * 55.0f:f2}] < DMG";

            float bps = StatsMath.GetGunBPS(this.playerGun);
            this.dataBPS.text = $"BPS > [{bps:f2}]";

            this.dataReloadSpeed.text = $"RLD > [{StatsMath.GetGunAmmoReloadTime(gunAmmo):f2}]";

            this.dataBlockCooldown.text = $"BlkCD > [{this.playerBlock.Cooldown():f2}]";

            this.dataBlockCount.text = $"x[{this.playerBlock.additionalBlocks + 1}] < Blocks";

            this.dataHealth.text = $"[{this.player.data.health:f2} / {this.player.data.maxHealth:f2}]";
        }

        private void UpdateScanDataHealthDelta()
        {
            if (healthDeltaTotal >= 0)
            {
                this.dataHealthDelta.text = $"+ [{Mathf.Abs(this.healthDeltaTotal):f3}]";
            }
            else
            {
                this.dataHealthDelta.text = $"- [{Mathf.Abs(this.healthDeltaTotal):f3}]";
            }

            healthDeltaTotal = 0.0f;
        }

        public void ApplyStatus(float ampFactor, float duration, bool isFriendly)
        {
            if (ampFactor > this.scannerAmpFactor)  this.scannerAmpFactor = ampFactor;
            if (duration > this.scannerDuration)    this.scannerDuration = duration;
            this.isFriendly = isFriendly;
            this.statusEnable = true;

            timeApplied = Time.time;
        }

        private void PurgeStatus()
        {
            // UnityEngine.Debug.Log($"Purging 'Scanned' [{this.player.playerID}]");

            Destroy(this);

            // UnityEngine.Debug.Log($"Purged 'Scanned' [{this.player.playerID}]");
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
            Destroy(scanDataUI);
        }
    }
}
