using System;
using System.Threading;

public static class TimerAgent
{

    private static MySqlDataAgent sqlAgent = new MySqlDataAgent();
    private static Timer _timer;
    private static readonly TimeSpan _frequencyTimeSpan = new TimeSpan(hours: 1, minutes: 0, seconds: 0);

    public static void Initialize()
    {
        _timer = BuildTimer();
    }

    private static Timer BuildTimer()
    {
        var result = new Timer(
            callback: FireTimer,
            state: null,
            dueTime: new TimeSpan(hours: 0, minutes: 0, seconds: 2),
            period: _frequencyTimeSpan);

        return result;
    }

    /// <summary>
    /// Fires every minute.  
    /// </summary>
    /// <param name="state">likely leave as default null</param>
    private static void FireTimer(object state = null)
    {
        try
        {
            Logger.Log("TimerAgent.FireTimer", "Start");
            var currentMstTime = Util.ConvertToTZ(DateTime.Now, "MST");
            var start = new TimeSpan(0, 0, 0); //midnight
            var end = new TimeSpan(2, 0, 0); //2 am
                                             //if after midnight before 2 AM
            if ((currentMstTime.TimeOfDay > start) && (currentMstTime.TimeOfDay < end))
            {
                Logger.Log("TimerAgent.FireTimer", "It is between 0 and 2");

                //loop thru all the timeclock that don't have a "ClockedOutAt" time,
                var timeclocklist = sqlAgent.GetAllMissingClockOut();
                foreach (var timeclock in timeclocklist)
                {
                    Logger.Log("TimerAgent.FireTimer", $"Signing out {timeclock.FirstName}, ID: {timeclock.TimeclockId}");
                    //sign them out at midnight that day.
                    var clockouttime = timeclock.ClockedInAt;
                    TimeSpan ts = new TimeSpan(23, 59, 59);
                    timeclock.ClockedOutAt = clockouttime.Date + ts;
                    sqlAgent.UpdateTimeclockAction(timeclock);
                }
                Logger.Log("TimerAgent.FireTimer", "End of IFF");
            }

            Logger.Log("TimerAgent.FireTimer", "End");
        }
        catch (Exception err)
        {
            Logger.Log("TimerAgent.FireTimer", $"Message: {err.Message}, StackTrace: {err.StackTrace}");
        }
    }

}

