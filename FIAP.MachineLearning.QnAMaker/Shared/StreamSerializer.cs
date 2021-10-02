using Newtonsoft.Json;
using System.IO;

namespace FIAP.MachineLearning.QnAMaker.Shared
{
    internal static class StreamSerializer
    {
        private static readonly JsonSerializer Serializer = new();

        public static MemoryStream Serialize<T>(T obj) where T : class
        {
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            var jsonWriter = new JsonTextWriter(streamWriter);

            Serializer.Serialize(jsonWriter, obj);

            return memoryStream;
        }

        public static T Deserialize<T>(Stream stream) where T : class
        {
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);

            return Serializer.Deserialize<T>(jsonReader);
        }
    }
}