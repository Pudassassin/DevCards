using UnboundLib;
using UnityEngine;
using UnboundLib.GameModes;
using ModdingUtils.MonoBehaviours;

using GearUpCards.Utils;
using GearUpCards.Extensions;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace GearUpCards.MonoBehaviours
{
    public class BeamVFXMono : MonoBehaviour
    {
        static float defaultBeamLength = 4.0f;
        static string maskBoundName = "Mask_Bound";
        static Vector3 referenceVector = new Vector3(1.0f, 0.0f, 0.0f);

        public Transform TLinkFrom, TLinkTo;
        public float unlinkedLifetime = 3.0f;

        private Vector3 posLinkFrom, posLinkTo;

        private float distance, rotation;
        private Vector3 direction, beamScale;

        private GameObject beamMaskObject;
        private RemoveAfterSeconds remover;

        SpriteMask[] spriteMasks;
        SpriteRenderer[] spriteRenderers;
        SortingGroup[] sortingGroups;

        public void Start()
        {
            posLinkFrom = Vector3.zero;
            posLinkTo = Vector3.zero;
            beamMaskObject = transform.Find(maskBoundName).gameObject;

            spriteRenderers = transform.root.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer item in spriteRenderers)
            {
                item.sortingLayerName = "UI";
            }

            spriteMasks = transform.root.GetComponentsInChildren<SpriteMask>();
            foreach (SpriteMask item in spriteMasks)
            {
                item.sortingLayerName = "UI";
                item.frontSortingLayerID = SortingLayer.NameToID("UI");
                item.backSortingLayerID = SortingLayer.NameToID("UI");
                item.frontSortingOrder = 100;
                item.backSortingOrder = -100;
                item.isCustomRangeActive = true;
            }
            
            sortingGroups = transform.root.GetComponentsInChildren<SortingGroup>();
            foreach (SortingGroup item in sortingGroups)
            {
                item.sortingLayerName = "UI";
            }
        }

        public void LateUpdate()
        {
            if (TLinkTo != null)
            {
                posLinkTo = TLinkTo.position;
                posLinkTo.z = 0;
            }
            if (TLinkFrom != null)
            {
                posLinkFrom = TLinkFrom.position;
                posLinkFrom.z = 0;
            }
            if (TLinkFrom == null && TLinkTo == null)
            {
                if (remover == null)
                {
                    remover = gameObject.AddComponent<RemoveAfterSeconds>();
                    remover.seconds = unlinkedLifetime;
                }
            }

            if (beamMaskObject == null)
            {
                Miscs.LogWarn("[GearUp] BeamVFXMono: cannot find mask object");
                Destroy(gameObject);
            }
            else
            {
                beamScale = beamMaskObject.transform.localScale;
                beamScale.x = distance / defaultBeamLength;
                beamMaskObject.transform.localScale = beamScale;
            }

            direction = posLinkTo - posLinkFrom;
            distance = direction.magnitude;
            rotation = GetRotationFromVector(direction);

            transform.eulerAngles = new Vector3(0.0f, 0.0f, rotation);
            transform.position = Vector3.Lerp(posLinkFrom, posLinkTo, 0.5f);

        }

        public static float GetRotationFromVector(Vector3 vector)
        {
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }
    }
}
