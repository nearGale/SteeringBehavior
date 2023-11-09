using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "ScriptableOject/BuildConfig")]
public class UtilCreateSettingScript : ScriptableObject
{
    public float forcePursuit = 1;
    public float forceAvoidence = 2;
    public float forceSteeringMax = 1;
    public float mass = 1;
    public float maxSpeed = 1;
    public float stopRadius = 1;
    public float collisionAngleMax = 160;
    public float repickTargetInterval = 0.5f;
    public string createEntity = "";
    public string backupFormation1 = "1:-5,-2,-2,2;2:2,5,-2,2;3:-2,2,-5,-2;4:-2,2,2,5";
    public string backupFormation2 = "1:-5,5,-4,-2;2:-5,5,2,4";
}