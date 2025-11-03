namespace SimpleInstructionMachine.VirtualProcessors;

public static class KnownMemory
{
    public const byte APP_DATA_BOTTOM = 0x00;  // User space grows upwards
    public const byte STACK_BOTTOM = 0xF7;     // Stack grows downwards
    public const byte LCD_0 = 0xF8;
    public const byte LCD_1 = 0xF9;
    public const byte LCD_2 = 0xFA;
    public const byte LCD_3 = 0xFB;
    public const byte LCD_4 = 0xFC;
    public const byte ControlFlags = 0xFD;
    public const byte IVT_RIGHT_ARROW = 0xFE;    
    public const byte IVT_LEFT_ARROW = 0xFF;
}