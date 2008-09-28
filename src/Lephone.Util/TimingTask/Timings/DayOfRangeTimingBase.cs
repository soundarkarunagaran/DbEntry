﻿using System;

namespace Lephone.Util.TimingTask.Timings
{
	public abstract class DayOfRangeTimingBase : ITiming
	{
		protected TimeSpan TimeOfDay;
		protected int DayOfRange;
		protected MiscProvider miscTimeProvider;
		protected DateTime LastCheckTime = DateTime.Now;

		public DayOfRangeTimingBase(TimeOfDayStructure TimeOfDay, int DayOfRange)
			: this(TimeOfDay, DayOfRange, MiscProvider.Instance) {}

		public DayOfRangeTimingBase(TimeOfDayStructure TimeOfDay, int DayOfRange, MiscProvider miscTimeProvider)
		{
			this.miscTimeProvider = miscTimeProvider;
			this.TimeOfDay = TimeOfDay.TimeSpanFromMidNight;
			this.DayOfRange = DayOfRange;
		}

		public bool TimesUp()
		{
			bool bRet = false;
			try
			{
				long ts = TimeSpanFromNowOn().Ticks;
				// using recode last check stat
				bRet = ( ts <= 0 && LastCheckTime < miscTimeProvider.Now.Date.Add(TimeOfDay) );
			}
			catch ( ArgumentException ) {}

			LastCheckTime = miscTimeProvider.Now;

			return bRet;
		}

		// return today TimeSpan only.
		public TimeSpan TimeSpanFromNowOn()
		{
			DateTime itsDate = miscTimeProvider.Now.Date;
			if ( IsDayOfRange() )
			{
				DateTime dt = itsDate.Add(TimeOfDay);
				TimeSpan ts = dt.Subtract(miscTimeProvider.Now);
				return ts;
			}
			// return new TimeSpan(1, 0, 0, 0);
			throw new ArgumentException("Not today");
		}

		public void Reset() {}

		protected abstract bool IsDayOfRange();

	}
}
