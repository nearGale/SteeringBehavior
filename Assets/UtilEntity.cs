using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameHelper
{
    public static void ResetEntitiesTargets()
    {
        foreach (var entity in EntityManager.Instance.Entities)
        {
            ResetEntityTargets(entity);
        }
    }

    public static void ResetEntityTargets(Entity entity)
    {
        float sqrDistanceMin = 0;
        ulong targetId = 0;

        foreach (var otherEntity in EntityManager.Instance.Entities)
        {
            if (otherEntity.teamId == entity.teamId) continue;

            var sqrDistance = (otherEntity.pos - entity.pos).sqrMagnitude;
            if (sqrDistanceMin == 0 || sqrDistance < sqrDistanceMin)
            {
                targetId = otherEntity.id;
                sqrDistanceMin = sqrDistance;
            }
        }

        entity.targetId = targetId;
    }

    public static void DrawLineToTargets()
    {
        foreach(var entity in EntityManager.Instance.Entities)
        {
            Entity targetEntity = EntityManager.Instance.GetEntity(entity.targetId);
            if(targetEntity != null)
            {
                Debug.DrawLine(entity.pos, targetEntity.pos, GetTeamColor(entity.teamId));
            }
        }
    }

    public static void DrawLine(Entity entity, Vector2 force, Color color)
    {
        force = force.normalized * 0.2f;
        Debug.DrawLine(entity.pos, entity.pos + force, color);
    }

    public static Color GetTeamColor(ETeam teamId)
    {
        switch (teamId)
        {
            case ETeam.Team1:
                return Color.blue;
            case ETeam.Team2:
                return Color.green;
            case ETeam.Team3:
                return Color.yellow;
            case ETeam.Team4:
                return Color.red;
            default:
                return Color.white;
        }
    }

    public static void SetEntityColor(ulong entityId, Color color)
    {
        Transform entityTr = EntityManager.Instance.GetEntityTr(entityId);
        entityTr.GetComponent<SpriteRenderer>().color = color;
    }
}
