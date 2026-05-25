using System;
using System.IO;
namespace SoochakBharat.SDK.Utils
{
    public class PacketLogger
    {
        private readonly string _path;
        public PacketLogger(string path) { _path = path; }
        public void Log(string direction, byte[] data, int length)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            using var sw = new StreamWriter(_path, append: true);
            sw.WriteLine($"[{DateTime.UtcNow:o}] {direction} {length} bytes");
            sw.WriteLine(BitConverter.ToString(data, 0, length));
        }
    }
}
