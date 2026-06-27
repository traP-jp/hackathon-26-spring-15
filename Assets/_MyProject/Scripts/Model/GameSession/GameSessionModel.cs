using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace MyProject.Model
{
    public class GameSessionModel : IDisposable
    {
        public ReadOnlyReactiveProperty<int> Score => scoreModel.Value;
        readonly ScoreModel scoreModel;

        public Observable<Unit> Finished => finished;
        readonly Subject<Unit> finished = new();

        GameState state = GameState.Idol;

        public GameSessionModel(ScoreModel scoreModel)
        {
            this.scoreModel = scoreModel;
        }

        public void Initialize()
        {
            state = GameState.Preparing;

            scoreModel.Initialize();

            state = GameState.Ready;
        }

        public void Start()
        {
            if (state is not GameState.Ready)
            {
                throw new InvalidOperationException($"Cannot start game unless the game is ready. Current state: {state}");
            }

            state = GameState.Playing;
        }

        public void Pause()
        {
            if (state is not GameState.Playing)
            {
                throw new InvalidOperationException($"Cannot pause unless the game is playing. Current state: {state}");
            }

            state = GameState.Paused;
        }

        public void Resume()
        {
            if (state is not GameState.Paused)
            {
                throw new InvalidOperationException($"Cannot resume unless the game is paused. Current state: {state}");
            }

            state = GameState.Playing;
        }

        public void Finish()
        {
            if (state is not GameState.Playing)
            {
                throw new InvalidOperationException($"Cannot finish unless the game is playing. Current state: {state}");
            }

            state = GameState.Finished;
            finished.OnNext(Unit.Default);
        }

        public async UniTask SaveAsync(CancellationToken ct)
        {
            if (state is not GameState.Finished)
            {
                throw new InvalidOperationException($"Cannot save unless the game is finished. Current state: {state}");
            }

            await scoreModel.SaveAsync(ct);
        }

        public void AddScore(int amount)
        {
            if (state is not GameState.Playing)
            {
                throw new InvalidOperationException($"Cannot add score unless the game is playing. Current state: {state}");
            }

            scoreModel.Add(amount);
        }

        public void Dispose()
        {
            finished.OnCompleted();
            finished.Dispose();
        }
    }
}
