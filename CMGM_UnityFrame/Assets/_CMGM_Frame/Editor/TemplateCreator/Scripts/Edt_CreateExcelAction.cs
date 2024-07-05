using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;


namespace MYDQC
{
    public class Edt_CreateExcelAction : EndNameEditAction
    {
        private const string TemplatesPath = Consts.Paths.RootPath + "/Editor/TemplateCreator/Templates";
        private const string Path_Excel = TemplatesPath + "/新建 XLSX 工作表.xlsx";


        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            File.Copy(Path_Excel, pathName, false);

            //刷新本地资源
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 返回在Project窗口下选中的路径
        /// </summary>
        /// <returns></returns>
        private static string GetSelectedPath()
        {
            //默认路径为Assets
            string selectedPath = "Assets";

            //获取选中的资源
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

            //遍历选中的资源以返回路径
            foreach (Object obj in selection)
            {
                selectedPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(selectedPath) && File.Exists(selectedPath))
                {
                    selectedPath = Path.GetDirectoryName(selectedPath);
                    break;
                }
            }

            return selectedPath;
        }


        //--------------------------------------------------------------
        [MenuItem("Assets/CMGM Create/XLSX工作表", false, 0)]
        [MenuItem("草木句萌/Create/XLSX工作表", false, 0)]
        static void CreateNewExcelScripts()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                CreateInstance<Edt_CreateExcelAction>(),
                GetSelectedPath() + "/NewXLSX.xlsx", null,
                Path_Excel);
        }
    }
}
