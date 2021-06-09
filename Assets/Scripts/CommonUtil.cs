using System;
using System.Collections.Generic;
using TalkPlus;

public class CommonUtil
{
    public static readonly string KEY_USER_ID = "KeyUserId";
    public static readonly string KEY_USER_NAME = "KeyUserName";

    public static string GetAttendees(TPChannel tpChannel)
    {
        List<TPUser> users = tpChannel.GetMembers();
        if (users != null && users.Count > 0)
        {
            List<string> names = new List<string>();
            users.ForEach((TPUser user) => { names.Add(user.GetUsername()); });
            string attendees = string.Join(", ", names);

            return attendees;
        }

        return null;
    }

    public static string GetFormattedTime(long milliseconds)
    {
        if (milliseconds > 0)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddMilliseconds(milliseconds).ToLocalTime();
            return string.Format("{0:yyyy/MM/dd HH:mm}", date);
        }
        else
        {
            return null;
        }
    }
}
