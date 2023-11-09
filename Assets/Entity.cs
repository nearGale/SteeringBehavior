using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity
{
    public ulong id;
    public ulong targetId;
    public ETeam teamId;
    public float radius;
    public Vector2 pos;
    public Vector2 velocity;
    public Vector2 steering;
    public Vector2 steeringPursuit;
    public Vector2 steeringCollision;
    public bool collisionBlock;
}
