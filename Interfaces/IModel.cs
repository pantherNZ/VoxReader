namespace VoxReader.Interfaces
{
    public interface IModel
    {
        Vector3 Size { get; }
        
        Voxel[] Voxels { get; }
    }
}