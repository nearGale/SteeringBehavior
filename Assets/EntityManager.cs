using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityManager : Singleton<EntityManager>
{
    private Dictionary<ulong, Entity> _entities = new();
    private Dictionary<ulong, Transform> _entityTrs = new();
    private ulong _curIdx = 0;

    public List<Entity> Entities => _entities.Values.ToList();

    protected override void OnInit()
    {
    }

    protected override void OnClean()
    {
        _entities.Clear();
        _entityTrs.Clear();
    }

    public Entity CreateEntity(ETeam teamId, Vector2 pos, Transform root)
    {
        float radius = 0.4f;
        _curIdx++;
        Entity entity = new();
        entity.id = _curIdx;
        entity.teamId = teamId;
        entity.pos = pos;
        entity.radius = radius;

        GameObject entityPrefab = Resources.Load<GameObject>("Entity");
        GameObject entityObj = GameObject.Instantiate(entityPrefab, root);
        entityObj.transform.localPosition = pos;
        entityObj.transform.localScale = Vector3.one * radius * 2;
        entityObj.transform.name = $"Entity{_curIdx}";

        entityObj.GetComponent<SpriteRenderer>().color = GameHelper.GetTeamColor(teamId);

        _entities.Add(_curIdx, entity);
        _entityTrs.Add(_curIdx, entityObj.transform);

        return entity;
    }

    public Entity GetEntity(ulong id)
    {
        _entities.TryGetValue(id, out Entity entity);
        return entity;
    }

    public Transform GetEntityTr(ulong id)
    {
        _entityTrs.TryGetValue(id, out Transform tr);
        return tr;
    }

    public void EntitiesMoveToTargets()
    {
        foreach (var entity in Entities)
        {
            GameHelper.MoveToTarget(entity, entity.targetId);
        }
    }

    public void EntitiesPosRedraw()
    {
        foreach (var kvPair in _entityTrs)
        {
            Entity entity = _entities[kvPair.Key];
            kvPair.Value.localPosition = entity.pos;
        }
    }
}
