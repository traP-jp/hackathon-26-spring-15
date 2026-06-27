using UnityEngine;

namespace MyProject.Model
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MyProject/GameConfigSO")]
    public class GameConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public SceneType InitialSceneType { get; private set; } = SceneType.Title;
    }
}
