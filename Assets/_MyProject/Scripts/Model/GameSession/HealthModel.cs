using System;
using R3;

namespace MyProject.Model
{
    public class HealthModel : IDisposable
    {
        public ReadOnlyReactiveProperty<int> Value => value;
        public Observable<Unit> Died => died;

        readonly GameConfigSO gameConfig;
        readonly ReactiveProperty<int> value;
        readonly Subject<Unit> died = new();

        public HealthModel(GameConfigSO gameConfig)
        {
            this.gameConfig = gameConfig;
            value = new ReactiveProperty<int>(gameConfig.PlayerMaxHp);
        }

        public void Initialize()
        {
            value.Value = gameConfig.PlayerMaxHp;
        }

        public void Damage(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            var previous = value.Value;
            value.Value = Math.Max(0, previous - amount);

            if (previous > 0 && value.Value <= 0)
            {
                died.OnNext(Unit.Default);
            }
        }

        public void Dispose()
        {
            value.Dispose();
            died.OnCompleted();
            died.Dispose();
        }
    }
}
