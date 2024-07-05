using Excel;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExcelTool
{
    /// <summary>
    /// excel文件存放的路径
    /// </summary>
    public static string EXCEL_PATH = Consts.Paths.RootPath + "/Excels";
    /// <summary>
    /// 数据容器脚本存储位置路径
    /// </summary>
    public static string DATA_CONTAINER_PATH = Consts.Paths.ScriptsPath + "/GameData/GameConfig/ExcelContainer";

    /// <summary>
    /// 表格中数据信息开始的行号
    /// </summary>
    public static int BEGIN_INDEX = 4;

    /// <summary>
    /// 表格支持的合法变量类型
    /// </summary>
    public static HashSet<string> VALID_TYPES = new HashSet<string> { "int", "float", "bool", "string" };


    [MenuItem("草木句萌/导入Excel配置",false, 112)]
    private static void ImportExcelData()
    {
        //得到Excel目录下的所有文件
        DirectoryInfo dInfo = Directory.CreateDirectory(EXCEL_PATH);
        FileInfo[] files = dInfo.GetFiles();
        //声明一个xlsx中所有表格的集合
        DataTableCollection tableConllection;
        for (int i = 0; i < files.Length; i++)
        {
            //文件的扩展名不是.xlsx或.xls就不处理了,且不考虑“~”开头的表格的缓存文件
            if (files[i].Extension != ".xlsx" &&
                files[i].Extension != ".xls")
                continue;

            //打开一个Excel文件得到其中的所有表的数据
            using (FileStream fs = files[i].Open(FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
                tableConllection = excelReader.AsDataSet().Tables;
                fs.Close();
            }

            //遍历文件中的所有表的信息，生成对应容器类和二进制数据
            foreach (DataTable table in tableConllection)
            {
                //一、重新整理表格，删除不合理的行和列
                var typeRow = GetVariableTypeRow(table);
                int keyIdx = GetKeyIndex(table);
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    //1、如果值的类型是不支持的玩意儿直接删掉这一列且打印提示信息（避免某列为空或乱填的值时进行无提示性报错）
                    if (!VALID_TYPES.Contains(typeRow[j].ToString()))
                    {
                        if (typeRow[j] != DBNull.Value)//如果是空白直接删，不是空白报个错再删
                            CmgmLog.FrameLogWarning($"在Excel表中出现了不支持的变量类型：|{typeRow[j]}|，序列化时已删除该列");
                        table.Columns.RemoveAt(j);
                        j--; //因为移除了当前列，所以要再检查一下当前列
                    }

                    //2、如果某一格子的主键是空的，直接移除这一行且打印提示信息（避免某行为空时进行无提示性报错）
                    if (j == keyIdx)
                    {   //只遍历主键这一列的行
                        for (int r = 0; r < table.Rows.Count; r++)
                        {
                            if (table.Rows[r][j] == DBNull.Value)
                            {
                                table.Rows.RemoveAt(r);
                                r--;//因为移除了当前行，所以要再检查一下当前行
                            }
                        }
                    }
                }
                table.AcceptChanges();

                //二、生成容器类
                GenerateExcelContainer(table);
                //三、生成2进制数据
                GenerateExcelBinary(table);
            }
        }
    }
    [MenuItem("草木句萌/清空并重导Excel配置",false, 112)]
    private static void ClearExcelData()
    {
        if (Directory.Exists(GameConfigManager.BINARY_CONFIG_PATH))
        {
            var files = Directory.GetFiles(GameConfigManager.BINARY_CONFIG_PATH);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        if (Directory.Exists(DATA_CONTAINER_PATH))
        {
            var files = Directory.GetFiles(DATA_CONTAINER_PATH);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        //重新导入
        ImportExcelData();
        //刷新Project窗口
        AssetDatabase.Refresh();
    }


    #region 生成C#容器数据
    /// <summary>
    /// 获取变量名所在行
    /// </summary>
    private static DataRow GetVariableNameRow(DataTable table)
    {
        return table.Rows[0];
    }
    /// <summary>
    /// 获取变量类型所在行
    /// </summary>
    private static DataRow GetVariableTypeRow(DataTable table)
    {
        //暂定为第二行，下标为1
        return table.Rows[1];
    }
    /// <summary>
    /// 获取主键索引
    /// </summary>
    private static int GetKeyIndex(DataTable table)
    {
        //在第三行内容中找key
        DataRow row = table.Rows[2];
        for (int i = 0; i < table.Columns.Count; i++)
        {
            if (row[i].ToString() == "key")
                return i;
        }
        return 0;
    }


    /// <summary>
    /// 生成Excel表对应的数据容器类
    /// </summary>
    private static void GenerateExcelContainer(DataTable table)
    {
        //从表格前三行得到 字段名、字段类型、主键索引
        DataRow rowName = GetVariableNameRow(table);
        DataRow rowType = GetVariableTypeRow(table);
        int keyIndex = GetKeyIndex(table);

        //没有路径创建路径
        if (!Directory.Exists(DATA_CONTAINER_PATH))
            Directory.CreateDirectory(DATA_CONTAINER_PATH);

        //写入代码，先写入一行数据的容器类，再用一个以主键为key的字典去存它
        string str =
            "using System.Collections.Generic;\n" +

            $"public class {table.TableName}\n" +
            "{\n";

        for (int i = 0; i < table.Columns.Count; i++)
        {
            str += $"    public {rowType[i]} {rowName[i]};\n";
        }
        str += "}" +

            "\n" +
            $"public class {table.TableName}Container\n" +
            "{\n" +
            $"    public Dictionary<{rowType[keyIndex]},{table.TableName}> dataDic = new Dictionary<{rowType[keyIndex]},{table.TableName}>();\n" +
            "}";


        File.WriteAllText(DATA_CONTAINER_PATH +"/"+ table.TableName + "Container.cs", str);

        //刷新Project窗口
        AssetDatabase.Refresh();
    }
    #endregion

    #region 生成编码数据
    /// <summary>
    /// 生成excel2进制数据
    /// </summary>
    /// <param name="table"></param>
    private static void GenerateExcelBinary(DataTable table)
    {
        //没有路径创建路径
        if (!Directory.Exists(GameConfigManager.BINARY_CONFIG_PATH))
            Directory.CreateDirectory(GameConfigManager.BINARY_CONFIG_PATH);

        //创建一个2进制文件进行写入
        using (MemoryStream ms = new MemoryStream())
        {
            //编码头部chuck：{ int 数据行数(表格行数-4) | int 主键变量名长度 | string 主键变量名内容 }
            //写入行数
            ms.Write(BitConverter.GetBytes(table.Rows.Count - 4), 0, 4);
            //写入主键变量名的字符串（字符串都要先写长度再写内容）
            string keyName = GetVariableNameRow(table)[GetKeyIndex(table)].ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(keyName);
            ms.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
            ms.Write(bytes, 0, bytes.Length);

            //编码主体chuck
            DataRow row;
            DataRow rowType = GetVariableTypeRow(table);
            string EmptyPosInfo = "";
            string tableDebugData = "";
            for (int i = BEGIN_INDEX; i < table.Rows.Count; i++)
            {
                //得到一行的数据
                row = table.Rows[i];
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    tableDebugData += $"|{row[j]}|";
                    try
                    {
                        if (string.IsNullOrEmpty(row[j].ToString()))
                        {
                            EmptyPosInfo += $"（{i},{j}）";//自动处理空格子
                            switch (rowType[j].ToString())
                            {
                                case "int":
                                    ms.Write(BitConverter.GetBytes(default(int)), 0, 4);
                                    break;
                                case "float":
                                    ms.Write(BitConverter.GetBytes(default(float)), 0, 4);
                                    break;
                                case "bool":
                                    ms.Write(BitConverter.GetBytes(default(bool)), 0, 1);
                                    break;
                                case "string":
                                    bytes = new byte[0];
                                    //写入字符串字节数组的长度
                                    ms.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
                                    //写入字符串字节数组
                                    ms.Write(bytes, 0, bytes.Length);
                                    break;
                            }
                        }
                        else
                            switch (rowType[j].ToString())
                            {
                                case "int":
                                    ms.Write(BitConverter.GetBytes(int.Parse(row[j].ToString())), 0, 4);
                                    break;
                                case "float":
                                    ms.Write(BitConverter.GetBytes(float.Parse(row[j].ToString())), 0, 4);
                                    break;
                                case "bool":
                                    ms.Write(BitConverter.GetBytes(bool.Parse(row[j].ToString())), 0, 1);
                                    break;
                                case "string":
                                    bytes = Encoding.UTF8.GetBytes(row[j].ToString());
                                    //写入字符串字节数组的长度
                                    ms.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
                                    //写入字符串字节数组
                                    ms.Write(bytes, 0, bytes.Length);
                                    break;
                            }
                    }
                    catch(Exception ex)
                    {
                        CmgmLog.FrameLogError($"序列化表{table.TableName}在（{i},{j}）处的值时出现错误，" +
                            $"请检查此处的值变量类型是否正确，是否按规则填写。\n" + ex.Message);
                    }
                    
                }
                tableDebugData += "\n";
            }
            CmgmLog.FrameLogPositive($"导入了表格：\n{tableDebugData}");


            if (!string.IsNullOrEmpty(EmptyPosInfo))
                CmgmLog.FrameLog_Negative($"数据表{table.TableName}中：{EmptyPosInfo}处的值为空，写入了对应类型变量的默认值");

            //加密并写入数据
            byte[] data = ms.GetBuffer();
            CipherTool.Encryption(ref data);
            File.WriteAllBytes(GameConfigManager.BINARY_CONFIG_PATH + table.TableName + GameConfigManager.DATAFILE_EXTENSION, data);

            ms.Close();
        }

        AssetDatabase.Refresh();
    }
    #endregion
}
    

