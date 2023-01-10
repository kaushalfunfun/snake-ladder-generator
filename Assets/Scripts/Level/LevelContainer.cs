using UnityEngine;

public class LevelContainer : ScriptableObject
{
    public LevelData levelData;

    public void SetData(LevelData _levelData)
    {
        levelData = _levelData;
    }
}
