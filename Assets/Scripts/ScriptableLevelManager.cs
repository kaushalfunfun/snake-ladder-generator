using UnityEngine;
using UnityEditor;
namespace SweetSugar.Scripts.System
{
    public static class ScriptableLevelManager
    {
#if UNITY_EDITOR
        public static void CreateOrSaveFileLevel(LevelData _levelData)
        {
            var path = "Assets/Resources/Levels/";

            if (Resources.Load("Levels/" + _levelData.levelName))
            {
                Debug.Log("file already exists!");
                SaveLevel(path, _levelData);
            }
            else
            {
                Debug.Log("file does not exists!");
                string fileName = _levelData.levelName;
                var newLevelData = ScriptableObjectUtility.CreateAsset<LevelContainer>(path, fileName);
                newLevelData.SetData(_levelData.DeepCopy());
                EditorUtility.SetDirty(newLevelData);
                AssetDatabase.SaveAssets();
            }
        }
        public static void SaveLevel(string path, LevelData _levelData)
        {
            var levelScriptable = Resources.Load("Levels/"+_levelData.levelName) as LevelContainer;
            if (levelScriptable != null)
            {
                Debug.Log("file loaded successfully!");
                levelScriptable.SetData(_levelData.DeepCopy());
                EditorUtility.SetDirty(levelScriptable);
            }

            AssetDatabase.SaveAssets();            
        }
#endif

        public static LevelData LoadLevel(int level)
        {
            var levelScriptable = Resources.Load("Levels/Level_" + level) as LevelContainer;
            LevelData levelData = null;
            if (levelScriptable)
            {
                levelData = levelScriptable.levelData.DeepCopy();
            }

            return levelData;
        }
    }
}