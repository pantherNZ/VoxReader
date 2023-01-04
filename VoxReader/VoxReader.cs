/* Reference:
https://github.com/ephtracy/voxel-model/blob/master/MagicaVoxel-file-format-vox.txt
https://github.com/ephtracy/voxel-model/blob/master/MagicaVoxel-file-format-vox-extension.txt
*/

using System;
using System.IO;
using System.Linq;
using VoxReader.Chunks;
using VoxReader.Extensions;
using VoxReader.Interfaces;

namespace VoxReader
{
    /// <summary>
    /// Used to read data from .vox files.
    /// </summary>
    public static class VoxReader
    {
        /// <summary>
        /// Reads the file at the provided path.
        /// </summary>
        public static IVoxFile Read(string filePath)
        {
            byte[] data = File.ReadAllBytes(filePath);
            return Read(filePath, data);
        }
        
        /// <summary>
        /// Reads the data from the provided byte array.
        /// </summary>
        public static IVoxFile Read(byte[] data)
        {
            return Read( null, data );
        }

        private static IVoxFile Read( string filePath, byte[] data )
        {
            int versionNumber = BitConverter.ToInt32( data, 4 );

            var mainChunk = new RootChunk( data.GetRange( 8 ) );

            var palette = new Palette( mainChunk.GetChild<IPaletteChunk>().Colors );

            var models = Helper.ExtractModels( mainChunk, palette ).ToArray();

            return new VoxFile( versionNumber, models, palette, mainChunk, filePath );
        }
    }
}