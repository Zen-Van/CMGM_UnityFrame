using System.Collections.Generic;
public class RoleInfo
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
public class RoleInfoContainer
{
    public Dictionary<int,RoleInfo> dataDic = new Dictionary<int,RoleInfo>();
}