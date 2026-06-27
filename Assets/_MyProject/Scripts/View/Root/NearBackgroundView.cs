using UnityEngine;

namespace MyProject.View
{
    public class NearBackgroundView : MonoBehaviour
    {
        [SerializeField]
        GameObject grid;
        [SerializeField, Range(0f, 1f)]
        float gridDisplayProbability = 0.2f;

        public void RandomizeGridVisibility()
        {
            grid.SetActive(UnityEngine.Random.value < gridDisplayProbability);
        }
    }
}
