using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyProject.Model
{
    public interface IRankingRegisterer
    {
        UniTask RegisterAsync(RankingData rankingData, CancellationToken ct);
    }
}
