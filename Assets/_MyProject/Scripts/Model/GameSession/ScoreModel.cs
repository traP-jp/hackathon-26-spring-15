using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace MyProject.Model
{
    public class ScoreModel : IDisposable
    {
        public ReadOnlyReactiveProperty<int> Value => value;
        readonly ReactiveProperty<int> value = new(0);

        readonly ISaveDataRepository saveDataRepository;
        readonly IRankingRegisterer rankingRegisterer;

        public ScoreModel(ISaveDataRepository saveDataRepository, IRankingRegisterer rankingRegisterer)
        {
            this.saveDataRepository = saveDataRepository;
            this.rankingRegisterer = rankingRegisterer;
        }

        public void Initialize()
        {
            value.Value = 0;
        }

        public void Add(int amount)
        {
            value.Value += amount;
        }

        public async UniTask SaveAsync(CancellationToken ct)
        {
            var saveData = new ScoreSaveData(value.Value);
            var rankingData = new RankingData(value.Value);

            await UniTask.WhenAll
            (
                saveDataRepository.SaveScoreAsync(saveData, ct),
                rankingRegisterer.RegisterAsync(rankingData, ct)
            );
        }

        public void Dispose()
        {
            value.Dispose();
        }
    }
}
