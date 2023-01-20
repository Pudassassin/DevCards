using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GearUpCards.MonoBehaviours
{
    public class CircleMeshMono : MonoBehaviour
    {
        [HideInInspector]
        public static Vector3 refZeroAngle = new Vector3(1.0f, 0.0f, 0.0f);

        public int subDivision = 36;
        public float radius = 1.0f;
        public float lineWidth = 0.2f;

        public float startAngle = 0.0f;
        public float endAngle = 360.0f;

        public bool rebuildMesh = false;
        private bool prevRebuildFlag = false;

        private MeshFilter meshFilter;
        private Mesh mesh;

        private Vector3[] meshVerts;
        private Vector2[] meshUVs;
        private int[] meshTrigs;

        // Start is called before the first frame update
        void Start()
        {
            meshFilter = gameObject.GetComponent<MeshFilter>();
            BuildMesh();
        }

        // Update is called once per frame
        void Update()
        {
            if (rebuildMesh == true && prevRebuildFlag == false)
            {
                BuildMesh();
            }
            else
            {
                ShapeMesh();
            }

            prevRebuildFlag = rebuildMesh;
        }

        public void ShapeMesh()
        {
            Vector3 innerVert, outerVert;
            int iIn0, iIn1, iOut0, iOut1;
            float degreeAngle, subDivAngle;

            if (endAngle <= startAngle)
            {
                endAngle += 360.0f;
            }
            subDivAngle = (endAngle - startAngle) / (float)subDivision;

            if (lineWidth < 0.0f)
            {
                lineWidth *= -1.0f;
            }
            else if (lineWidth == 0.0f)
            {
                Debug.LogWarning("[GearUp] CircleMeshMono: Line Width is exactly zero!! using fail-safe value");
                lineWidth = 0.1f;
            }

            // (re)assign verts
            for (int i = 0; i < (meshVerts.Length / 2); i++)
            {
                degreeAngle = startAngle + (subDivAngle * i);

                innerVert = RotateVector(refZeroAngle, degreeAngle) * (radius - (lineWidth / 2.0f));
                outerVert = RotateVector(refZeroAngle, degreeAngle) * (radius + (lineWidth / 2.0f));

                iIn0 = i * 2;
                iOut0 = iIn0 + 1;

                meshVerts[iIn0] = innerVert;
                meshVerts[iOut0] = outerVert;
            }

            // (re)set UVs (unclamped)
            float angleU;
            for (int i = 0; i < (meshUVs.Length / 2); i++)
            {
                degreeAngle = startAngle + (subDivAngle * i);
                angleU = degreeAngle / 360.0f;

                iIn0 = i * 2;
                iOut0 = iIn0 + 1;

                meshUVs[iIn0] = new Vector2(angleU, 0.0f);
                meshUVs[iOut0] = new Vector2(angleU, 1.0f);
            }

            mesh.vertices = meshVerts;
            mesh.uv = meshUVs;
        }

        public void BuildMesh()
        {
            Vector3 innerVert, outerVert;
            int iIn0, iIn1, iOut0, iOut1;
            float degreeAngle, subDivAngle;

            if (endAngle <= startAngle)
            {
                endAngle += 360.0f;
            }
            subDivAngle = (endAngle - startAngle) / (float)subDivision;

            if (lineWidth < 0.0f)
            {
                lineWidth *= -1.0f;
            }
            else if (lineWidth == 0.0f)
            {
                Debug.LogWarning("[GearUp] CircleMeshMono: Line Width is exactly zero!! using fail-safe value");
                lineWidth = 0.1f;
            }

            meshVerts = new Vector3[(subDivision + 1) * 2];
            meshUVs = new Vector2[(subDivision + 1) * 2];
            meshTrigs = new int[(subDivision) * 6];

            //assign verts
            for (int i = 0; i < subDivision + 1; i++)
            {
                degreeAngle = startAngle + (subDivAngle * i);

                innerVert = RotateVector(refZeroAngle, degreeAngle) * (radius - (lineWidth / 2.0f));
                outerVert = RotateVector(refZeroAngle, degreeAngle) * (radius + (lineWidth / 2.0f));

                iIn0 = i * 2;
                iOut0 = iIn0 + 1;

                meshVerts[iIn0] = innerVert;
                meshVerts[iOut0] = outerVert;
            }

            //set UVs (unclamped)
            float angleU;
            for (int i = 0; i < subDivision + 1; i++)
            {
                degreeAngle = startAngle + (subDivAngle * i);
                angleU = degreeAngle / 360.0f;

                iIn0 = i * 2;
                iOut0 = iIn0 + 1;

                meshUVs[iIn0] = new Vector2(angleU, 0.0f);
                meshUVs[iOut0] = new Vector2(angleU, 1.0f);
            }

            //forming trigs
            for (int i = 0; i < subDivision; i++)
            {
                degreeAngle = startAngle + (subDivAngle * i);

                iIn0 = i * 2;
                iIn1 = (i + 1) * 2;
                iOut0 = iIn0 + 1;
                iOut1 = iIn1 + 1;

                meshTrigs[(i * 6) + 2] = iIn0;
                meshTrigs[(i * 6) + 1] = iOut0;
                meshTrigs[(i * 6) + 0] = iOut1;

                meshTrigs[(i * 6) + 5] = iIn0;
                meshTrigs[(i * 6) + 4] = iOut1;
                meshTrigs[(i * 6) + 3] = iIn1;
            }

            mesh = new Mesh();
            mesh.vertices = meshVerts;
            mesh.triangles = meshTrigs;

            meshFilter.mesh = mesh;
        }

        // Vector Utils
        public static Vector3 RotateVector(Vector3 vector, float degree)
        {
            float sin = Mathf.Sin(degree * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degree * Mathf.Deg2Rad);

            float prevX = vector.x;
            float prevY = vector.y;

            vector.x = (cos * prevX) - (sin * prevY);
            vector.y = (sin * prevX) + (cos * prevY);

            return vector;
        }
    }
}