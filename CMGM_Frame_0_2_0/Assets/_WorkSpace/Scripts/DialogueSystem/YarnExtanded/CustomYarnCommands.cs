using UnityEngine;
using Yarn.Unity;

public class CustomYarnCommands
{
    [YarnCommand("Log")]
    public static void Log(string message)
    {
        CmgmLog.yarnNormal(message);
    }


}
