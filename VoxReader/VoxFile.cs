using VoxReader.Chunks;
using VoxReader.Interfaces;

namespace VoxReader
{
    internal class VoxFile : IVoxFile
    {
        public int VersionNumber { get; }
        public IModel[] Models { get; }
        public IPalette Palette { get; }
        public RootChunk ChunkRoot { get; }
        public string FilePath { get; }

        internal VoxFile(int versionNumber, IModel[] models, IPalette palette, RootChunk root, string filePath )
        {
            VersionNumber = versionNumber;
            Models = models;
            Palette = palette;
            ChunkRoot = root;
            FilePath = filePath;
        }
    }
}