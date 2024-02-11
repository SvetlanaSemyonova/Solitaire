using UnityEngine;

namespace Solitaire.Services
{
    public class StorageService : IStorageService
    {
        public void Save<T>(string key, T obj)
        {
            if (string.IsNullOrEmpty(key) || obj == null)
            {
                return;
            }

            var json = JsonUtility.ToJson(obj);

            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public T Load<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default;
            }

            var json = PlayerPrefs.GetString(key);

            return JsonUtility.FromJson<T>(json);
        }
    }
}
