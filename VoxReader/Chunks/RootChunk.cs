using System.Linq;
using VoxReader.Interfaces;

namespace VoxReader.Chunks
{
    public class RootChunk : IChunk
    {
        public ChunkType Type { get; }
        public byte[] Content { get; }
        public IChunk[] Children { get; }
        public int TotalBytes { get; }

        public RootChunk(byte[] data)
        {
            ChunkType id = Chunk.GetChunkId( data );
            Type = ChunkType.Root;
            Children = ChunkFactory.Parse( data ).Children;
            TotalBytes = Children.Sum( x => x.TotalBytes );
        }

        public T GetChild<T>() where T : class, IChunk
        {
            return Children.FirstOrDefault( c => c is T ) as T;
        }

        public IChunk[] GetChildren( ChunkType chunkType )
        {
            return Children.Where( chunk => chunk.Type == chunkType ).ToArray();
        }

        public T[] GetChildren<T>() where T : class, IChunk
        {
            return Children.Where( c => c is T ).Cast<T>().ToArray();
        }
    }
}