using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private Transform _rootEntity;
    private float _time;

    private void Awake()
    {
        _rootEntity = GameObject.Find("RootEntity").transform;
        EntityManager.Instance.Init();
        GameObject.Find("Button").GetComponent<Button>().onClick.AddListener(
            () => 
            {
                EntityManager.Instance.Clean();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
            });
    }

    void Start()
    {
        CreateEntities();
        GameHelper.ResetEntitiesTargets();
    }

    void Update()
    {
        EntityManager.Instance.EntitiesMoveToTargets();
        EntityManager.Instance.EntitiesPosRedraw();
        GameHelper.DrawLineToTargets();

        _time += Time.deltaTime;
        if(_time > GameHelper.GetSetting().repickTargetInterval)
        {
            _time = 0;
            GameHelper.ResetEntitiesTargets();
        }
    }

    private void CreateEntities()
    {
        string[] createTeams = GameHelper.GetSetting().createEntity.Split(';');  // 1:-5,0,-5,0;2:5,10,-5,0
        foreach (string team in createTeams)
        {
            string[] teamStr = team.Split(":"); // 1:-5,0,-5,0
            int teamId = int.Parse(teamStr[0]);
            string[] posStr = teamStr[1].Split(","); // -5,0,-5,0

            for (int i = int.Parse(posStr[0]); i < int.Parse(posStr[1]); i++)
            {
                for (int j = int.Parse(posStr[2]); j < int.Parse(posStr[3]); j++)
                {
                    ETeam eTeam = (ETeam)teamId;
                    EntityManager.Instance.CreateEntity(eTeam, new Vector2(i, j), _rootEntity);
                }
            }
        }
    }

}
