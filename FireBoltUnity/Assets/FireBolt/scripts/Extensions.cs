﻿using System.IO;
using UnityEngine;
using LN.Utilities;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace Assets.scripts
{
    public static class Extensions
    {
        private static readonly string doublePattern = @"[-+]?([0-9]+,)*[0-9]+(\.[0-9]*)?(E[-+][0-9]+)?";
        private static readonly Regex regex = new Regex(string.Format(@"^\s*(\(\s*(?<x>({0}))\s*,\s*(?<y>{0})\s*,\s*(?<z>{0})\s*\))\s*$", doublePattern), RegexOptions.ExplicitCapture);

        /// <summary>
        /// converts comma delimited string into a vector 3
        /// </summary>
        /// <param name="s">this better have x,y,z in it</param>
        /// <returns>shiny vector3</returns>
        public static bool TryParseVector3(this string s, out Vector3 v)
        {
            v = Vector3.zero;
            var match = regex.Match(s);
            if (match.Success)
            {
                v = new Vector3(float.Parse(match.Groups["x"].Value), float.Parse(match.Groups["y"].Value), float.Parse(match.Groups["z"].Value));
                return true;
            }
            return false;            
        }


        /// <summary>
        /// converts comma delimited numeric pair into x,z coordinates
        /// </summary>
        /// <param name="s">string of format x,z </param>
        /// <returns>vector 3</returns>     
        public static bool TryParsePlanarCoords(this string s, out Vector2 v)
        {
            v = Vector3.zero;
            string[] values = s.Split(new char[] { ',' });
            float x, z;
            if (values.Length > 1 &&
                float.TryParse(values[0], out x )&&
                float.TryParse(values[1], out z)) //we got two coords
            {
                v = new Vector2((float)x,(float)z);
                return true;
            }
            return false;
        }

        public static void RenderColliders()
        {
            foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
            {
                BoxCollider collider = obj.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.bounds.BuildDebugBox(5, Color.green);
                }
            }
        }

        /// <summary>
        /// gives a pretty cyan box at the bounds...only in scene view as it's a debug thinger
        /// </summary>
        /// <param name="bounds"></param>
        public static void BuildDebugBox(this Bounds bounds, float duration, Color color)
        {
            //add some debugging box for the area we think we are framing
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;
            Vector3[] corners = new Vector3[8];
            //top face
            corners[0] = new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z);
            corners[1] = new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z);
            corners[2] = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
            corners[3] = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
            //bottom face
            corners[4] = new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z);
            corners[5] = new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z);
            corners[6] = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
            corners[7] = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
            //gogo complete graph!
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (i == j) continue;
                    UnityEngine.Debug.DrawLine(corners[i], corners[j], color, duration);
                    //Gizmos.DrawLine(corners[i], corners[j]);//, Color.cyan, 150);
                }
            }
        }

        /// <summary>
        /// finds the angle of rotation to change orientation from "from" to "to"
        /// ignores Y values
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>degree measure of needed rotation -180 to 180</returns>
        public static float GetXZAngleTo(this Vector3 from, Vector3 to)
        {

            Vector2 from2d = new Vector2(from.x, from.z);
            Vector2 to2d = new Vector2(to.x, to.z);
            Vector2 direction = to2d - from2d;
            float radians = Mathf.Atan2(direction.x,direction.y);
            if (radians > Mathf.PI) radians -= 2 * Mathf.PI;
            else if (radians < -Mathf.PI) radians += 2 * Mathf.PI;
            return radians * 180/Mathf.PI;
        }

        //public static Vector3 ToVector3(this Impulse.v_1_336.Constants.Coordinate2D from)
        //{
        //    //return new Vector3((float)from.X, 0, (float)from.Y);
        //}

        public static Vector3 ToVector3(this Impulse.v_1_336.Constants.Coordinate2D from, 
                                        float domainToEngineX, float domainToEngineY, float domainToEngineZ)
        {
            return new Vector3((float)(from.X / domainToEngineX), 0, (float)(from.Y / domainToEngineZ));
        }

        public static Vector3Nullable ToVector3Nullable(this Impulse.v_1_336.Constants.Coordinate2D from,
                                                        float domainToEngineX, float domainToEngineY, float domainToEngineZ)
        {
            return new Vector3Nullable((float)(from.X / domainToEngineX), null, (float)(from.Y / domainToEngineZ));
        }

        public static Vector3Nullable ToVector3Nullable(this Impulse.v_1_336.Constants.Coordinate3D from,
                                                        float domainToEngineX, float domainToEngineY, float domainToEngineZ)
        {
            return new Vector3Nullable((float)(from.X / domainToEngineX), (float)(from.Y / domainToEngineY), (float)(from.Z / domainToEngineZ));
        }

        public static Vector3 ToVector3(this Impulse.v_1_336.Constants.Coordinate3D from,
                                        float domainToEngineX, float domainToEngineY, float domainToEngineZ)
        {
            return new Vector3((float)(from.X / domainToEngineX), (float)(from.Y / domainToEngineY), (float)(from.Z / domainToEngineZ));
        }

        public static float ToMillis(this uint tick, uint millisPerTick)
        {
            return tick * millisPerTick;
        }

        /// <summary>
        /// applies the specified values in this Vector3Nullable, newValues, over those in overridden.
        /// for all unspecified values in newValues, overridden controls.
        /// </summary>
        /// <param name="newValues"></param>
        /// <param name="overridden"></param>
        /// <returns></returns>
        public static Vector3 Merge(this Vector3Nullable newValues, Vector3 overridden)
        {
            return new Vector3(newValues.X.HasValue ? newValues.X.Value : overridden.x,
                               newValues.Y.HasValue ? newValues.Y.Value : overridden.y,
                               newValues.Z.HasValue ? newValues.Z.Value : overridden.z);
        }

        public static float convertSourceEngineToUnityRotation(this float sourceDegrees)
        {
            float unityDegrees = -sourceDegrees + 90;
            unityDegrees = unityDegrees.BindToSemiCircle();
            //% 360;
            //while (unityDegrees > 180)
            //{
            //    unityDegrees -= 360;
            //}
            //while (unityDegrees < -180)
            //{
            //    unityDegrees += 360;
            //}
            return unityDegrees;
        }

        public static float BindToSemiCircle(this float theta)
        {
            theta = theta % 360;
            while (theta > 180)
            {
                theta -= 360;
            }
            while (theta < -180)
            {
                theta += 360;
            }
            return theta;
        }

        public static string AppendTimestamps(this string s)
        {
            return s + string.Format(" d:s[{0}:{1}]", ElPresidente.currentDiscourseTime, ElPresidente.currentStoryTime);
        }

        public static void Log(string formatString, params object[] values)
        {
            if(ElPresidente.Instance.LogDebugStatements)
                Debug.Log(string.Format(formatString,values).AppendTimestamps());
        }

        private static bool statLogInitialized = false;
        public static void InitStatFile()
        {
            if (File.Exists(ElPresidente.Instance.StatFile))
            {
                File.Delete(ElPresidente.Instance.StatFile);
                statLogInitialized = true;
            }
        }
        public static void LogStatistics(Dictionary<string,string> stats)
        {
            if (!statLogInitialized)
            {
                InitStatFile();
            }
            StringBuilder keys = new StringBuilder();
            StringBuilder values = new StringBuilder();
            foreach(var key in stats.Keys)
            {
                keys.Append(key);
                keys.Append(',');

                values.Append(stats[key]);
                values.Append(',');
            }
            keys.Append(Environment.NewLine);
            values.Append(Environment.NewLine);

            File.AppendAllText(ElPresidente.Instance.StatFile, keys.ToString() + values.ToString());            
        }
    }
}
