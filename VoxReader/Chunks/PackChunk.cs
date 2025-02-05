using VoxReader.Interfaces;

namespace VoxReader.Chunks
{
    public class PackChunk : Chunk, IPackChunk
    {
        public int ModelCount { get; }

        public PackChunk(byte[] data) : base(data)
        {
            var formatParser = new FormatParser(Content);

            ModelCount = formatParser.ParseInt32();
        }
    }
}