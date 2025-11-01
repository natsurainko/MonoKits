namespace MonoKits.Spatial3D;
public enum Object3DRestriction
{
    None = 1,

    DisForward = 1 << 1,
    DisRight = 1 << 2,
    DisUp = 1 << 3,
    DisYaw = 1 << 4,
    DisPitch = 1 << 5,
    DisRoll = 1 << 6,

    LimitYaw = 1 << 7,
    LimitPitch = 1 << 8,
    LimitRoll = 1 << 9,

    Other = 1 << 10
}