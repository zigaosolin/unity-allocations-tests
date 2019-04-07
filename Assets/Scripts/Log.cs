using UnityEngine;

public static class Log
{
    public static bool EnableLogging = false;

    public static void Info(string message)
    {
        if (!EnableLogging)
        {
            return;
        }

        Debug.Log(message);
    }

    public static void Info(string format, int argument)
    {
        if(!EnableLogging)
        {
            return;
        }

        Debug.Log(string.Format(format, argument));
    }
}
