namespace MonoKits.Spatial3D;
public enum Object3DRestriction
{
    DisForward = 1,
    DisRight = 1 << 1,
    DisUp = 1 << 2,
    DisYaw = 1 << 3,
    DisPitch = 1 << 4,
    DisRoll = 1 << 5,

    LimitYaw = 1 << 6,
    LimitPitch = 1 << 7,
    LimitRoll = 1 << 8,

    Other = 1 << 9
}