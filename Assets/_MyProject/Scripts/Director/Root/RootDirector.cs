using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.View;

namespace MyProject.Director
{
    public class RootDirector
    {
        readonly RootViewHub rootViewHub;

        public RootDirector(RootViewHub rootViewHub)
        {
            this.rootViewHub = rootViewHub;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            rootViewHub.Initialize();
            await UniTask.CompletedTask;
        }

        public void Tick()
        {

        }
    }
}
