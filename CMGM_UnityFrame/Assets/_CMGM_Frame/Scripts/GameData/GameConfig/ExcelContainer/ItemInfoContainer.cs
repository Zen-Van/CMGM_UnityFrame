using System.Collections.Generic;
public class ItemInfo
{
    public string id;
    public string name;
    public string type;
    public string imagePath;
    public string descript;
    public bool canBattleUse;
    public bool canMapUse;
    public string luaContent;
    public int priceSell;
    public int priceBuy;
}
public class ItemInfoContainer
{
    public Dictionary<string,ItemInfo> dataDic = new Dictionary<string,ItemInfo>();
}