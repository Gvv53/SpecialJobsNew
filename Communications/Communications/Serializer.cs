using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Communications
{
    public class Serializer<T>
    {
        public void ToFile(string path, T data)
        {
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                new BinaryFormatter().Serialize(fs, data);
            }
        }

        public T FromFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                return (T)new BinaryFormatter().Deserialize(fs);
            }
        }

        public void ToStream(Stream stream, T data)
        {
            new BinaryFormatter().Serialize(stream, data);
        }

        public T FromStream(Stream stream)
        {
            return (T)new BinaryFormatter().Deserialize(stream);
        }

        public byte[] ToBytes(T data)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, data);
                return ms.ToArray();
            }
        }

        public T FromBytes(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return (T)new BinaryFormatter().Deserialize(ms);
            }
        }
    }
}
