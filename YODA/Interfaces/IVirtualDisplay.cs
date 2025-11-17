namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualDisplay<in T>
	where T : struct
{
	/// <summary>
	/// Holds the memory address which controls refreshing of the virtual display
	/// </summary>
	int ControlFlagAddress { get; }
	
	/// <summary>
	/// Performs a refresh of the screen if the <see cref="ControlFlagAddress"/> value toggles and has bit 0 set
	/// </summary>
	/// <param name="controlFlags">The value for the flag</param>
	void Refresh(T controlFlags);
}