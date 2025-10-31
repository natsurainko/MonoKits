using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoKits.Extensions;

public static class ModelExtensions
{
    public static BoundingBox CreateBoundingBoxFromVertices(this Model model)
    {
        Vector3 min = new(float.MaxValue);
        Vector3 max = new(float.MinValue);

        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (ModelMeshPart meshPart in mesh.MeshParts)
            {
                int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                int vertexCount = meshPart.NumVertices;

                byte[] vertexData = new byte[vertexCount * vertexStride];
                meshPart.VertexBuffer.GetData(
                    meshPart.VertexOffset * vertexStride,
                    vertexData,
                    0,
                    vertexCount * vertexStride
                );

                for (int i = 0; i < vertexCount; i++)
                {
                    int offset = i * vertexStride;
                    Vector3 position = new(
                        BitConverter.ToSingle(vertexData, offset),
                        BitConverter.ToSingle(vertexData, offset + 4),
                        BitConverter.ToSingle(vertexData, offset + 8)
                    );

                    min = Vector3.Min(min, position);
                    max = Vector3.Max(max, position);
                }
            }
        }

        return new BoundingBox(min, max);
    }
}
