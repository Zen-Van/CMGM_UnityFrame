using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using VFolders.Libs;

public class DefaultRhythmMapGenerator : EditorWindow
{
    private AudioClip selectedClip;
    private float bpm = 120;
    private int offsetMs = 0;
    private int beatsPerMeasure = 4;
    private int totalMeasures = 100;

    [MenuItem("草木句萌/音频系统/默认节拍数据生成器")]
    public static void ShowWindow()
    {
        GetWindow<DefaultRhythmMapGenerator>("节奏映射生成器");
    }

    private void OnGUI()
    {
        GUILayout.Label("音乐节奏映射生成器", EditorStyles.boldLabel);

        // 音乐文件选择
        selectedClip = (AudioClip)EditorGUILayout.ObjectField("音乐文件", selectedClip, typeof(AudioClip), false);

        // 参数输入
        bpm = EditorGUILayout.FloatField("BPM (每分钟节拍数)", bpm);
        offsetMs = EditorGUILayout.IntField("偏移量 (毫秒)", offsetMs);
        beatsPerMeasure = EditorGUILayout.IntField("每小节拍数", beatsPerMeasure);
        totalMeasures = EditorGUILayout.IntField("总小节数", totalMeasures);

        EditorGUILayout.Space();

        // 生成按钮
        if (GUILayout.Button("生成节奏映射数据"))
        {
            if (selectedClip == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择音乐文件", "确定");
                return;
            }

            GenerateBeatMap();
        }
    }

    private void GenerateBeatMap()
    {
        int totalBeats = totalMeasures * beatsPerMeasure;
        List<int> beatTimes = new List<int>(totalBeats);

        // 计算每拍的时间间隔（毫秒）
        int beatInterval = (60000 / bpm).RoundToInt();  // 60,000ms / BPM

        // 生成所有拍点时间
        for (int i = 0; i < totalBeats; i++)
        {
            int beatTime = offsetMs + i * beatInterval;
            beatTimes.Add(beatTime);
        }

        // 创建JSON数据
        BeatEvtListData data = new BeatEvtListData();
        data.beatTimesMs = beatTimes.ToArray();

        string json = JsonUtility.ToJson(data, true);

        if(!Directory.Exists(Consts.Paths.RhythmMap_Path))
        {
            Directory.CreateDirectory(Consts.Paths.RhythmMap_Path);
        }
        if(!Directory.Exists(Consts.Paths.RhythmMap_Path + $"/{selectedClip.name}"))
        {
            Directory.CreateDirectory(Consts.Paths.RhythmMap_Path + $"/{selectedClip.name}");
        }

        AssetDatabase.Refresh();

        // 保存文件
        string path = EditorUtility.SaveFilePanel(
            "保存节奏映射数据",
            Consts.Paths.RhythmMap_Path + $"/{selectedClip.name}",
            $"{selectedClip.name}_BeatEvtList.json",
            "json");

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log($"节奏映射已保存至: {path}");
        }
    }
}
