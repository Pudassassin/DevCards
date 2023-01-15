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
        protected static float defaultBeamLength = 4.0f;
        protected static float defaultBeamWidth = 1.0f;
        protected static string maskBoundName = "Mask_Bound";
        protected static Vector3 referenceVector = new Vector3(1.0f, 0.0f, 0.0f);

        public Transform TLinkFrom, TLinkTo;
        public float unlinkedLifetime = 3.0f;

        protected Vector3 posLinkFrom, posLinkTo;

        protected float distance, rotation, width;
        protected Vector3 direction, beamScale, parentScale;

        protected GameObject beamMaskObject;
        protected RemoveAfterSeconds remover;

        SpriteMask[] spriteMasks;
        SpriteRenderer[] spriteRenderers;
        SortingGroup[] sortingGroups;

        public void Start()
        {
            posLinkFrom = Vector3.zero;
            posLinkTo = Vector3.zero;
            beamMaskObject = transform.Find(maskBoundName).gameObject;
            width = defaultBeamWidth;

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
            transform.localScale = Vector3.one;
            parentScale = transform.lossyScale;

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
                beamScale = new Vector3
                (
                    (distance / defaultBeamLength) / parentScale.x,
                    1.0f / parentScale.y,
                    1.0f / parentScale.z
                );
                // beamScale.x = distance / defaultBeamLength;
                beamMaskObject.transform.localScale = beamScale;
            }

            direction = posLinkTo - posLinkFrom;
            distance = direction.magnitude;
            rotation = GetRotationFromVector(direction);

            transform.localScale = new Vector3(1.0f, (width / defaultBeamWidth) / parentScale.y, 1.0f);
            transform.eulerAngles = new Vector3(0.0f, 0.0f, rotation);
            transform.position = Vector3.Lerp(posLinkFrom, posLinkTo, 0.5f);

        }

        public void SetBeamWidth(float width)
        {
            this.width = width;
        }

        public static float GetRotationFromVector(Vector3 vector)
        {
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }
    }

    public class RayVFXMono: BeamVFXMono
    {
        static string rayFocusName = "Beam_Focus";

        protected GameObject rayFocusObject;
        protected Vector3 rayScale;

        public void Start()
        {
            base.Start();

            rayFocusObject = transform.Find(rayFocusName).gameObject;
        }

        public void LateUpdate()
        {
            base.LateUpdate();

            rayScale = new Vector3
            (
                (distance / defaultBeamLength) / parentScale.x,
                1.0f / parentScale.y,
                1.0f / parentScale.z
            );
            // rayScale.x = distance / defaultBeamLength;
            rayFocusObject.transform.localScale = rayScale;
        }
    }
}
