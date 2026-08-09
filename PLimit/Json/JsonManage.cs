using System.Text.Json;

namespace PLimit.Json
{
    public static class JsonManage
    {
        private static readonly object FileLock = new();
        private static readonly JsonSerializerOptions IndentedJsonOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>
        /// Reads and deserializes a JSON file.
        /// </summary>
        public static T ReadJsonFromFile<T>(string filePath)
        {
            lock (FileLock)
                return ReadJsonFromFileCore<T>(filePath);
        }

        /// <summary>
        /// Creates or replaces a JSON file.
        /// </summary>
        public static void CreateJsonFile(string filePath, object value)
        {
            lock (FileLock)
                WriteJsonToFile(filePath, value);
        }

        /// <summary>
        /// Appends one item to a JSON array.
        /// </summary>
        public static void UpdateJsonFile<T>(string filePath, T item)
        {
            lock (FileLock)
            {
                if (!File.Exists(filePath))
                {
                    WriteJsonToFile(filePath, new[] { item });
                    return;
                }

                var items = ReadJsonFromFileCore<T[]>(filePath).ToList();
                items.Add(item);
                WriteJsonToFile(filePath, items);
            }
        }

        /// <summary>
        /// Reads, updates, and rewrites a JSON document while holding one lock so
        /// concurrent settings changes cannot overwrite one another.
        /// </summary>
        public static void UpdateJsonFileParameter<T>(string filePath, Action<T> updateAction)
            where T : new()
        {
            ArgumentNullException.ThrowIfNull(updateAction);

            lock (FileLock)
            {
                T data;
                if (!File.Exists(filePath))
                {
                    data = new T();
                }
                else
                {
                    string json = File.ReadAllText(filePath);
                    data = string.IsNullOrWhiteSpace(json)
                        ? new T()
                        : JsonSerializer.Deserialize<T>(json) ?? new T();
                }

                updateAction(data);
                WriteJsonToFile(filePath, data, IndentedJsonOptions);
            }
        }

        /// <summary>
        /// Removes matching values from a JSON array.
        /// </summary>
        public static void DeleteJsonData<T>(
            string filePath,
            Func<IEnumerable<T>, IEnumerable<T>> toRemove)
        {
            ArgumentNullException.ThrowIfNull(toRemove);

            lock (FileLock)
            {
                if (!File.Exists(filePath))
                    return;

                var items = ReadJsonFromFileCore<T[]>(filePath).ToList();
                var removedItems = toRemove(items).ToHashSet();
                items.RemoveAll(removedItems.Contains);
                WriteJsonToFile(filePath, items);
            }
        }

        private static T ReadJsonFromFileCore<T>(string filePath) =>
            JsonSerializer.Deserialize<T>(File.ReadAllText(filePath))
            ?? throw new InvalidDataException($"The JSON file '{filePath}' did not contain a value.");

        private static void WriteJsonToFile(
            string filePath,
            object value,
            JsonSerializerOptions? options = null)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(filePath, JsonSerializer.Serialize(value, options));
        }
    }
}
