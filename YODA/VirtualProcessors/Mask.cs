namespace SimpleInstructionMachine.VirtualProcessors;

/// <summary>
/// Bitmap mask to apply to opcodes.  For example all 8 opcodes
/// for Add begin with ob0100
/// </summary>
public static class Mask
{
    public const byte Misc = 0b0000;
    public const byte SaveToFile = 0b0001;
    public const byte LoadFromFile = 0b0010;
    public const byte Write = 0b0011;
    public const byte Add = 0b0100;
    public const byte Sub = 0b0101;
    public const byte Inc = 0b0110;
    public const byte Dec = 0b0111;
    public const byte JumpIfZero = 0b1000;
    public const byte JumpWithReturn = 0b1001;
}



