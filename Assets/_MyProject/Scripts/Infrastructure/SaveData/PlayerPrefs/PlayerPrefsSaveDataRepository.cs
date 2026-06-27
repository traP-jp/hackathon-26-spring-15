using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Model;
using UnityEngine;
using Newtonsoft.Json;

namespace MyProject.Infrastructure
{
    public class PlayerPrefsSaveDataRepository : ISaveDataRepository
    {
        const string ScoreKey = "score_data";

        public UniTask SaveScoreAsync(ScoreSaveData saveData, CancellationToken ct)
        {
            return SaveAsync(ScoreKey, saveData, ct);
        }

        public UniTask<ScoreSaveData> LoadScoreAsync(CancellationToken ct)
        {
            return LoadAsync(ScoreKey, new ScoreSaveData(), ct);
        }

        UniTask SaveAsync<T>(string key, T saveData, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var json = JsonConvert.SerializeObject(saveData);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();

            Debug.Log($"[PlayerPrefsSaveDataRepository] Saved data. key={key}, length={json.Length}");

            return UniTask.CompletedTask;
        }

        UniTask<T> LoadAsync<T>(string key, T defaultValue, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (!PlayerPrefs.HasKey(key))
            {
                return UniTask.FromResult(defaultValue);
            }

            var json = PlayerPrefs.GetString(key);
            var saveData = JsonConvert.DeserializeObject<T>(json);
            if (saveData is null)
            {
                throw new InvalidOperationException($"Failed to deserialize save data. key={key}");
            }

            Debug.Log($"[PlayerPrefsSaveDataRepository] Loaded data. key={key}, length={json.Length}");

            return UniTask.FromResult(saveData);
        }
    }
}
