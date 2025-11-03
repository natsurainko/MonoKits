using Microsoft.Xna.Framework.Content;
using Minesweeper.Scenes.Contents;
using MonoKits.Content;
using MonoKits.Content.Attributes.Scenes;
using MonoKits.Extensions;
using MonoKits.Spatial3D.Objects;
using MonoKits.Spatial3D.Objects.Physics;
using MonoKits.Spatial3D.Physics;

namespace Minesweeper.Scenes;

internal partial class MainScene(MainSceneContent mainSceneContent, ContentManager contentManager, PhysicsSystem pSystem) 
    : Scene3D<MainSceneContent>(mainSceneContent, contentManager, pSystem)
{
    [SceneObject]
    [PhysicsObject]
    [StaticModel(nameof(MainSceneContent.Starvalley))]
    public StaticModelObject3D? Ground { get; set; }

    [SceneObject]
    [PhysicsObject]
    [SceneObjectPosition(0, 3.25f, 10)]
    [StaticModel(nameof(MainSceneContent.House))]
    public StaticModelObject3D? House { get; set; }

    [SceneObject]
    [PhysicsObject]
    [SceneObjectPosition(0f, 0.5f, 0f)]
    [StaticModel(nameof(MainSceneContent.Board))]
    public StaticModelObject3D? Board { get; set; }

    [SceneObject]
    [SceneObjectPosition(10, 10, 10)]
    public SpriteObject3D? Sprite { get; set; }

    [SceneObject]
    [SceneObjectPosition(0f, 10f, 0f)]
    public SpriteObject3D? Bait { get; set; }

    [SceneObject]
    [PhysicsObject]
    [SceneObjectPosition(0, 4, 10)]
    public BodyModelObject3D? Player { get; set; }

    [SceneObject]
    [PhysicsObject]
    public BodyModelObject3D? Plane { get; set; }

    protected override void Loading()
    {
        Sprite = new SpriteObject3D(ScopedContent.Title, _graphicsDevice, new(4.8f, 1.2f));
        Bait = new SpriteObject3D(ScopedContent.Bait, _graphicsDevice, new(4f, 1f));
        Player = new BodyModelObject3D(ScopedContent.Block);
        Plane = new BodyModelObject3D(ScopedContent.QuickPlane);

        Sprite.Billboard = SpriteObject3D.BillboardMode.CameraBillboard;
        Bait.Billboard = SpriteObject3D.BillboardMode.CylindricalBillboard;
    }

    protected override void LoadPhysicsObjects(PhysicsSystem? physicsSystem)
    {
        if (physicsSystem == null) return;

        Ground?.InitializeStatic(physicsSystem, Ground.Model);
        Board?.InitializeStatic(physicsSystem, Board.Model);
        House?.InitializeStatic(physicsSystem, House.Model);

        Player?.InitializeBody(physicsSystem, Player.Model);
        Plane?.InitializeBody(physicsSystem, Plane.Model);
    }

    protected override void OnLoaded()
    {
        Ground!.EnableDefaultLighting();
        Board!.EnableDefaultLighting();
        House!.EnableDefaultLighting();
        Player!.EnableDefaultLighting();
        Plane!.EnableDefaultLighting();
    }
}