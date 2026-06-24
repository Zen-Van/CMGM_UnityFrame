using System.Collections.Generic;

namespace CMGM.Workspace
{
/// <summary>
/// RoleInfo表中一行数据的存储类
/// </summary>
public class RoleInfoRow
{
    public int id;
    public string name;
    public string styleName;
    public string descript;
    public int initLevel;
    public int Jing;
    public int Qi;
    public int Shen;
}
public class RoleInfo
{
    public Dictionary<int,RoleInfoRow> dataDic = new Dictionary<int,RoleInfoRow>();
}
}