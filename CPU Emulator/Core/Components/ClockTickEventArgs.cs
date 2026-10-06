namespace CPU_Emulator.Core.Components;

public class ClockTickEventArgs : EventArgs
{
	public ulong CurrentTick { get; private set; }


	public ClockTickEventArgs(ulong currentTick)
	{
		CurrentTick = currentTick;
	}
}