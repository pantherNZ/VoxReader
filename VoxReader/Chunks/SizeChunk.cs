using VoxReader.Interfaces;

namespace VoxReader.Chunks
{
    public class SizeChunk : Chunk, ISizeChunk
    {
        public Vector3 Size { get; }

        public SizeChunk(byte[] data) : base(data)
        {
            var formatParser = new FormatParser(Content);

            Size = formatParser.ParseVector3();
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {Size}";
        }
    }
}