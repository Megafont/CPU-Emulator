using System;
using System.Collections.Generic;
using System.Text;

using CPU_Emulator.Core.Components;

namespace CPU_Emulator.Core.Interfaces.Components
{
	public interface IClock
	{
		event EventHandler<ClockTickEventArgs> OnTick;


		ulong CurrentTick { get; }
		bool IsRunning { get; }

		void Reset();
		void Start();
		void Stop();
	}
}
