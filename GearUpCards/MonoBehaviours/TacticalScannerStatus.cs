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


        internal Player player;
        internal CharacterStatModifiers playerStats;
        internal Gun playerGun;
        internal Block playerBlock;


        internal float healthDelta;

        override public void OnAwake()
        {
            // get the affected player data
            this.player = this.gameObject.GetComponent<Player>();
            this.playerStats = this.gameObject.GetComponent<CharacterStatModifiers>();
            this.playerGun = this.gameObject.GetComponent<Gun>();
            this.playerBlock = this.gameObject.GetComponent<Block>();

            GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);

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
            else if (healthDelta < 0)
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
                WorldSpaceOverlayUI component = scanDataUI.AddComponent<WorldSpaceOverlayUI>();
                Graphic[] graphics = component.gameObject.GetComponentsInChildren<Graphic>();
                component.uiElementsToApplyTo = graphics;
                component.Apply();

                scanDataUI.transform.SetParent(this.player.transform);
                scanDataUI.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f) * playerStats.sizeMultiplier * .75f;
                scanDataUI.transform.localPosition += new Vector3(0.0f, 0.0f, 500.0f);

            }
        }

        private void UpdateScanDataUI()
        {
            Text[] textArray = scanDataUI.GetComponentsInChildren<Text>();

            Text dataDamage         = textArray[0];
            Text dataBPS            = textArray[1];
            Text dataBlockCooldown  = textArray[2];
            Text dataBlockCount     = textArray[3];
            Text dataHealth         = textArray[4];

            UnityEngine.Debug.Log("ScanData - #1");

            dataDamage.text = $"[{this.playerGun.damage:f2}] < DMG";
            UnityEngine.Debug.Log("ScanData - #2");

            float bps = GearUpCalc.GetGunBPS(this.playerGun);
            dataBPS.text = $"BPS > [{bps:f2}]";
            UnityEngine.Debug.Log("ScanData - #3");

            dataBlockCooldown.text = $"BlkCD > [{this.playerBlock.Cooldown():f2}]";
            UnityEngine.Debug.Log("ScanData - #4");

            dataBlockCount.text = $"x[{this.playerBlock.additionalBlocks + 1}] < Blocks";
            UnityEngine.Debug.Log("ScanData - #5");

            dataHealth.text = $"[{this.player.data.health} / {this.player.data.maxHealth}]";
            UnityEngine.Debug.Log("ScanData - #6");
        }

        private void UpdateScanDataHealthDelta()
        {
            Text[] textArray = scanDataUI.GetComponentsInChildren<Text>();
            Text dataHealthDelta = textArray[5];

            UnityEngine.Debug.Log("ScanData - #7");

            dataHealthDelta.text = $"[{this.healthDeltaTotal:f3}]";
            UnityEngine.Debug.Log("ScanData - #8");
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
            GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);

            Destroy(scanDataUI);
            Destroy(this);
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
           
        }
    }
}
