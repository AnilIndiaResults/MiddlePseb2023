using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
//using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

namespace PsebPrimaryMiddle.Helpers
{
    /// <summary>
    /// Defines global helper utility methods
    /// </summary>
    public class Helper
    {
        #region private variables

        private static DateTime NULL_DATE_VALUE = DateTime.MinValue;
        private static string NULL_STRING_VALUE = string.Empty;

        #endregion

        #region Public Methods

        #region Date time methods

        /// <summary>
        /// Date to Number.
        /// </summary>
        /// <param name="date">Date time</param>
        /// <returns>Long number value for the date time</returns>        
        public static long DatetoNum(DateTime date)
        {
            long i;

            i = Convert.ToInt64(date.Year) * 10000000000000;
            i += Convert.ToInt64(date.Month) * 100000000000;
            i += Convert.ToInt64(date.Day) * 1000000000;
            i += Convert.ToInt64(date.Hour) * 10000000;
            i += Convert.ToInt64(date.Minute) * 100000;
            i += Convert.ToInt64(date.Second) * 1000;
            i += Convert.ToInt64(date.Millisecond) * 1;

            return i;
        }

        /// <summary>
        /// Checks whether or not a date is a valid Date
        /// </summary>
        /// <param name="date"></param>
        /// <returns>True or False</returns>
        public static bool IsDate(object date)
        {
            bool isDate = true;

            try
            {
                System.DateTime.Parse(date.ToString());
            }
            catch
            {

                isDate = false;
            }

            return isDate;
        }

        /// <summary>
        /// Counts the number of weekdays between two dates.
        /// </summary>
        /// <returns></returns>
        public static int CountWeekDays(DateTime dtStart, DateTime dtEnd)
        {
            TimeSpan timeSpan = dtEnd - dtStart;

            int Count = 0;
            for (int ikey = 0; ikey < timeSpan.Days; ikey++)
            {
                DateTime dt = dtStart.AddDays(ikey);
                if (IsWeekDay(dt))
                    Count++;
            }

            return Count;
        }

        /// <summary>
        /// Counts the number of weekends between two dates.
        /// </summary>
        /// <returns></returns>
        public static int CountWeekends(DateTime dtStart, DateTime dtEnd)
        {
            TimeSpan timeSpan = dtEnd - dtStart;

            int Count = 0;
            for (int ikey = 0; ikey < timeSpan.Days; ikey++)
            {
                DateTime dt = dtStart.AddDays(ikey);
                if (IsWeekEnd(dt))
                    Count++;
            }

            return Count;
        }

        /// <summary>
        /// Returns true if day is weekday, otherwise false.
        /// </summary>
        /// <returns></returns>
        public static bool IsWeekDay(DateTime datetime)
        {
            if (datetime.DayOfWeek == DayOfWeek.Saturday || datetime.DayOfWeek == DayOfWeek.Sunday)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Returns true of day is weekend, otherwise false.
        /// </summary>
        /// <returns></returns>
        public static bool IsWeekEnd(DateTime datetime)
        {
            if (datetime.DayOfWeek == DayOfWeek.Sunday || datetime.DayOfWeek == DayOfWeek.Saturday)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Returns a string that enumerates years, months, days, etc.
        /// This method lists the time in words between a start time and an end time.  For example, if right now I put in a start date of May 06, 2005 5:30:00 PM (my oldest son's birthday) and an end date of DateTime.Now, this method would return the following string: 4 months 6 days 18 hours 10 minutes 56 seconds.
        /// </summary>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        public static string TimeBreakDown(DateTime dtStart, DateTime dtEnd)
        {
            int seconds = dtEnd.Second - dtStart.Second;
            int minutes = dtEnd.Minute - dtStart.Minute;
            int hours = dtEnd.Hour - dtStart.Hour;
            int days = dtEnd.Day - dtStart.Day;
            int months = dtEnd.Month - dtStart.Month;
            int years = dtEnd.Year - dtStart.Year;

            if (seconds < 0)
            {
                minutes--;
                seconds += 60;
            }

            if (minutes < 0)
            {
                hours--;
                minutes += 60;
            }

            if (hours < 0)
            {
                days--;
                hours += 24;
            }

            if (days < 0)
            {
                months--;
                int previousMonth = (dtEnd.Month == 1) ? 12 : dtEnd.Month - 1;
                int year = (previousMonth == 12) ? dtEnd.Year - 1 : dtEnd.Year;
                days += DateTime.DaysInMonth(year, previousMonth);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            string sYears = FormatString("year", string.Empty, years);
            string sMonths = FormatString("month", sYears, months);
            string sDays = FormatString("day", sMonths, days);
            string sHours = FormatString("hour", sDays, hours);
            string sMinutes = FormatString("minute", sHours, minutes);
            string sSeconds = FormatString("second", sMinutes, seconds);

            return sYears + sMonths + sDays + sHours + sMinutes + sSeconds;
        }
        
        /// <summary>
        /// Gets a list of all month names, if true is passed the month names are abreviated, culture specific
        /// </summary>
        /// <param name="abreviated"></param>
        /// <param name="Culture"></param>
        /// <returns></returns>
        public static SortedList GetMonths(bool abreviated, CultureInfo Culture)
        {
            SortedList months = new SortedList();
            string[] monthNames = null;

            try
            {
                if (abreviated)
                    monthNames = Culture.DateTimeFormat.AbbreviatedMonthNames;
                else
                    monthNames = Culture.DateTimeFormat.MonthNames;
            }
            catch
            {
                monthNames = new CultureInfo("en-us").DateTimeFormat.MonthNames;
            }

            for (int iKey = 0; iKey < monthNames.Length - 1; iKey++)
            {
                months.Add(iKey + 1, monthNames[iKey]);
            }

            return months;

        }

        /// <summary>
        /// Gets a list of all month names, if true is passed the month names are abreviated, en-us culture specific
        /// </summary>
        /// <param name="abreviated"></param>
        public static SortedList GetMonths(bool abreviated)
        {
            // create the culture object for en-us
            return GetMonths(abreviated, new CultureInfo("en-us"));
        }

        /// <summary>
        /// Gets a list of all days in a month
        /// </summary>
        /// <returns></returns>
        public static Queue GetDays()
        {
            Queue days = new Queue();

            for (short iKey = 1; iKey < 32; iKey++)
            {
                days.Enqueue(iKey);
            }

            return days;
        }

        public static int IndianFinancialYearQuarterPeriod(int MonthNumber)
        {
            if (MonthNumber > 0 &&
                MonthNumber <= 3)
                return 4;

            if (MonthNumber > 3 &&
                MonthNumber <= 6)
                return 1;

            if (MonthNumber > 6 &&
                MonthNumber <= 9)
                return 2;

            if (MonthNumber > 9 &&
                MonthNumber <= 12)
                return 3;

            return 0;
        }

        /// <summary>
        /// Validates the indian financial year date range
        /// </summary>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public static bool ValidateIndianFinancialYearDateRange(DateTime StartDate, DateTime EndDate)
        {
            //1. Endate should be greater then StartDate
            if (StartDate > EndDate)
                return false;

            // now we need to check whether a single financial year is selected or not
            // implies that the end date should never be greater and start date year + 1 of 31st march
            if (EndDate > new DateTime(StartDate.Year + 1, 3, 31))
                return false;

            if (StartDate < new DateTime(EndDate.Year - 1, 4, 1))
                return false;

            return true;
        }

        /// <summary>
        /// Gets a list of all years from today to n years behind
        /// </summary>
        /// <returns></returns>
        public static ArrayList GetYears(int yearsBehind)
        {
            return GetYears(0, yearsBehind);
            //			Queue years = new Queue();
            //			
            //			for (int iKey = DateTime.Today.Year; iKey >= (DateTime.Today.Year - yearsBehind); iKey--) 
            //			{
            //				years.Enqueue(iKey);
            //			}
            //
            //			return years;
        }

        /// <summary>
        /// Gets a list all yeas from today + years ahead to today - years behind
        /// </summary>
        /// <param name="yearsAhead"></param>
        /// <param name="yearsBehind"></param>
        /// <returns></returns>
        public static ArrayList GetYears(int yearsAhead, int yearsBehind)
        {
            ArrayList years = new ArrayList();

            for (int iKey = (DateTime.Now.Year + yearsAhead); iKey >= (DateTime.Today.Year - yearsBehind); iKey--)
            {
                years.Add(iKey);
            }

            return years;
        }

        /// <summary>
        /// Gets an array of int values from the start value to the end value.
        /// The list of numbers can be incremented by any amount (like step).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static int[] GetInts(int start, int end, int increment)
        {
            int ilength = ((end - start) / increment) + 1;

            int[] values = new int[ilength];
            for (int iKey = 0; iKey < ilength; iKey++)
            {
                values[iKey] = start + (iKey * increment);
            }

            return values;
        }

        /// <summary>
        /// Gets an array of int values from the start value to the end value.
        /// The list of numbers can be incremented by any amount (like step).
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static ArrayList GetDecimals(decimal start, decimal end, decimal increment)
        {
            ArrayList values = new ArrayList();
            for (decimal ikey = start; ikey <= end; ikey = ikey + increment)
            {
                values.Add(ikey + "0");
            }

            return values;
        }

        /// <summary>
        /// Returns the date in outlook format style
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static string GetOutlookDateTimeFormat(DateTime datetime)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(datetime.ToString("yyyyMMdd")).Append("T").Append(datetime.ToString("HHmmss")).Append("A");
            return sb.ToString();
        }

        /// <summary>
        /// Converts GMT Time to IST Time
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ConvertToISTFromGMT(DateTime value)
        {
            if (IsDate(value))
            {
                if (!value.Equals(DateTime.MinValue))
                {
                    return value.AddMinutes(330);
                }
            }

            return value;
        }

        /// <summary>
        /// Converts the passed datetime to a GMT time
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ConvertDateTimeToGTM(DateTime value)
        {
            // get the current time and GTM time
            DateTime GMT = DateTime.UtcNow;
            DateTime Current = DateTime.Now;

            // get the time span difference between the two 
            TimeSpan timeSpan = GMT - Current;

            // return gtm time
            return value.AddMinutes(timeSpan.TotalMinutes);
        }

        /// <summary>
        /// Returns the GTM value of the passed date time
        /// </summary>
        public static DateTime ConvertCurrentDateTimeToGTM()
        {
            return ConvertDateTimeToGTM(DateTime.Now);
        }

        #endregion

        #region String Methods

        /// <summary>
        /// Cuts the provided string at allowedLength - 3 (ellipsis length)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="allowedLength">Max. length of the string to return</param>
        /// <returns>string that ends with ellipsis (...)</returns>
        public static string ShortenByEllipsis(string text, int allowedLength)
        {
            if (EmptyTrimOrNull(text))
                return String.Empty;

            if (text.Length > allowedLength + 3)
            {
                int nlPos = text.IndexOfAny(new char[] { '\n', '\r' });

                if (nlPos >= 0 && nlPos < allowedLength)
                    return text.Substring(0, nlPos) + "...";
                else
                    return text.Substring(0, allowedLength) + "...";
            }
            else
                return text;
        }

        /// <summary>
        /// Cuts the provided string at allowedLength - 3 (ellipsis length)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="allowedLength">Max. length of the string to return</param>
        /// <returns>string that ends with ellipsis (...)</returns>
        public static string ShortenByEllipsisWithBreaks(string text, int allowedLength)
        {
            if (EmptyTrimOrNull(text))
                return String.Empty;

            if (text.Length > allowedLength + 3)
            {
                return text.Substring(0, allowedLength) + "...";
            }
            else
                return text;
        }

        /// <summary>
        /// removes all html tags from the passed value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string StripHTML(string value)
        {
            // Strips the HTML tags
            string pattern = "<(.|\n)+?>";
            string strOutput = string.Empty;

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            // Replace all HTML tag matches with the empty string
            strOutput = regex.Replace(value, string.Empty);

            // replace all < and > with &lt; and &gt;
            strOutput = strOutput.Replace("<", "&alt;");
            strOutput = strOutput.Replace(">", "&gt;");

            return strOutput;
        }

        /// <summary>
        /// remove leading strings with zeros and adjust for singular/plural
        /// </summary>
        /// <param name="str"></param>
        /// <param name="previousStr"></param>
        /// <param name="ikey"></param>
        /// <returns></returns>
        private static string FormatString(string str, string previousStr, int ikey)
        {
            if ((ikey == 0) && (previousStr.Length == 0)) return string.Empty;
            if (ikey == 1)
            {
                str = ikey.ToString() + " " + str;
            }
            else
            {
                str = ikey.ToString() + " " + str + "s";
            }
            return str + " ";
        }

        /// <summary>
        /// Count how many times a string appears.
        /// Counts the number of times a string appears in an array of strings.
        /// </summary>
        /// <param name="strArray"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int CountString(string[] strArray, string str)
        {
            int iCount = 0;

            for (int iKey = 0; iKey < strArray.Length; iKey++)
            {
                if (strArray[iKey].IndexOf(str) > -1)
                    iCount++;
            }

            return iCount;
        }

        /// <summary>
        /// Count how many times a string appears.
        /// Counts the number of times a string appears in string. It counts the number of times a regular expression pattern occurs in a string.  
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regexStr"></param>
        /// <returns></returns>
        public static int CountString(string str, string regexStr)
        {
            Regex regex = new Regex(regexStr);
            return regex.Matches(str).Count;
        }

        /// <summary>
        /// Gets a string with ASCII characters
        /// </summary>
        /// <returns></returns>
        public static string GetASCII()
        {
            char low = (char)35;
            char high = (char)255;

            StringBuilder sb = new StringBuilder();
            for (char ch = low; ch <= high; ch++)
            {
                sb.Append(ch.ToString() + " ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Remove extra white space in a string.
        /// Removes all white space except a single space.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static string RemoveWhiteSpace(string Value)
        {
            if (Value.Length > 0)
            {
                while (Value.IndexOf("  ") > 0)
                {
                    Value = Value.Replace("  ", " ");
                }
            }

            return Value;
        }

        /// <summary>
        /// Converts the passed string to proper case 
        /// </summary>
        /// <example>deepankar raizada will be converted to Deepankar Raizada</example>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static string ProperCase(string Value)
        {
            StringBuilder sb = new StringBuilder();
            string[] words = Value.Split(new char[] { ' ' });

            foreach (string word in words)
            {
                sb.Append(Char.ToUpper(word[0])); // first letter
                sb.Append(word.Substring(1).ToLower()); // remaining words
                sb.Append(" ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a list of Titles
        /// </summary>
        /// <returns></returns>
        public static string[] GetTitles()
        {
            string[] titles = new string[] {
											   "Dr.",
											   "Mr.",
											   "Mrs.",
											   "Miss",
											   "Ms.",
											   "Other"
										   };

            return titles;
        }

        /// <summary>
        /// Converts an array of strings into a string
        /// </summary>
        /// <param name="stringArray"></param>
        /// <param name="bQuotes"></param>
        /// <param name="deliminator"></param>
        /// <returns></returns>
        public static string StringArrayToString(string[] stringArray, string deliminator, bool bQuotes)
        {
            StringBuilder sb = new StringBuilder();

            for (int iKey = 0; iKey < stringArray.Length; iKey++)
            {
                if (stringArray[iKey].Trim().Length > 0)
                {
                    string delim = iKey > 0 ? deliminator : String.Empty;

                    if (bQuotes)
                        sb.Append(delim + "'" + stringArray[iKey].Trim() + "'");
                    else
                        sb.Append(delim + stringArray[iKey].Trim());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets an hashTable of Yes No, If abreviatedValues is True then key is Y and N else 0 and 1
        /// </summary>
        /// <returns></returns>
        public static Hashtable YesNoCodes(bool abreviatedValues)
        {
            Hashtable hash = new Hashtable();

            if (abreviatedValues)
            {
                hash.Add("Y", "Yes");
                hash.Add("N", "No");
            }
            else
            {
                hash.Add(0, "Yes");
                hash.Add(1, "No");
            }

            return hash;
        }

        /// <summary>
        /// Test for a valid e-Mail address and returns true,
        /// if the content match, else false. 
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool IsEmail(string Value)
        {
            if (Value == null || Value.Length == 0)
                return false;

            Regex regexEMail = new Regex(@"(?<prefix>mailto:)?(?<address>(?:[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+\.)*[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!\.)){0,61}[a-zA-Z0-9]?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\[(?:(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\.){3}(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\]))$)",
                RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

            return regexEMail.IsMatch(Value.Trim());
        }

        /// <summary>
        /// True, if 'text' was null or of length zero. No trimming of the string happens
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool EmptyOrNull(string text)
        {
            return (text == null || text.Length == 0);
        }

        /// <summary>
        /// True, if 'text' was null or of length zero
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool EmptyTrimOrNull(string text)
        {
            return (text == null || text.Trim().Length == 0);
        }

        /// <summary>
        /// Add slashes for special characters
        /// </summary>
        /// <param name="value">string to be parsed</param>
        /// <returns>Parsed string</returns>
        /// <example>Is your name D'raizada? will return Is your name D\'raizada?</example>
        public static string AddSlashes(string value)
        {
            if (value.Length > 0)
            {
                if (value.IndexOf("\"") > 0)
                {
                    value = value.Replace("\"", "'");
                }

                if (value.IndexOf("'") > 0)
                {
                    value = value.Replace("'", "\\'");
                }

                //textToEscape = textToEscape.Replace( "'", "\\'" );
                //textToEscape = textToEscape.Replace( "\"", "\\\"" );
            }

            return value;
        }

        /// <summary>
        /// Add quotes for \"
        /// </summary>
        /// <param name="value">string to be parsed</param>
        /// <returns>Parsed string</returns>
        /// <example>Is your name D"raizada? will return Is your name D&quot;raizada?</example>
        public static string AddQuotes(string value)
        {
            if (value.Length > 0)
            {
                if (value.IndexOf("\"") > 0)
                {
                    value = value.Replace("\"", "&quot;");
                }
            }

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string[] SplitEx(
            string s,
            string separator)
        {
            if (s == null || s.Length == 0)
            {
                return null;
            }
            else
            {
                ArrayList list = new ArrayList();

                int lastPos = 0;
                int pos = 0;
                while ((pos = s.IndexOf(separator, lastPos)) >= 0)
                {
                    list.Add(s.Substring(lastPos, pos - lastPos + 1));

                    lastPos = pos + separator.Length;
                }

                if (lastPos < s.Length - 1)
                {
                    list.Add(s.Substring(lastPos));
                }

                return (string[])list.ToArray(typeof(string));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string[] SplitEx(
            string s,
            params char[] separator)
        {
            string[] t = s.Split(separator);

            // remove empties.
            ArrayList list = new ArrayList();
            foreach (string u in t)
            {
                if (u.Length > 0)
                    list.Add(u);
            }

            return (string[])list.ToArray(typeof(string));
        }

        /// <summary>
        /// Split a string.
        /// </summary>
        /// <param name="value">String that needs to be splitted</param>
        /// <param name="Separator">String needs to be splitted with this string value</param>
        /// <returns></returns>
        public static string[] Split(string value, string Separator)
        {
            if (value == null ||
                value.Length == 0)
            {
                return null;
            }
            else
            {
                ArrayList list = new ArrayList();

                int lastPos = 0;
                int Position = 0;

                while ((Position = value.IndexOf(Separator, lastPos)) >= 0)
                {
                    // add to list
                    list.Add(value.Substring(lastPos, Position - lastPos));

                    lastPos = Position + Separator.Length;
                }

                if (lastPos < value.Length + 1)
                {
                    list.Add(value.Substring(lastPos));
                }

                return (string[])list.ToArray(typeof(string));
            }
        }

        public static String CreateSlug(string value)
        {
            value = value.Replace(" & ", " ").Replace("'s", String.Empty);

            value = Regex.Replace(value, @"[^\w\@-]", "-");

            return value;
        }


        #endregion

        /// <summary>
        /// Return a string of the name value collection in a datagrid control htmlized
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <param name="Server"></param>
        /// <returns></returns>
      

        /// <summary>
        /// Return a string of the cookie collection in a datagrid control htmlized
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
      
        #endregion

        #region Miscellaneous Methods

        /// <summary>
        /// Converts a URL into one that is usable on the requesting client.
        /// </summary>
        /// <remarks>Converts ~ to the requesting appliRewriteUrlcation path.  Mimics the behavior of the 
        /// <b>Control.ResolveUrl()</b> method, which is often used by control developers.</remarks>
        /// <param name="appPath">The application path.</param>
        /// <param name="URl">The URL, which might contain ~.</param>
        /// <returns>A resolved URL.  If the input parameter <b>url</b> contains ~, it is replaced with the
        /// value of the <b>appPath</b> parameter.</returns>
        public static string ResolveURl(string appPath, string URl)
        {
            if (URl.Length == 0 || URl[0] != '~')
                return URl; // there is no ~ in the first character position, just return the url
            else
            {
                if (URl.Length == 1)
                    return appPath; // there is just the ~ in the url, return the root path

                if (URl[1] == '/' || URl[1] == '\\')
                {
                    // url looks like ~/ or ~\
                    if (appPath.Length > 1)
                    {
                        // Check if in the end the / is there or not, if not then add
                        if (appPath.Substring(appPath.Length - 1, 1) != "/")
                            return appPath + "/" + URl.Substring(2);
                        else
                            return appPath + URl.Substring(2);
                    }
                    else
                        return "/" + URl.Substring(2);
                }
                else
                {
                    // url looks like ~something
                    if (appPath.Length > 1)
                        return appPath + "/" + URl.Substring(1);
                    else
                        return "/" + URl.Substring(1);
                }
            }
        }

        /// <summary>
        /// Checks whether CDATA Section is required for the XML string
        /// </summary>
        /// <param name="value">String to check for CDATA</param>
        /// <returns>True or False</returns>
        public static bool IsCDATA(string value)
        {
            if (value.IndexOf("<") > -1)
                return true;

            if (value.IndexOf(">") > -1)
                return true;

            if (value.IndexOf("\"") > -1)
                return true;

            if (value.IndexOf("'") > -1)
                return true;

            if (value.IndexOf("&") > -1)
                return true;

            return false;
        }

        /// <summary>
        /// Generate Current days XML Log File Name
        /// </summary>
        /// <example>20052005- 20th May, 2005</example>
        public static string GenerateCurrentLogFileName()
        {
            StringBuilder sb = new StringBuilder();
            DateTime dt = DateTime.Now.Date;

            if (dt.Day.ToString().Length < 2)
                sb.Append("0" + dt.Day.ToString());
            else
                sb.Append(dt.Day.ToString());

            if (dt.Month.ToString().Length < 2)
                sb.Append("0" + dt.Month.ToString());
            else
                sb.Append(dt.Month.ToString());

            sb.Append(dt.Year.ToString() + ".xml");

            return sb.ToString();
        }


        /// <summary>
        /// Converts a webcontrol to string
        /// </summary>
        /// <returns></returns>
        public static string WebControlToString(Control objWebControl)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            HtmlTextWriter textWriter = new HtmlTextWriter(sw);
            objWebControl.RenderControl(textWriter);

            return sb.ToString();
        }

        #endregion

        #region Conversion Methods

        public static string ListToString(List<int> listItems)
        {
            string values = string.Empty;

            foreach (int value in listItems)
            {
                // concatenate 
                values += string.Format("{0},", value);
            }

            // remove trailing comma, split and return 
            if (values.Length > 0)
                return values.TrimEnd(new char[] { ',' });

            return string.Empty;
        }

        public static string ListToString(List<Guid> listItems)
        {
            string values = string.Empty;

            foreach (var value in listItems)
            {
                // concatenate 
                values += string.Format("{0},", value.ToString());
            }

            // remove trailing comma, split and return 
            if (values.Length > 0)
                return values.TrimEnd(new char[] { ',' });

            return string.Empty;
        }

        public static bool IsEmptyString(string str)
        {
            if (str == null || str == String.Empty)
                return true;
            return false;
        }

        public static bool IsEmptyString(object obj)
        {
            if (obj == null || obj == DBNull.Value)
                return true;
            if (obj.ToString() == String.Empty)
                return true;
            return false;
        }

        public static string GetString(object value)
        {
            if (value == DBNull.Value ||
                value == null)
                return NULL_STRING_VALUE;

            if (value is string)
            {
                if (EmptyTrimOrNull(value.ToString()))
                    return NULL_STRING_VALUE;
            }

            return value.ToString();
        }

        /// <summary>
        /// Convert value to byte field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte GetByte(object value)
        {
            if (value == DBNull.Value)
            {
                return 0;
            }
            return Convert.ToByte(value);
        }

        /// <summary>
        /// Convert value to boolean field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetBoolean(object value)
        {
            if (value == DBNull.Value)
            {
                return false;
            }
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Converts a bool value to a byte field value, 1 means true and 0 means false
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte GetBooleanToByte(bool value)
        {
            if (value)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Converts a bool value to a byte field value, 1 means true and 0 means false
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetByteToBoolean(object value)
        {
            if (GetByte(value) == 1)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Convert value to date time field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime GetDate(object value)
        {
            if (value == DBNull.Value)
            {
                return NULL_DATE_VALUE;
            }
            return Convert.ToDateTime(value);
        }

        /// <summary>
        /// Convert value to db safe string for quotes field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDBSafeString(string value)
        {
            return value.Replace("'", "''");
        }

        /// <summary>
        /// Convert value to decimel field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal GetDecimal(object value)
        {
            if (value == DBNull.Value)
            {
                return new decimal(-1, -1, -1, true, 0);
            }

            return Convert.ToDecimal(value);
        }

        public static decimal GetDecimalOrZero(object value)
        {
            if (value == DBNull.Value ||
                value == null ||
                EmptyTrimOrNull(value.ToString()))
            {
                return 0;
            }

            return GetDecimal(value);
        }

        /// <summary>
        /// Convert value to decimel field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double GetDouble(object value)
        {
            if (value == DBNull.Value)
            {
                return new double();
            }

            return Convert.ToDouble(value);
        }

        /// <summary>
        /// Convert value to integer field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetInt(object value)
        {
            if (value == DBNull.Value)
            {
                return -1;
            }
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Convert value to long field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long GetLong(object value)
        {
            if (value == DBNull.Value)
            {
                return (long)(-1);
            }
            return Convert.ToInt64(value);
        }

        /// <summary>
        /// Convert value to short field
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static short GetShort(object value)
        {
            if (value == DBNull.Value)
            {
                return -1;
            }
            return Convert.ToInt16(value);
        }

        /// <summary>
        /// Check the passed value and returns null, if no value exists
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object CheckEmpty(object value)
        {
            if (value == DBNull.Value || value == null)
                return null;

            if (value is string)
            {
                if (EmptyOrNull(value.ToString()))
                    return null;
            }

            return value;
        }

        #endregion

        #region Private Methods

        private static DataTable CookieCollectionToDataTable(HttpContext Context)
        {
            DataTable dataTable = new DataTable();

            // Add columns
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Value", typeof(string));

            // Iterate through the name value collection and add rows to the data table
            foreach (string Value in Context.Request.Cookies)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["Name"] = Value;
                dataRow["Value"] = Context.Request.Cookies[Value].Value;

                // Add this to the data table
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <summary>
        /// Converts an Name Value collection to a DataTable for binding with a Data Grid
        /// </summary>
        /// <param name="inputCollection"></param>
        /// <param name="Server"></param>
        /// <returns></returns>
        private static DataTable NameValueCollectionToDataTable(NameValueCollection inputCollection, HttpServerUtility Server)
        {
            DataTable dataTable = new DataTable();

            // Add columns
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Value", typeof(string));

            // Iterate through the name value collection and add rows to the data table
            foreach (string Value in inputCollection)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["Name"] = Value;
                dataRow["Value"] = Server.HtmlEncode(inputCollection[Value]);

                // Add this to the data table
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

       

        #endregion

        #region Stream Methods

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="initialLength">The initial buffer length</param>
        public static byte[] ReadStream(Stream stream, long initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }

        #endregion

        public static Control FindControlRecursively(string controlID, ControlCollection controls)
        {
            if (controlID == null || controls == null)
                return null;

            foreach (Control c in controls)
            {
                if (c.ID == controlID)
                    return c;

                if (c.HasControls())
                {
                    Control inner = FindControlRecursively(controlID, c.Controls);
                    if (inner != null)
                        return inner;
                }
            }
            return null;
        }

        public static void MakeGridViewHeaderClickable(GridView gridView, GridViewRow gridViewRow,
            string sortExpression, string sortDirection)
        {
            if (gridViewRow.RowType == DataControlRowType.Header)
            {
                for (int iKey = 0; iKey < gridView.Columns.Count; iKey++)
                {
                    string columnSortExpression = gridView.Columns[iKey].SortExpression;
                    TableCell tableCell = gridViewRow.Cells[iKey];

                    //Make sure the column we are working with has a sort expression
                    if (!string.IsNullOrEmpty(columnSortExpression))
                    {
                        #region Set Sort Image

                        System.Web.UI.WebControls.Image sortDirectionImage;

                        //Create an instance of a Image WebControl
                        sortDirectionImage = new System.Web.UI.WebControls.Image();

                        //Determine the image url based on the SortDirection
                        string imageUrl = "~/Images/sort_neutral.gif";
                        string altText = string.Empty;

                        if (columnSortExpression.ToLower().Equals(sortExpression.ToLower()))
                        {
                            if (sortDirection.ToLower().Equals("asce"))
                            {
                                imageUrl = "~/Images/sort_asc.gif";
                                altText = "Ascending";
                            }
                            else
                            {
                                imageUrl = "~/Images/sort_desc.gif";
                                altText = "Descending";
                            }
                        }

                        //Add the Image Web Control to the cell
                        sortDirectionImage.ImageUrl = imageUrl;
                        sortDirectionImage.AlternateText = string.Format("Current Sort - {0} {1}",
                            gridView.Columns[iKey].HeaderText, altText);

                        sortDirectionImage.Style.Add(HtmlTextWriterStyle.MarginLeft, "5px");
                        tableCell.Wrap = false;

                        tableCell.Controls.Add(sortDirectionImage);

                        #endregion

                        //Enumerate the controls within the current cell and find the link button.
                        #region Enumerate

                        foreach (System.Web.UI.Control gridViewRowCellControl in
                            gridViewRow.Cells[iKey].Controls)
                        {
                            LinkButton linkButton = gridViewRowCellControl as LinkButton;

                            if ((linkButton != null) &&
                                linkButton.CommandName == "Sort")
                            {
                                //Add an onclick attribute to the current cell
                                tableCell.Attributes.Add("onclick", "RequestData('" + linkButton.ClientID + "', this, event)");

                                tableCell.Style.Add(HtmlTextWriterStyle.Cursor, "hand");
                                tableCell.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                                break;
                            }
                        }

                        #endregion
                    }
                }
            }

            //Enuerate all the rows and change the color of the column that is being sorted
            //if (gridViewRow.RowType == DataControlRowType.Header)
            //{
            //    for (int iKey = 0; iKey < gridViewRow.Cells.Count; iKey++)
            //    {
            //        if ((!String.IsNullOrEmpty(sortExpression)) &&
            //            gridView.Columns[iKey].SortExpression.ToLower().Equals(sortExpression.ToLower()))
            //        {

            //        }
            //    }
            //}
        }

        #region Create a Table

        /// <summary>
        /// Create a table from a comma-delimited list of field names and data types. 
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="FieldList"></param>
        /// <returns></returns>
        public static DataTable CreateTable(string TableName, string FieldList)
        {
            DataTable dataTable = new DataTable(TableName);
            DataColumn dataColumn;

            string[] Fields = FieldList.Split(new char[] { ',' });
            string[] FieldsParts;

            string Expression;

            foreach (string Field in Fields)
            {
                // get field parts
                FieldsParts = Field.Trim().Split(" ".ToCharArray(), 3); // allow for spaces in the expression

                // add fieldname and datatype
                if (FieldsParts.Length == 2)
                {
                    dataColumn = dataTable.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true));
                    dataColumn.AllowDBNull = true;
                }
                else if (FieldsParts.Length == 3)
                {
                    // for reqd. 
                    Expression = FieldsParts[3].Trim();
                    if (Expression.ToUpper() == "REQUIRED")
                    {
                        dataColumn = dataTable.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true));
                        dataColumn.AllowDBNull = false;
                    }
                    else
                    {
                        dataColumn = dataTable.Columns.Add(FieldsParts[0].Trim(), Type.GetType("System." + FieldsParts[1].Trim(), true, true), Expression);
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid field definition: " + Field + ".");
                }
            }

            return dataTable;
        }

        #endregion

        public static string ParseCamelToProper(string sIn)
        {
            // we need to first replace the _ with - 
            sIn = sIn.Replace("_", " - ");

            char[] letters = sIn.ToCharArray();
            string sOut = string.Empty;

            foreach (char c in letters)
            {
                if (c.ToString() != c.ToString().ToLower())
                {
                    // it's uppercase, add a space
                    sOut += " " + c.ToString();
                }
                else
                {
                    sOut += c.ToString();
                }
            }

            return sOut;
        }

        public static object XmlToObject(Type ucType, string Xml)
        {
            object obj = null;

            // hydrated base on private string var
            if (Xml.Length > 0)
            {
                XmlSerializer xmlSer = new XmlSerializer(ucType);
                StringBuilder sb = new StringBuilder();
                sb.Append(Xml);

                StringReader sReader = new StringReader(Xml);
                obj = xmlSer.Deserialize(sReader);
                sb = null;
                sReader.Close();
            }

            return obj;
        }

        public static string DataSetToString(DataSet dataSet, string title, bool printHeaders)
        {
            StringBuilder sb = new StringBuilder();

            // start the excel file headers
            sb.Append("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\" ");
            sb.Append("xmlns=\"https://www.w3.org/TR/REC-html40\"><head><meta http-equiv=Content-Type content=\"text/html; charset=windows-1252\">");
            sb.Append("<meta name=ProgId content=Excel.Sheet><meta name=Generator content=\"Microsoft Excel 9\"><!--[if gte mso 9]>");
            sb.Append("<xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>" + title + "</x:Name><x:WorksheetOptions>");
            sb.Append("<x:Selected/><x:ProtectContents>False</x:ProtectContents><x:ProtectObjects>False</x:ProtectObjects>");
            sb.Append("<x:ProtectScenarios>False</x:ProtectScenarios></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets>");
            sb.Append("<x:ProtectStructure>False</x:ProtectStructure><x:ProtectWindows>False</x:ProtectWindows></x:ExcelWorkbook></xml>");
            sb.Append("<![endif]--></head><body><table>");

            // Start the excel worksheet
            sb.Append("</table><table><tr><td colspan=\"4\"><h3><b>" + title + "</b></h3></td></tr></table>");
            sb.Append("</table><table><tr><td></td></tr></table>");

            for (int table = 0; table < dataSet.Tables.Count; table++)
            {
                sb.Append("<table>");

                if (printHeaders)
                {
                    sb.Append(string.Format("<TR><td colspan = \"{0}\"><B>{1}</b></td></tr>",
                        dataSet.Tables[table].Columns.Count, dataSet.Tables[table].TableName.ToUpper()));
                    sb.Append("<TR BgColor = \"#CCCCCC\">");

                    for (int iCol = 0; iCol < dataSet.Tables[table].Columns.Count; iCol++)
                    {
                        sb.Append("<td><b>" + dataSet.Tables[table].Columns[iCol].ColumnName + "</b></td>");
                    }
                    sb.Append("</TR>");
                }

                for (int iRow = 0; iRow < dataSet.Tables[table].Rows.Count; iRow++)
                {
                    sb.Append("<TR>");
                    for (int iCol = 0; iCol < dataSet.Tables[table].Columns.Count; iCol++)
                    {
                        sb.Append("<td>" + dataSet.Tables[table].Rows[iRow][iCol] + "</b></td>");
                    }
                    sb.Append("<TR>");
                }

                sb.Append("</table><table><tr><td></td></tr></table>");
            }

            return sb.ToString();
        }

        public static string DataTableToString(DataTable dataTable, bool headers)
        {
            return DataTableToString(dataTable, headers, null);
        }

        /// <summary>
        /// Converts a DataTable to a string with an html table.
        /// </summary>
        /// <returns></returns>
        public static string DataTableToString(DataTable datatable, bool headers, ArrayList ColumnsNtRequired)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<table border=\"1\">");

            if (headers)
            {
                //write column headings
                sb.Append("<tr>");
                foreach (DataColumn datacolumn in datatable.Columns)
                {
                    bool IsAddColumn = false;
                    if (ColumnsNtRequired != null)
                    {
                        // now check whether the column name is the same one of the column names in the array
                        IsAddColumn = !ColumnsNtRequired.Contains(datacolumn.ColumnName);
                    }
                    else
                        IsAddColumn = true;

                    if (IsAddColumn)
                        sb.Append("<td>" + datacolumn.ColumnName + "</td>");
                }
                sb.Append("</tr>");
            }

            //write table data
            foreach (DataRow datarow in datatable.Rows)
            {
                sb.Append("<tr>");
                foreach (DataColumn datacolumn in datatable.Columns)
                {
                    bool IsAddColumn = false;
                    if (ColumnsNtRequired != null)
                    {
                        // now check whether the column name is the same one of the column names in the array
                        IsAddColumn = !ColumnsNtRequired.Contains(datacolumn.ColumnName);
                    }
                    else
                        IsAddColumn = true;

                    if (IsAddColumn)
                        sb.Append("<td>" + datarow[datacolumn].ToString() + "</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            return sb.ToString();
        }

        /// <summary>
        /// A simple function to detect if a user vesting your site is on a mobile device or not.
        /// </summary>
        /// <returns></returns>
        public static bool IsMobileBrowser()
        {
            //get the current context
            HttpContext Context = HttpContext.Current;

            // First try built in asp.net check
            if (Context.Request.Browser.IsMobileDevice)
            {
                return true;
            }

            // Then try checking for the http_x_wap_profile header
            if (Context.Request.ServerVariables["HTTP_X_WAP_PROFILE"] != null)
            {
                return true;
            }

            // Then try checking that HTTP_ACCEPT EXISTS AND CONTAINS WAP
            if (Context.Request.ServerVariables["HTTP_ACCEPT"] != null &&
                Context.Request.ServerVariables["HTTP_ACCEPT"].ToLower().Contains("wap"))
            {
                return true;
            }

            // and finally check the the HTTP_USER_AGENT 
            // header variable for any one of the following
            if (Context.Request.ServerVariables["HTTP_USER_AGENT"] != null)
            {
                // create a list of all mobile types
                string[] mobiles = new string[] {
                    "midp", "j2me", "avant", "docomo", 
                    "novarra", "palmos", "palmsource", 
                    "240x320", "opwv", "chtml",
                    "pda", "windows ce", "mmp/", 
                    "blackberry", "mib/", "symbian", 
                    "wireless", "nokia", "hand", "mobi",
                    "phone", "cdm", "up.b", "audio", 
                    "SIE-", "SEC-", "samsung", "HTC", 
                    "mot-", "mitsu", "sagem", "sony"
                    , "alcatel", "lg", "eric", "vx", 
                    "NEC", "philips", "mmm", "xx", 
                    "panasonic", "sharp", "wap", "sch",
                    "rover", "pocket", "benq", "java", 
                    "pt", "pg", "vox", "amoi", 
                    "bird", "compal", "kg", "voda",
                    "sany", "kdd", "dbt", "sendo", 
                    "sgh", "gradi", "jb", "dddi", 
                    "moto", "iphone"
                };

                //Loop through each item in the list created above 
                //and check if the header contains that text
                foreach (string mobile in mobiles)
                {
                    if (Context.Request.ServerVariables["HTTP_USER_AGENT"].
                                                        ToLower().Contains(mobile.ToLower()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Converts the basic string to a secure string
        /// </summary>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        public static SecureString StringToSecureString(string inputValue)
        {
            SecureString secureString = new SecureString();
            char[] chars = inputValue.ToCharArray(0, inputValue.Length);

            foreach (char c in chars)
                secureString.AppendChar(c);

            return secureString;
        }

        /// <summary>
        /// Calls the passed executable file path with the arguments
        /// </summary>
        /// <param name="processExecutable"></param>
        /// <param name="args"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public static bool CallExternalProcess(string processExecutable, string args, int timeOut)
        {
            Process process = new Process();
            try
            {
                //Create a new process info structure.
                ProcessStartInfo processInfo = new ProcessStartInfo();

                // set the credentials the application will use to run
                //processInfo.Domain = Environment.MachineName;
                //processInfo.UserName = @"SrvcAcntReportWriter";
                //processInfo.Password = StringToSecureString("serViceAcntRptWrtr");

                //set execuates and arguments etc
                processInfo.FileName = processExecutable;
                processInfo.Arguments = args;
                //processInfo.UseShellExecute = false;

                //Start the process.
                process = Process.Start(processInfo);

                //Wait for window to finish loading.
                process.WaitForInputIdle();

                //Set a time-out value
                //Wait for the process to exit or time out.
                process.WaitForExit(timeOut);

                //Check to see if the process is still running.
                if (process.HasExited == false)
                {
                    //Process is still running.
                    //Test to see if the process is hung up.
                    if (process.Responding)
                    {
                        //Process was responding; close the main window.
                        process.CloseMainWindow();
                    }
                    else
                    {
                        //Process was not responding; force the process to close.
                        process.Kill();
                    }

                    return false; // failure in running the executable
                }
            }
            catch (Win32Exception winEx)
            {
                throw winEx;
            }
            catch (Exception ex)
            {
                throw new Exception("Helper.CallExternalProcess." + processExecutable + " threw an error: " + ex.ToString());
            }
            finally
            {
                if (process != null)
                    process.Close();
            }

            return true;
        }
    }

    #region Generate Password

    /// <summary>
    /// Creates a random password. 
    /// </summary>
    //public class PasswordGenerator
    //{
    //    // Define default min and max password lengths.
    //    private int DEFAULT_MIN_PASSWORD_LENGTH = 8;
    //    private int DEFAULT_MAX_PASSWORD_LENGTH = 10;

    //    // Define supported password characters divided into groups.
    //    private string PASSWORD_CHARS_LCASE = "abcdefgijkmnopqrstwxyz";
    //    private string PASSWORD_CHARS_UCASE = "ABCDEFGHJKLMNPQRSTWXYZ";
    //    private string PASSWORD_CHARS_NUMERIC = "1234567890";
    //    //private string PASSWORD_CHARS_SPECIAL = "@";

    //    /// <summary>
    //    /// Create a new instance of the object to generate a new random password
    //    /// </summary>
    //    public PasswordGenerator()
    //    {
    //        // No constructor required
    //    }

    //    /// <summary>
    //    /// To generate a password with the predefined lengths(8,10)
    //    /// </summary>
    //    /// <returns>string</returns>
    //    public string Generate()
    //    {
    //        return this.Generate(this.DEFAULT_MIN_PASSWORD_LENGTH, this.DEFAULT_MAX_PASSWORD_LENGTH);
    //    }

    //    /// <summary>
    //    /// To generate a password with a the minimum length
    //    /// <param name="length">Number of characters to be returned</param>
    //    /// </summary>
    //    /// <returns>string</returns>
    //    public string Generate(int length)
    //    {
    //        return this.Generate(length, length);
    //    }

    //    /// <summary>
    //    /// To generate a password
    //    /// <param name="maxLength">Maximum number of characters to be returned</param>
    //    /// <param name="minLength">Minimum number of characters</param>
    //    /// </summary>
    //    /// <returns>string</returns>
    //    public string Generate(int minLength, int maxLength)
    //    {
    //        // Make sure the input parameters are correct
    //        if (minLength <= 0 || maxLength <= 0 || minLength > maxLength)
    //            return null;

    //        // Create a local array containing supported password characters
    //        // grouped by types.
    //        Char[][] charGroups = new Char[][] {
    //                                               this.PASSWORD_CHARS_LCASE.ToCharArray(), 
    //                                               this.PASSWORD_CHARS_UCASE.ToCharArray(),
    //                                               this.PASSWORD_CHARS_NUMERIC.ToCharArray()};


    //        // Use this array to track the number of unused characters in each
    //        // character group.
    //        int[] charsLeftInGroup = new int[] { (charGroups.Length - 1) };
    //        int iKey;
    //        for (iKey = 0; (iKey <= (charsLeftInGroup.Length - 1)); iKey++)
    //        {
    //            charsLeftInGroup[iKey] = charGroups[iKey].Length;
    //        }

    //        int[] leftGroupsOrder = new int[] { (charGroups.Length - 1) };

    //        // Initially, all character groups are not used.
    //        for (iKey = 0; (iKey <= (leftGroupsOrder.Length - 1)); iKey++)
    //        {
    //            leftGroupsOrder[iKey] = iKey;
    //        }

    //        // Because we cannot use the default randomizer, which is based on the
    //        // current time (it will produce the same "random" number within a
    //        // second), we will use a random number generator to seed the
    //        // randomizer.

    //        // Use a 4-byte array to fill it with random bytes and convert it then
    //        // to an integer value.
    //        byte[] randomBytes = new byte[4];

    //        // Generate 4 random bytes.
    //        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
    //        rng.GetBytes(randomBytes);

    //        // Convert 4 bytes into a 32-bit integer value.
    //        int seed = (randomBytes[0] & 0x7f) << 24 |
    //            randomBytes[1] << 16 |
    //            randomBytes[2] << 8 |
    //            randomBytes[3];


    //        // Now, this is real randomization.
    //        Random random = new Random(seed);

    //        // This array will hold the pasword characters
    //        char[] password = null;

    //        // Allocate appropriate memory for the password            
    //        if (minLength < maxLength)
    //        {
    //            password = new char[random.Next(minLength - 1, maxLength)];
    //        }
    //        else
    //        {
    //            password = new char[minLength - 1];
    //        }

    //        // Index of the next character to be added to password.
    //        int nextCharIdx;

    //        // Index of the next character group to be processed.
    //        int nextGroupIdx;

    //        // Index which will be used to track not processed character groups.
    //        int nextLeftGroupsOrderIdx;

    //        // Index of the last non-processed character in a group.
    //        int lastCharIdx;

    //        // Index of the last non-processed group. Initially, we will skip
    //        // special characters.
    //        int lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;

    //        // Generate password characters one at a time.
    //        for (int i = 0; i < password.Length; i++)
    //        {
    //            // If only one character group remained unprocessed, process it;
    //            // otherwise, pick a random character group from the unprocessed
    //            // group list.
    //            if (lastLeftGroupsOrderIdx == 0)
    //                nextLeftGroupsOrderIdx = 0;
    //            else
    //                nextLeftGroupsOrderIdx = random.Next(0,
    //                    lastLeftGroupsOrderIdx);

    //            // Get the actual index of the character group, from which we will
    //            // pick the next character.
    //            nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];

    //            // Get the index of the last unprocessed characters in this group.
    //            lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;

    //            // If only one unprocessed character is left, pick it; otherwise,
    //            // get a random character from the unused character list.
    //            if (lastCharIdx == 0)
    //                nextCharIdx = 0;
    //            else
    //                nextCharIdx = random.Next(0, lastCharIdx + 1);

    //            // Add this character to the password.
    //            password[i] = charGroups[nextGroupIdx][nextCharIdx];

    //            // If we processed the last character in this group, start over.
    //            if (lastCharIdx == 0)
    //                charsLeftInGroup[nextGroupIdx] = charGroups[nextGroupIdx].Length;
    //            // There are more unprocessed characters left.
    //            else
    //            {
    //                // Swap processed character with the last unprocessed character
    //                // so that we don't pick it until we process all characters in
    //                // this group.
    //                if (lastCharIdx != nextCharIdx)
    //                {
    //                    char temp = charGroups[nextGroupIdx][lastCharIdx];
    //                    charGroups[nextGroupIdx][lastCharIdx] =
    //                        charGroups[nextGroupIdx][nextCharIdx];
    //                    charGroups[nextGroupIdx][nextCharIdx] = temp;
    //                }
    //                // Decrement the number of unprocessed characters in
    //                // this group.
    //                charsLeftInGroup[nextGroupIdx]--;
    //            }

    //            // If we processed the last group, start all over.
    //            if (lastLeftGroupsOrderIdx == 0)
    //                lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
    //            // There are more unprocessed groups left.
    //            else
    //            {
    //                // Swap processed group with the last unprocessed group
    //                // so that we don't pick it until we process all groups.
    //                if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
    //                {
    //                    int temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
    //                    leftGroupsOrder[lastLeftGroupsOrderIdx] =
    //                        leftGroupsOrder[nextLeftGroupsOrderIdx];
    //                    leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
    //                }
    //                // Decrement the number of unprocessed groups.
    //                lastLeftGroupsOrderIdx--;
    //            }
    //        }

    //        // Convert password characters into a string and return the result.
    //        return new string(password);
    //    }
    //}

    #endregion
}