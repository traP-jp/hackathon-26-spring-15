using MyProject.View;
using MyProject.Model;
using MyProject.Director;
using MyProject.Infrastructure;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using unityroom.Api;

namespace MyProject.CompositionRoot
{
    public class MainLifeTimeScope : LifetimeScope
    {
        [Header("View")]
        [SerializeField] RootViewHub rootViewHub;
        [SerializeField] TitleViewHub titleViewHub;
        [SerializeField] GameViewHub gameViewHub;
        [SerializeField] ResultViewHub resultViewHub;

        [Header("Config")]
        [SerializeField] GameConfigSO gameConfig;

        [Header("Infrastructure")]
        [SerializeField] UnityroomApiClient unityroomApiClient;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterModel(builder);
            RegisterView(builder);
            RegisterDirector(builder);
            RegisterInfrastructure(builder);
        }

        void RegisterModel(IContainerBuilder builder)
        {
            builder.Register<GameSessionModel>(Lifetime.Singleton);
            builder.Register<ScoreModel>(Lifetime.Singleton);
            builder.RegisterInstance(gameConfig);
        }

        void RegisterView(IContainerBuilder builder)
        {
            builder.RegisterInstance(rootViewHub);
            builder.RegisterInstance(titleViewHub);
            builder.RegisterInstance(gameViewHub);
            builder.RegisterInstance(resultViewHub);
        }

        void RegisterDirector(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MainEntryPoint>(Lifetime.Singleton);
            builder.Register<RootDirector>(Lifetime.Singleton);
            builder.Register<TitleDirector>(Lifetime.Singleton);
            builder.Register<GameDirector>(Lifetime.Singleton);
            builder.Register<ResultDirector>(Lifetime.Singleton);
        }

        void RegisterInfrastructure(IContainerBuilder builder)
        {
            builder.Register<PlayerPrefsSaveDataRepository>(Lifetime.Singleton)
                .As<ISaveDataRepository>();
            builder.Register<UnityroomRankingRegisterer>(Lifetime.Singleton)
                .As<IRankingRegisterer>();
            builder.RegisterInstance(unityroomApiClient);
        }
    }
}
