using BepuPhysics;
using BepuPhysics.Collidables;
using Microsoft.Xna.Framework.Graphics;
using MonoKits.Spatial3D;
using MonoKits.Spatial3D.Physics;
using MonoKits.Spatial3D.Physics.Interfaces;
using System.Numerics;

namespace MonoKits.Extensions;

public static class PhysicsExtensions
{
    public static void InitializeStatic<TGameObject3D>(this TGameObject3D gameObject3D, PhysicsSystem physicsSystem, Model model)
        where TGameObject3D : GameObject3D, IPhysicsStatic
    {
        List<Triangle> triangles = model.ExtractTrianglesFromModel();

        physicsSystem.BufferPool.Take<Triangle>(triangles.Count, out var triangleBuffer);

        for (int i = 0; i < triangles.Count; i++)
            triangleBuffer[i] = triangles[i];

        Mesh mesh = new(triangleBuffer, Vector3.One, physicsSystem.BufferPool);

        gameObject3D.StaticDescription = new StaticDescription
        {
            Pose = new RigidPose
            (
                gameObject3D.Position.ToNumerics(),
                QuaternionExtensions.CreateFromRotationVector3(gameObject3D.Rotation).ToNumerics()
            ),
            Shape = physicsSystem.Simulation.Shapes.Add(mesh)
        };
    }

    public static void InitializeBody<TGameObject3D>(this TGameObject3D gameObject3D, PhysicsSystem physicsSystem, Model model)
        where TGameObject3D : GameObject3D, IPhysicsBody
    {
        List<Triangle> triangles = model.ExtractTrianglesFromModel();
        physicsSystem.BufferPool.Take<Triangle>(triangles.Count, out var triangleBuffer);

        for (int i = 0; i < triangles.Count; i++)
            triangleBuffer[i] = triangles[i];

        var mesh = new Mesh(triangleBuffer, Vector3.One, physicsSystem.BufferPool);
        var inertia = mesh.ComputeOpenInertia(gameObject3D.Mass, out var center);
        var adjustedPosition = gameObject3D.Position.ToNumerics() + Vector3.Transform(center, gameObject3D.Orientation);

        gameObject3D.BodyDescription = new BodyDescription
        {
            Pose = new RigidPose(adjustedPosition, gameObject3D.Orientation),
            Collidable = new CollidableDescription
            {
                Shape = physicsSystem.Simulation.Shapes.Add(mesh),
                MinimumSpeculativeMargin = 0.1f
            },
            Velocity = new BodyVelocity(),
            Activity = new BodyActivityDescription(0.01f),
            LocalInertia = inertia
        };
    }

    private static List<Triangle> ExtractTrianglesFromModel(this Model model)
    {
        var triangles = new List<Triangle>();

        foreach (var mesh in model.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                var vertexBuffer = meshPart.VertexBuffer;
                var indexBuffer = meshPart.IndexBuffer;

                // 获取顶点声明以正确读取顶点数据
                var vertexDeclaration = vertexBuffer.VertexDeclaration;
                var vertexStride = vertexDeclaration.VertexStride;

                // 读取原始顶点数据
                var vertexDataSize = vertexBuffer.VertexCount * vertexStride;
                var vertexData = new byte[vertexDataSize];
                vertexBuffer.GetData(vertexData);

                // 找到位置元素的偏移量
                int positionOffset = 0;
                foreach (var element in vertexDeclaration.GetVertexElements())
                {
                    if (element.VertexElementUsage == VertexElementUsage.Position)
                    {
                        positionOffset = element.Offset;
                        break;
                    }
                }

                // 提取顶点位置
                var vertices = new Vector3[vertexBuffer.VertexCount];
                for (int i = 0; i < vertexBuffer.VertexCount; i++)
                {
                    int offset = i * vertexStride + positionOffset;
                    vertices[i] = new Vector3(
                        BitConverter.ToSingle(vertexData, offset),
                        BitConverter.ToSingle(vertexData, offset + 4),
                        BitConverter.ToSingle(vertexData, offset + 8)
                    );
                }

                // 提取索引并创建三角形
                int startIndex = meshPart.StartIndex;
                int primitiveCount = meshPart.PrimitiveCount;

                if (indexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
                {
                    var indices = new ushort[indexBuffer.IndexCount];
                    indexBuffer.GetData(indices);

                    for (int i = 0; i < primitiveCount; i++)
                    {
                        int idx = startIndex + i * 3;
                        int i0 = indices[idx] + meshPart.VertexOffset;
                        int i1 = indices[idx + 1] + meshPart.VertexOffset;
                        int i2 = indices[idx + 2] + meshPart.VertexOffset;

                        triangles.Add(new Triangle(
                            vertices[i0],
                            vertices[i1],
                            vertices[i2]));
                    }
                }
                else
                {
                    var indices = new uint[indexBuffer.IndexCount];
                    indexBuffer.GetData(indices);

                    for (int i = 0; i < primitiveCount; i++)
                    {
                        int idx = startIndex + i * 3;
                        long i0 = indices[idx] + meshPart.VertexOffset;
                        long i1 = indices[idx + 1] + meshPart.VertexOffset;
                        long i2 = indices[idx + 2] + meshPart.VertexOffset;

                        triangles.Add(new Triangle(
                            vertices[i0],
                            vertices[i1],
                            vertices[i2]));
                    }
                }
            }
        }

        return triangles;
    }
}
