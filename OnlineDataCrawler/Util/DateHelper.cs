using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OnlineDataCrawler.Util
{
    public class DateHelper
    {
        private const string vacationDaysFile = "vacations.json";
        private const string DateFormat = "yyyyMMdd";
        private static List<DateTime> vacationDays = new List<DateTime>();
        public static bool IsWorkingDay(DateTime date)
        {
            if (vacationDays.Count == 0)
            {
                if (File.Exists(vacationDaysFile))
                {
                    var jo = JObject.Parse(File.ReadAllText(vacationDaysFile));
                    var vacation = jo["vacations"];
                    if (vacation.Type == JTokenType.Array)
                    {
                        var array = (JArray)vacation;
                        foreach (var va in array)
                        {
                            vacationDays.Add(DateTime.ParseExact(va.ToString(), DateFormat, null, System.Globalization.DateTimeStyles.AssumeLocal));
                        }
                    }
                }
            }
            if (vacationDays.Contains(date))
                return false;
            if (date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday)
                return false;
            return true;
        }

        public static DateTime LastWorkingDay(DateTime date)
        {
            date = date.Subtract(date.TimeOfDay);
            while (true)
            {
                if (IsWorkingDay(date))
                {
                    return date;
                }
                else
                {
                    date = date.AddDays(-1);
                }
            }
        }

        public static DateTime ToLocalTime(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Local)
                return dateTime;
            else
            {
                if (dateTime.Kind == DateTimeKind.Utc)
                {
                    TimeSpan timeSpan = TimeZoneInfo.Local.GetUtcOffset(dateTime);
                    return TimeZoneInfo.ConvertTime(dateTime.Add(timeSpan), TimeZoneInfo.Local);
                }
                else
                {
                    return dateTime;
                }
            }
        }

        public static DateTime LastSeasonLastDay(DateTime dateTime)
        {
            int year = dateTime.Year;
            int month = 0;
            int day = 0;
            switch (dateTime.Month)
            {
                case 1:
                case 2:
                case 3:
                    month = 12;
                    break;
                case 4:
                case 5:
                case 6:
                    month = 3;
                    break;
                case 7:
                case 8:
                case 9:
                    month = 6;
                    break;
                case 10:
                case 11:
                case 12:
                    month = 9;
                    break;

            }
            switch (month)
            {
                case 3:
                    day = 31;
                    break;
                case 6:
                    day = 30;
                    break;
                case 9:
                    day = 30;
                    break;
                case 12:
                    year--;
                    day = 31;
                    break;
            }
            return new DateTime(year, month, day);
        }

        public static int GetSeason(DateTime date)
        {
            switch (date.Month)
            {
                case 1:
                case 2:
                case 3:
                    return 1;
                case 4:
                case 5:
                case 6:
                    return 2;
                case 7:
                case 8:
                case 9:

                    return 3;
                case 10:
                case 11:
                case 12:
                    return 4;
            }
            return 0;
        }
    }
}
