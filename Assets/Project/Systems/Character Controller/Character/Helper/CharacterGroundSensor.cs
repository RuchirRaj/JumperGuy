using System;
using System.Collections;
using System.Collections.Generic;
using Drawing;
using RR.Utils;
using UnityEngine;

namespace RR.Gameplay.CharacterController
{
    [Serializable]
    public class CharacterGroundSensor
    {

        public const float RAYCAST_INCREMENT = 0.00390625f; //1/(2^8)
        
        public enum Shape
        {
            Raycast,
            Sphere,
            Box
        }

        public Shape shape = Shape.Raycast;
        public int row = 0;
        public int column = 3;
        public bool offsetRow;
        public bool flatBase = true;
        
        public float radiusMultiplier = 0.99f;
        public int maxIteration = 3;
        public LayerMask groundMask = 255;
        public List<Collider> ignoreCollider;

        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private bool enableDebug;
        public Vector3 centerOffset;
        public bool stableGround, unStableGround;
        public RayHitInfo averageHit;
        public List<float> slopes = new();
        public List<Vector3> castPointsLocal = new();
        public List<Vector3> castPointsWorld = new();
        public List<float> castPointToBottom = new();
        public List<RayHitInfo> hitInfos = new();
        public int totalRayCasts;
        public bool hasDetectedHit;

        private float _startHeight, _endHeight;
        
        //----------------------------
        [SerializeReference] public Func<RayHitInfo, bool> RaycastValidationFunc;

        #region Editor

        private string RaycastPointInfoText()
        {
            return $"Total points : {GetTotalRaycastPoints().ToString()}";
        }

        #endregion

        #region Core

        public CharacterGroundSensor()
        {
            RaycastValidationFunc = Hlp_RaycastValidationFunc;
        }

        /// <summary>
        /// Initialize cache
        /// </summary>
        public void InitCache(float radius)
        {
            RaycastValidationFunc ??= Hlp_RaycastValidationFunc;
            totalRayCasts = shape == Shape.Raycast ? GetTotalRaycastPoints() : 1;
            Hlp_StoreRaycastPositionsLocal(row, column, offsetRow, radius * radiusMultiplier, castPointsLocal);
            hitInfos.Resize(totalRayCasts);
            castPointsWorld.Resize(totalRayCasts);
            castPointsLocal.Resize(totalRayCasts);
            castPointToBottom.Resize(totalRayCasts);
            slopes.Resize(totalRayCasts);
        }

        /// <summary>
        /// Update cache values
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="startHeight"></param>
        /// <param name="endHeight"></param>
        /// <param name="offset">Offset added to PhysicsCast Positions</param>
        /// <param name="radius"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void UpdateCache(Transform tr, float startHeight, float endHeight, Vector3 offset, float radius)
        {
            if (!tr)
                throw new NullReferenceException("Sensor needs a reference to Transform to update Global Variables");
            centerOffset = offset;
            Hlp_ConvertLocalToWorld(tr, castPointsLocal, castPointsWorld, startHeight);
            var count = 1;
            if (shape == Shape.Raycast && !flatBase)
                count = shape == Shape.Raycast ? flatBase ? 1 : totalRayCasts : 1;
            Hlp_UpdateRaycastLength(castPointToBottom, castPointsLocal, radius * radiusMultiplier, 1,
                startHeight - endHeight, count);
            _startHeight = startHeight;
            _endHeight = endHeight;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void Cast(Transform tr, Vector3 refUp, float distance, LayerMask mask, float slopeLimit, float radius = 0,
            bool enableRuntimeGizmos = false, CharacterBase.GizmoFlag runtimeGizmoFlag = 0)
        {
            hasDetectedHit = false;
            stableGround = unStableGround = false;
            var debug = enableRuntimeGizmos && (runtimeGizmoFlag & CharacterBase.GizmoFlag.Raycasts) != 0;
            switch (shape)
            {
                case Shape.Raycast:
                    Raycast();
                    break;
                case Shape.Sphere:
                    SphereCast();
                    break;
                case Shape.Box:
                    BoxCast();
                    break;
            }
            //BUG Doesn't work properly when using boxColliders
            SetAverageHit();
            
            void Raycast()
            {
                RayHitInfo hitInfo = new RayHitInfo();
                for (var i = 0; i < totalRayCasts; i++)
                {
                    // Gizmos.DrawSphere(castPointsWorld[i], 0.05f);
                    if(debug)
                        using (Draw.ingame.WithColor(Color.green))
                            Draw.ingame.DrawSolidSphere(castPointsWorld[i], 0.05f * Vector3.one);
                    var count = shape == Shape.Raycast ? flatBase ? 0 : i : 0;
                    var hitSomething = false;
                    var offsetV = Vector3.zero;
                    var offset = 0f;
                    if(debug)
                        Draw.ingame.DrawSolidLine(castPointsWorld[i], castPointsWorld[i] - refUp *
                            (castPointToBottom[count] + distance), 0.005f);
                    for (var j = 0; j < maxIteration; j++)
                    {
                        hitSomething = Physics.Raycast(castPointsWorld[i] + offsetV,
                            -refUp,
                            out var hit,
                            castPointToBottom[count] + distance - offset,
                            mask,
                            QueryTriggerInteraction.Ignore);

                        if (!hitSomething)
                            break;
                        hitInfo = (RayHitInfo)hit;
                        hitInfo.origin = castPointsWorld[i];
                        hitInfo.distance += offset;
                        hitSomething &= RaycastValidationFunc(hitInfo);
                        slopes[i] = Vector3.Angle(hit.normal, refUp);
                        
                        if (hitSomething)
                            break;
                        
                        offset += hitInfo.distance + RAYCAST_INCREMENT;
                        offsetV = -offset * tr.up;
                        if (offset > castPointToBottom[count] + distance)
                            break;
                    }

                    if (!hitSomething)
                        hitInfo = new RayHitInfo();
                    else
                    {
                        hasDetectedHit = true;
                        if (debug)
                            using (Draw.ingame.WithColor(Color.red))
                                Draw.ingame.DrawSolidSphere(hitInfo.point, 0.02f * Vector3.one);
                    }

                    hitInfos[i] = hitInfo;
                }

            }

            void SphereCast()
            {
                var iterations = 0;
                bool hitSomething;
                var hitInfo = new RayHitInfo();
                var offsetV = Vector3.zero;
                var offset = 0f;
                while (true)
                {
                    hitSomething = Physics.SphereCast(castPointsWorld[0] + refUp * RAYCAST_INCREMENT + offsetV,
                        radius * radiusMultiplier,
                        -refUp,
                        out var hit,
                        castPointToBottom[0] + distance - offset,
                        mask,
                        QueryTriggerInteraction.Ignore);

                    if (!hitSomething)
                        break;
                    hitInfo = (RayHitInfo)hit;
                    hitInfo.origin = castPointsWorld[0];
                    hitInfo.distance += offset - 0.00390625f;
                    hitSomething &= RaycastValidationFunc(hitInfo);
                    
                    //TODO Add two extra raycasts to get actual slope
                    if (hitSomething)
                        hitSomething &= Hlp_ValidateSphereSkin(hitInfo, -refUp, _startHeight - _endHeight, radius * radiusMultiplier,
                            out distance, hitInfo.distance);
                    slopes[0] = Vector3.Angle(hit.normal, refUp);
                    
                    if (hitSomething) break;
                    
                    offset += hitInfo.distance + 0.0078125f; //1/(2^7)
                    offsetV = -offset * tr.up;
                    
                    if (offset > castPointToBottom[0] + distance) break;

                    iterations++;
                    if (iterations > maxIteration) break;
                }

                if (!hitSomething)
                    hitInfo = new RayHitInfo();
                else
                    hasDetectedHit = true;

                hitInfo.distance += radius * radiusMultiplier;
                hitInfos[0] = hitInfo;
            }

            void BoxCast()
            {
                var iterations = 0;
                bool hitSomething;
                var hitInfo = new RayHitInfo();
                var offsetV = Vector3.zero;
                var offset = 0f;
                var extent = (radiusMultiplier * radius) * Vector3.one;
                while (true)
                {
                    hitSomething = Physics.BoxCast(castPointsWorld[0] + refUp * RAYCAST_INCREMENT + offsetV,
                        extent, -refUp,
                        out var hit,
                        tr.rotation,
                        castPointToBottom[0] + distance - offset,
                        mask, QueryTriggerInteraction.Ignore);
                    
                    if (!hitSomething)
                        break;
                    hitInfo = (RayHitInfo)hit;
                    hitInfo.origin = castPointsWorld[0];
                    hitInfo.distance += offset - 0.00390625f;
                    hitSomething &= RaycastValidationFunc(hitInfo);
                    
                    //TODO Add two extra raycasts to get actual slope
                    /*if (hitSomething)
                        hitSomething &= Hlp_ValidateSphereSkin(hitInfo, -refUp, _startHeight - _endHeight, radius,
                            out distance, hitInfo.distance);*/
                    slopes[0] = Vector3.Angle(hit.normal, refUp);
                    
                    if (hitSomething) break;
                    
                    offset += hitInfo.distance + 0.0078125f; //1/(2^7)
                    offsetV = -offset * tr.up;
                    
                    if (offset > castPointToBottom[0] + distance) break;

                    iterations++;
                    if (iterations > maxIteration) break;
                }

                if (!hitSomething)
                    hitInfo = new RayHitInfo();
                else
                    hasDetectedHit = true;

                hitInfo.distance += radius * radiusMultiplier;
                hitInfos[0] = hitInfo;
            }

            void SetAverageHit()
            {
                if (!hasDetectedHit)
                {
                    averageHit = new RayHitInfo();
                    return;
                }
                
                //TODO Differentiate between stable and unstable slopes
                
                //Average out the results
                var stableHits = new AverageGroundStats();
                var unStableHits = new AverageGroundStats();

                for (var i = 0; i < totalRayCasts; i++)
                {
                    var count = shape == Shape.Raycast ? flatBase ? 0 : i : 0;
                    //Calculate Average Normal
                    if(!hitInfos[i].valid) continue;

                    if (slopes[i] < slopeLimit)
                    {
                        stableHits.valid = true;
                        if (!stableHits.col)
                            stableHits.col = hitInfos[i].col;
                        stableHits.hits++;
                        stableHits.normal += hitInfos[i].normal;
                        stableHits.point += hitInfos[i].point;
                        stableHits.distance += hitInfos[i].distance - castPointToBottom[count];
                    }
                    else
                    {
                        unStableHits.valid = true;
                        if (!unStableHits.col)
                            unStableHits.col = hitInfos[i].col;
                        unStableHits.hits++;
                        unStableHits.normal += hitInfos[i].normal;
                        unStableHits.point += hitInfos[i].point;
                        unStableHits.distance += hitInfos[i].distance - castPointToBottom[count];
                    }
                }

                var average = stableHits.valid ? stableHits.Average() : unStableHits.Average();
                
                stableGround = stableHits.valid;
                unStableGround = unStableHits.valid;

                averageHit = new RayHitInfo
                {
                    col = average.col,
                    distance = average.distance,
                    normal = average.normal,
                    origin = castPointsWorld[0],
                    point = average.point,
                    valid = average.valid
                };
                
            }
        }

        #region Helper

        /// <summary>
        ///     Just a sample Validator functions
        /// </summary>
        /// <param name="hitInfo"></param>
        /// <returns>If the raycast should be considered valid</returns>
        private bool Hlp_RaycastValidationFunc(RayHitInfo hitInfo)
        {
            return !ignoreCollider.Contains(hitInfo.col);
        }

        /// <summary>
        ///     Special Validate function for sphere cast when (skinMultiplier != 1)
        /// </summary>
        /// <param name="hitInfo"></param>
        /// <param name="direction"></param>
        /// <param name="height"></param>
        /// <param name="dis">Actual distance from base</param>
        /// <param name="distance">Extra cast distance</param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private bool Hlp_ValidateSphereSkin(RayHitInfo hitInfo, Vector3 direction, float height, float radius,
            out float dis, float distance)
        {
            //Distance from bottom flat Disc
            var localHit = hitInfo.point - hitInfo.origin;
            dis = Vector3.Dot(direction, localHit);
            dis -= Hlp_UpdateRaycastLength(radius, height, (localHit - dis * direction).magnitude, 1);
            return !(dis > distance);
        }
        
        public int GetTotalRaycastPoints()
        {
            var points = 1;
            for (var i = 0; i < row; i++)
            for (int j = 0; j < column * (i + 1); j++)
                points++;
            return points;
        }
        
        /// <summary>
        ///     Returns an array containing the starting positions of all array rays (in local coordinates) based on the params
        ///     arguments;
        /// </summary>
        /// <param name="sensorRows"></param>
        /// <param name="sensorColumn"></param>
        /// <param name="offsetRows"></param>
        /// <param name="sensorRadius"></param>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static List<Vector3> Hlp_StoreRaycastPositionsLocal(int sensorRows, int sensorColumn, bool offsetRows,
            float sensorRadius, List<Vector3> positions)
        {
            //Initialize list used to store the positions;
            positions.Clear();

            //Add central start position to the list;
            var startPosition = Vector3.zero;
            positions.Add(startPosition);

            for (var i = 0; i < sensorRows; i++)
            {
                //Calculate radiusMultiplier for all positions on this row;
                var rowRadius = (float)(i + 1) / sensorRows;

                for (var j = 0; j < sensorColumn * (i + 1); j++)
                {
                    //Calculate angle (in degrees) for this individual position;
                    var angle = 360f / (sensorColumn * (i + 1)) * j;

                    //If 'offsetRows' is set to 'true', every other row is offset;
                    if (offsetRows && i % 2 == 0)
                        angle += 360f / (sensorColumn * (i + 1)) / 2f;

                    //Combine radiusMultiplier and angle into one position and add it to the list;
                    var x = rowRadius * Mathf.Cos(Mathf.Deg2Rad * angle);
                    var y = rowRadius * Mathf.Sin(Mathf.Deg2Rad * angle);

                    positions.Add(new Vector3(x, 0f, y) * sensorRadius);
                }
            }

            //Convert list to array and return array;
            return positions;
        }

        /// <summary>
        ///     Convert CastPoint stored in local space to world space
        /// </summary>
        /// <param name="local"></param>
        /// <param name="world"></param>
        /// <param name="height"></param>
        /// <param name="tr"></param>
        private void Hlp_ConvertLocalToWorld(Transform tr, IReadOnlyList<Vector3> local, IList<Vector3> world,
            float height)
        {
            for (var i = 0; i < local.Count; i++)
            {
                var pos = local[i];
                Hlp_ConvertLocalToWorld(tr, pos, out var worldPos, height);
                world[i] = worldPos;
            }
        }

        /// <summary>
        ///     Convert CastPoint stored in local space to world space
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="localPosition">Position in local space</param>
        /// <param name="worldPos">Variable to store position in global space</param>
        /// <param name="up">Up component in local space</param>
        private void Hlp_ConvertLocalToWorld(Transform tr, Vector3 localPosition, out Vector3 worldPos, float up = 0)
        {
            worldPos = tr.TransformPoint(localPosition + Vector3.up * up);
            //
            // worldPos = _right * localPosition.x +
            //            up * _up +
            //            _fwd * localPosition.z + tr.position;
        }


        /// <summary>
        ///     Store RaycastLengths in the given array
        /// </summary>
        /// <param name="distances"></param>
        /// <param name="localSpaceVector"></param>
        /// <param name="radius"></param>
        /// <param name="skinMul"></param>
        /// <param name="height"></param>
        /// <param name="count"></param>
        private void Hlp_UpdateRaycastLength(IList<float> distances, ICollection localSpaceVector, float radius, float skinMul,
            float height = 0, int count = 0)
        {
            count = count > 0 ? count : localSpaceVector.Count;
            for (var i = 0; i < count; i++)
                distances[i] = Hlp_UpdateRaycastLength(radius, height, castPointsLocal[i].magnitude, skinMul);
        }

        /// <summary>
        ///     Get the distance from cast point to character's bottom
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="height">Cast point's height from bottom hemisphere's center</param>
        /// <param name="distanceToCenter"></param>
        /// <param name="skinMul">Skin factor</param>
        /// <returns>distance from cast point to character's bottom</returns>
        private float Hlp_UpdateRaycastLength(float radius, float height, float distanceToCenter, float skinMul)
        {
            var radiusRatio = distanceToCenter / radius;
            var sine = Mathf.Sqrt((radius - distanceToCenter) / radius);
            var toEdge = sine * radius;
            toEdge *= skinMul + radiusRatio * (1 - skinMul);
            return toEdge + height;
        }

        #endregion
        
        [Serializable]
        private struct AverageGroundStats
        {
            public bool valid;
            public Collider col;
            public int hits;
            public Vector3 normal;
            public Vector3 point;
            public float distance;

            public AverageGroundStats Average()
            {
                normal /= hits;
                point /= hits;
                distance /= hits;
                return this;
            }
            
        }
        
        #endregion
    }
}