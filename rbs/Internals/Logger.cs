using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;

public static class Logger
{
    public static void Log(string nameOfErrorEvent, string messageCanBeJson)
    {      
            var client = new TelemetryClient();
            client.InstrumentationKey = "5a2c55f3-381f-4f38-9479-f6b02da3ce99";
            client.TrackTrace(nameOfErrorEvent, new Dictionary<string, string>() {{nameOfErrorEvent, messageCanBeJson}});
    }
}