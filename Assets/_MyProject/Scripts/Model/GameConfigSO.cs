using UnityEngine;

namespace MyProject.Model
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MyProject/GameConfigSO")]
    public class GameConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public SceneType InitialSceneType { get; private set; } = SceneType.Title;

        [field: SerializeField, Min(1)]
        public int PlayerMaxHp { get; private set; } = 100;
    }
}
