using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public partial class GameHelper
{
    public static UtilCreateSettingScript GetSetting()
    {
        UtilCreateSettingScript buildConfigTool = Resources.Load<UtilCreateSettingScript>("Setting");
        return buildConfigTool;
    }

    public static void MoveToTarget(Entity entity, ulong targetId)
    {
        //var startTime = DateTime.Now;

        // Pursuit sb.
        var target = EntityManager.Instance.GetEntity(targetId);
        if (target == null)
        {
            return;
        }

        var myPos = entity.pos;
        var myV = entity.velocity;
        DrawLine(entity, entity.velocity, Color.yellow);

        var targetPos = target.pos;
        var stopPos = targetPos - entity.pos;
        var distance = Vector2.Distance(target.pos, entity.pos);

        if (distance <= GetSetting().stopRadius)
        {
            entity.velocity = Vector2.zero;
            SetEntityColor(entity.id, GetTeamColor(entity.teamId) + Color.gray);
            entity.targetId = 0;
            entity.collisionBlock = false;
            return;
        }

        stopPos = stopPos.normalized * GetSetting().stopRadius;
        var desiredPos = targetPos - stopPos;

        bool needPursuit = !entity.collisionBlock;
        if (needPursuit)
        {
            var steeringPursuit = Pursuit(
                entity.pos,
                entity.velocity,
                GetSetting().maxSpeed,
                desiredPos,
                target.velocity,
                GetSetting().forcePursuit
            );
            entity.steering += steeringPursuit;
            entity.steeringPursuit = steeringPursuit;
        }
        else
        {
            entity.steeringPursuit = Vector2.zero;
            entity.velocity = Vector2.zero;
        }

        var steeringCollision = CollisionAvoidance(entity);
        entity.steering += steeringCollision;
        DrawLine(entity, steeringCollision, Color.black);

        var finalPos = CalMovePos(entity, GetSetting().maxSpeed);

        if(BattleUnitPosHasCollision(entity, finalPos, out var collisionForce))
        {
            Debug.Log($"PosHasCollision: entity:{entity.id} steering:{entity.steering} pos:{finalPos}");
            entity.collisionBlock = true;
            entity.steering += collisionForce;
            return;
        }
        entity.pos = finalPos;
        entity.collisionBlock = false;

        //var endTime = DateTime.Now;
        //var ts = endTime.Subtract(startTime);

        //if (ts.TotalMilliseconds > 5)
        //{
        //    Debug.Log($"ticktick_MoveToTarget_Server: " +
        //        $"entity:{entity.id} " +
        //        $"tick:{gameTimer.FrameTick} " +
        //        $"cost:{ts.TotalMilliseconds}ms " +
        //        $"tarPos:{target.pos} " +
        //        $"myPos:{myPos} " +
        //        $"desiredPos:{desiredPos} " +
        //        $"finalPos:{finalPos} " +
        //        $"targetV:{target.velocity} " +
        //        $"myV:{myV} " +
        //        $"finalV:{entity.velocity} " +
        //        $"steering:{entity.steering} " +
        //        $"steeringPursuit:{steeringPursuit} " +
        //        $"steeringCollision:{steeringCollision} ");
        //}

    }

    /// <summary>
    /// 向目标的未来位置追赶，计算转向力。
    /// </summary>
    /// <param name="targetPos">目标当前位置</param>
    /// <param name="targetV">目标当前速度</param>
    /// <returns>此行为得到的转向力</returns>
    public static Vector2 Pursuit(Vector2 curPos, Vector2 curV, float maxSpeed,
            Vector2 targetPos, Vector2 targetV, float forcePursuit, bool pursuitFuturePos = false)
    {
        var pursuitPos = targetPos;
        if (pursuitFuturePos)
        {
            float distance = (targetPos - curPos).magnitude;

            float time = maxSpeed == 0 ? 0 : distance / maxSpeed;
            var futurePosition = targetPos + targetV * time;
            pursuitPos = futurePosition;
        }

        var force = Seek(curPos, curV, maxSpeed, pursuitPos, forcePursuit);

        return force;
    }

    /// <summary>
    /// 向目标位置行进时，计算转向力。
    /// </summary>
    /// <param name="targetPos">目标位置</param>
    /// <param name="slowingRadius">距离目标开始减速的距离</param>
    /// <returns></returns>
    public static Vector2 Seek(Vector2 curPos, Vector2 curV, float maxSpeed, 
        Vector2 targetPos, float forceSeek = 1f, float slowingRadius = 1f)
    {
        Vector2 desired = targetPos - curPos;

        float distance = desired.magnitude;
        desired = desired.normalized;

        if (distance <= slowingRadius)
        {
            desired *= maxSpeed * distance / slowingRadius;
        }
        else
        {
            desired *= maxSpeed;
        }

        Vector2 force = desired - curV;

        if (force.magnitude > forceSeek)
        {
            force = force.normalized * forceSeek;
        }
        return force;
    }


    /// <summary>
    /// 检测运动方向上的障碍物，添加避免碰撞的力。
    /// </summary>
    /// <returns></returns>
    public static Vector2 CollisionAvoidance(Entity entity)
    {
        //var startTime = DateTime.Now;

        var dir = entity.velocity + entity.steering / GetSetting().mass;

        var avoidanceForce = Vector2.zero;

        for (var scale = 0.25f; scale <= 1; scale += 0.25f)
        {
            // check forward & near
            var tmpDir = dir * scale;
            var aheadPos = entity.pos+ tmpDir;
            avoidanceForce = GetCollisionAvoidanceForce(entity, aheadPos);
            if (avoidanceForce.magnitude > 0)
            {
                break;
            }
        }

        //GameHelper.Log($"TickTick_CollisionAvoidance ett:{entity.id} force:{avoidanceForce}");

        entity.steeringCollision = avoidanceForce;

        //var endTime = DateTime.Now;
        //var ts = endTime.Subtract(startTime);

        //if (ts.TotalMilliseconds > 5)
        //{
        //    Debug.Log($"ticktick_CollisionAvoidance_Server: " +
        //        $"entity:{entity.id} " +
        //        $"cost:{ts.TotalMilliseconds}ms");
        //}

        return avoidanceForce;
    }

    private static Vector2 GetCollisionAvoidanceForce(Entity entity, Vector2 aheadPos)
    {
        //var startTime = DateTime.Now;
        //DateTime endTime;
        //TimeSpan ts;

        var idx = 0;
        foreach (var otherEntity in EntityManager.Instance.Entities)
        {
            var hit = false;
            if (otherEntity.id == entity.id) continue;

            if (otherEntity.id == entity.targetId) continue;

            var distanceSquared = (otherEntity.pos - aheadPos).sqrMagnitude;
            var radius = (otherEntity.radius + otherEntity.radius);
            var radiusSquared = radius * radius;

            if (distanceSquared < radiusSquared)
            {
                hit = true;
            }

            if (hit)
            {
                Vector2 avoidance;
                //var avoidanceForceBasic = aheadPos - otherEntity.pos;
                //var angle = VectorCalAngle(entity.steeringPursuit, avoidanceForceBasic);
                //var avoidanceForceLeft = CalculatePerpendicular(aheadPos - otherEntity.pos);

                //if (angle < 20 || angle > 160)
                //{
                //    avoidance = avoidanceForceLeft;
                //}
                //else
                //{
                //    avoidance = avoidanceForceBasic;
                //}
                
                
                var avoidanceForceLeft = CalculatePerpendicular(aheadPos - otherEntity.pos);
                var angle = VectorCalAngle(entity.steeringPursuit, avoidanceForceLeft);

                bool useLeftForce = Vector2.Dot(entity.steeringCollision, avoidanceForceLeft) >= 0;
                if (useLeftForce)
                {
                    avoidance = avoidanceForceLeft;
                }
                else
                {
                    avoidance = CalculatePerpendicular(otherEntity.pos - entity.pos, true);
                }


                var avoidanceForce = avoidance.normalized * GetSetting().forceAvoidence;

                //endTime = DateTime.Now;
                //ts = endTime.Subtract(startTime);

                //if (ts.TotalMilliseconds > 5)
                //{
                Debug.Log("GetCollisionAvoidanceForce: " +
                    $"entity:{entity.id} " +
                    $"tarEtt{otherEntity.id} " +
                    $"pos:{entity.pos} " +
                    $"tarPos:{otherEntity.pos} " +
                    $"hitPos:{aheadPos} " +
                    $"avoidanceForce:{avoidanceForce}");
                //}

                return avoidanceForce;
            }

            idx++;
        }

        //endTime = DateTime.Now;
        //ts = endTime.Subtract(startTime);
        //if (ts.TotalMilliseconds > 5)
        //{
        //    var pipeId = GetBattleServerId(game.EntityManager);
        //    Log(pipeId,
        //        "ticktick_GetCollisionAvoidanceForce_Server: " +
        //        $"entity:{entity.id} " +
        //        $"restBUnitCount: {battleMapComponent.RestBattleUnits.Count} " +
        //        "curIdx: FULL! " +
        //        $"cost:{ts.TotalMilliseconds}ms");
        //}
        return Vector2.zero;
    }

    private static bool BattleUnitPosHasCollision(Entity entity, Vector2 aheadPos, out Vector2 force)
    {
        force = Vector2.zero;

        foreach (var otherEntity in EntityManager.Instance.Entities)
        {
            if (otherEntity.id == entity.id) continue;

            if (otherEntity.id == entity.targetId) continue;

            var distanceSquared = (otherEntity.pos - aheadPos).sqrMagnitude;
            var radius = otherEntity.radius + entity.radius;
            var radiusSquared = radius * radius;

            bool hit = distanceSquared < radiusSquared;
            if (hit)
            {
                force = entity.pos - otherEntity.pos;
                return true;
            }
        }

        return false;
    }

    public static Vector2 CalMovePos(Entity entity, float speed)
    {
        var v = entity.velocity;
        var pos = entity.pos;

        var tempSteering = entity.steering;
        VectorTruncate(ref tempSteering, GetSetting().forceSteeringMax);
        entity.steering = tempSteering;
        entity.steering *= 1 / GetSetting().mass;

        v += entity.steering;

        var distance = MathF.Min(speed * Time.deltaTime, v.magnitude);
        VectorTruncate(ref v, distance);

        entity.velocity = v;

        pos += v;

        Debug.Log($"CalMovePos: entity:{entity.id} steering:{entity.steering} pos:{pos} v:{v}");
        return pos;
    }

}
