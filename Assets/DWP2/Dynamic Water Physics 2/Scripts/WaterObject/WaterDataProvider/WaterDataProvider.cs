﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if DWP_CREST
using Crest;
#endif

namespace DWP2
{
    /// <summary>
    /// Class for providing water height and velocity data to WaterObjectManager.
    /// </summary>
    public abstract class WaterDataProvider
    {
        public string waterObjectTag = "Water";
        public bool groupWaterQueries = true;
        public bool queryWaterHeights = true;
        public bool queryWaterVelocities = true;
        public float waterHeightOffset = 0f;
        public GameObject waterObject;

        public virtual void Initialize()
        {
            GameObject[] waterObjects = GameObject.FindGameObjectsWithTag(waterObjectTag).Where(g => g.activeInHierarchy).ToArray();

            if (waterObjects.Length == 0)
            {
                Debug.LogWarning("Found 0 objects with tag 'Water'. Assuming water is at y = 0.");
                return;
            }

            if (waterObjects.Length > 1)
            {
                Debug.LogWarning($"More than one object has tag 'Water'. Only one water object may be active at a time. " +
                                 $"Using first water object: {waterObjects[0].name}");
            }

            waterObject = waterObjects[0];
        }

        /// <summary>
        /// Returns water height at each given point.
        /// </summary>
        /// <param name="p0s">Position array in local coordinates.</param>
        /// <param name="p1s">Position array in local coordinates.</param>
        /// <param name="p2s">Position array in local coordinates.</param>
        /// <param name="waterHeights0">Water array height in world coordinates. Corresponds to p0s.</param>
        /// <param name="waterHeights1">Water array height in world coordinates. Corresponds to p1s.</param>
        /// <param name="waterHeights2"Water array height in world coordinates. Corresponds to p2s.</param>
        /// <param name="localToWorldMatrices">Maxtrix to convert from local to world coordinates.</param>
        public virtual void GetWaterHeights(ref Vector3[] p0s, ref Vector3[] p1s, ref Vector3[] p2s,
            ref float[] waterHeights0, ref float[] waterHeights1, ref float[] waterHeights2, ref Matrix4x4[] localToWorldMatrices)
        {
#if DWP_CREST
            // Wait for Crest to initialize
            if (Time.frameCount < 16) return;
#endif

            Debug.Assert(p0s.Length == waterHeights0.Length, "Points and WaterHeights arrays must have same length.");

            int n = p0s.Length;

            // Fill in with 0s if no water object set
            if (waterObject == null || !queryWaterHeights)
            {
                for (int i = 0; i < n; i++)
                {
                    waterHeights0[i] = 10;
                    waterHeights1[i] = 10;
                    waterHeights2[i] = 10;
                }
            }
            else
            {
#if DWP_FLAT_WATER
                float waterY = waterObject.transform.position.y;
                for (int i = 0; i < n; i++)
                {
                    waterHeights0[i] = waterY;
                    waterHeights1[i] = waterY;
                    waterHeights2[i] = waterY;
                }
#else
                if (groupWaterQueries)
                {
                    for (int i = 0; i < n; i++)
                    {
                        waterHeights0[i] = waterHeights1[i] = waterHeights2[i] = GetWaterHeight(localToWorldMatrices[i].MultiplyPoint3x4((p0s[i] + p1s[i] + p2s[i]) / 3f)) + waterHeightOffset;
                    }
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        waterHeights0[i] = GetWaterHeight(localToWorldMatrices[i].MultiplyPoint3x4(p0s[i])) + waterHeightOffset;
                        waterHeights1[i] = GetWaterHeight(localToWorldMatrices[i].MultiplyPoint3x4(p1s[i])) + waterHeightOffset;
                        waterHeights2[i] = GetWaterHeight(localToWorldMatrices[i].MultiplyPoint3x4(p2s[i])) + waterHeightOffset;
                    }
                }
#endif
            }
        }

        public virtual float GetWaterHeight(Vector3 point)
        {
            return 0;
        }


        public virtual void GetWaterVelocities(ref Vector3[] p0s, ref Vector3[] p1s, ref Vector3[] p2s,
            ref Vector3[] waterVelocities0, ref Vector3[] waterVelocities1, ref Vector3[] waterVelocities2,
            ref Matrix4x4[] localToWorldMatrices)
        {
            Debug.Assert(p0s.Length == waterVelocities0.Length, "Points and WaterHeights arrays must have same length.");

            int n = p0s.Length;

            if (waterObject == null || !queryWaterVelocities)
            {
                for (int i = 0; i < n; i++)
                {
                    waterVelocities0[i] = Vector3.zero;
                    waterVelocities1[i] = Vector3.zero;
                    waterVelocities2[i] = Vector3.zero;
                }
            }
            else
            {
                if (groupWaterQueries)
                {
                    for (int i = 0; i < n; i++)
                    {
                        waterVelocities0[i] = waterVelocities1[i] = waterVelocities2[i] = GetWaterVelocity(localToWorldMatrices[i].MultiplyPoint3x4((p0s[i] + p1s[i] + p2s[i]) / 3f));
                    }
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        waterVelocities0[i] = GetWaterVelocity(localToWorldMatrices[i].MultiplyPoint3x4(p0s[i]));
                        waterVelocities1[i] = GetWaterVelocity(localToWorldMatrices[i].MultiplyPoint3x4(p1s[i]));
                        waterVelocities2[i] = GetWaterVelocity(localToWorldMatrices[i].MultiplyPoint3x4(p2s[i]));
                    }
                }
            }
        }

        public virtual Vector3 GetWaterVelocity(Vector3 point)
        {
            return Vector3.zero;
        }
    }
}

