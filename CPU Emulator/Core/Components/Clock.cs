using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using CPU_Emulator.Core.Interfaces.Components;
using CPU_Emulator.Core.Components;

using Timer = System.Timers.Timer;



namespace CPU_Emulator.Core.Components
{
	public class Clock : IClock
	{
		public event EventHandler<ClockTickEventArgs> OnTick;

		private float _TickDuration;

		public ulong CurrentTick { get; private set; }
		public bool IsRunning { get => _Timer.Enabled; }


		private Timer _Timer;


		// Remember that tickDuration is in ms.
		public Clock(float tickDuration = 0.00001f) 
		{
			if (tickDuration <= 0)
				throw new ArgumentOutOfRangeException($"{nameof(tickDuration)} must be greater than to 0.0!");


			_TickDuration = tickDuration;

			_Timer = new Timer(tickDuration);
			_Timer.Elapsed += async (sender, e) => await OnTimerElapsed();
			_Timer.AutoReset = true;
			//_Timer.Start();
		}

		~Clock()
		{
			_Timer.Stop();
			_Timer.Dispose();
		}

		public void Reset()
		{
			CurrentTick = 0;
		}

		public void Start()
		{
			_Timer.Enabled = true;
		}

		public void Stop()
		{
			_Timer.Enabled = false;
		}

		internal Task OnTimerElapsed()
		{
			CurrentTick++;

			OnTick?.Invoke(this, new ClockTickEventArgs(CurrentTick));

			return Task.CompletedTask;
		}
	}
}
