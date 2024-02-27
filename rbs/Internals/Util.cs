using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

public static class Util
{
    public static int RETRYHOURS = 3;
    public static readonly string filefolderpath = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? "c:\\code\\plwebfiles\\" : "/home/sthomason/plwebfiles/";

    public static DateTime ConvertDateToUTC(DateTime sourceDate)
    {
        var windows = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        TimeZoneInfo mstZone;
        try
        {
            if (windows)
            {
                mstZone = TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time");
            }
            else
            {
                mstZone = TimeZoneInfo.FindSystemTimeZoneById("America/Phoenix");
            }
            DateTime timeUtc = TimeZoneInfo.ConvertTimeToUtc(sourceDate, mstZone);
            return timeUtc;
        }
        catch (Exception err)
        {
            Logger.Log("ConvertDateToUTC PLWEB.ERROR TZ", err.Message + err.StackTrace);
            return DateTime.MinValue;
        }
    }

    public static DateTime ConvertToTZ(DateTime sourceDate, string tz)
    {
        var windows = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        DateTime timeUtc = TimeZoneInfo.ConvertTimeToUtc(sourceDate);
        DateTime tzTime = DateTime.MinValue;

        //new TZ --> https://en.wikipedia.org/wiki/List_of_tz_database_time_zones
        try
        {
            if (string.IsNullOrEmpty(tz))
            {
                Logger.Log("ConvertToTZ Empty TZ!!", $"sourceDate:{sourceDate}, TZ:{tz}");
                throw new Exception();
            }
            TimeZoneInfo tzone = null;
            if (tz == "MST" && windows)
            {
                tzone = TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time");
            }
            else if (tz == "MST" && !windows)
            {
                tzone = TimeZoneInfo.FindSystemTimeZoneById("America/Phoenix");
            }
            else if (tz == "CST" && windows)
            {
                tzone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            }
            else if (tz == "CST" && !windows)
            {
                tzone = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");
            }
            tzTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, tzone);
        }
        catch (Exception err)
        {
            Logger.Log("ConvertToTZ PLWEB.ERROR TZ", err.Message + err.StackTrace);
        }

        return tzTime;
    }

    public static string GetResourceFileAsString(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = filename;
        string result = string.Empty;

        using (Stream stream = assembly.GetManifestResourceStream("plweb.Resources." + resourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            result = reader.ReadToEnd();
        }

        return result;
    }

    public static Stream GetResourceFileAsStream(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = filename;

        return assembly.GetManifestResourceStream("plweb.Resources." + resourceName);
    }

    public static string GetContentType(string path)
    {
        var types = GetMimeTypes();
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return types[ext];
    }

    private static Dictionary<string, string> GetMimeTypes()
    {
        return new Dictionary<string, string>
        {
            {".pdf", "application/pdf"},
            {".jpg", "application/image"},
            {".png", "application/image"},
        };
    }

    //doesn't work ... leaving code to fight another day ...
    /*
    public static string GetShortenedUrl(string longurl)
    {
        var shorturl = string.Empty;
        try
        {
            var url = $"http://api.bit.ly/shorten?format=json&version=2.0.1&longUrl={HttpUtility.UrlEncode(longurl)}&login=sthomason&apiKey=a88f9568c970ebec1c5ef5de150be783c8f852d4";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                var result = reader.ReadToEnd();
            }
        }
        catch (Exception e)
        {
            Logger.Log("GetShortenedUrl", "failed for" + longurl + e.Message + e.StackTrace);
        }

        return shorturl;
    }
    */
}

