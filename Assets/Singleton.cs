using System;

public interface ISingletonInstance
{
    void OnCreate();
    void OnInit();
    void OnClean();
}

public abstract class Singleton<T> where T : Singleton<T>, new()
{
    private static T m_Instance;
    public Singleton() { }
    public static T Instance
    {
        get
        {
            if (null == m_Instance)
            {
                m_Instance = new T();
                m_Instance.Init();
            }
            return m_Instance;
        }
    }

    public void Init()
    {
        OnInit();
    }

    protected abstract void OnInit();
    
    public void Clean()
    {
        OnClean();
    }

    protected abstract void OnClean();
}