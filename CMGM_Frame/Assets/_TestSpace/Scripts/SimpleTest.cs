using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CMGM.Core;

public class SimpleTest : MonoBehaviour
{
    public TMP_Text txtState;
    public Slider sldPercent;

    private async void Awake()
    {
        CmgmLog.fNormal("SimpleTest Awake");

        _ = ResourcesResMgr.Instance;

        GameObject cube = await AddressablesResMgr.Instance.LoadAssetAsync<GameObject>("LevelPrefabs/Cube.prefab");
        Instantiate(cube, Vector3.zero, Quaternion.identity);


        MusicSyncTool.PlayBgmWithBeatSync("Play_AlienLoveSong", "AlienLoveSong");
    }

    private void Start()
    {
        
    }

    private int bgmPosition;

    private void Update()
    {
        //得到的是在Source上的位置（即每次循环清空&暂停不计时），单位是毫秒
        
        CmgmLog.fNormal($"音乐播放的进度为：{MusicSyncTool.CurBgmPosition}\n" +
            $"音乐节拍的状态为：{MusicSyncTool.curBeatInputState}\n" +
            $"距下个节拍的百分比为：{MusicSyncTool.BeatPercent}");

        switch (MusicSyncTool.curBeatInputState)
        {
            case MusicSyncTool.BeatInputState.miss:
                txtState.color = Color.white;
                break;
            case MusicSyncTool.BeatInputState.good:
                txtState.color = Color.blue;
                break;
            case MusicSyncTool.BeatInputState.great:
                txtState.color = Color.green;
                break;
            case MusicSyncTool.BeatInputState.perfect:
                txtState.color = Color.red;
                break;
            default:
                txtState.color = Color.white;
                break;
        }

        txtState.text = MusicSyncTool.curBeatInputState.ToString();
        sldPercent.value = MusicSyncTool.BeatPercent;
    }

}
