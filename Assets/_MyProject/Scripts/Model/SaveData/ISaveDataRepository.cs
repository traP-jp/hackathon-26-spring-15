using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyProject.Model
{
    public interface ISaveDataRepository
    {
        UniTask SaveScoreAsync(ScoreSaveData saveData, CancellationToken ct);
        UniTask<ScoreSaveData> LoadScoreAsync(CancellationToken ct);
    }
}
