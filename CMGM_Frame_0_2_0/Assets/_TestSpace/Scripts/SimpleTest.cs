using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SimpleTest : MonoBehaviour
{
    private async void Awake()
    {
        CmgmLog.fNormal("SimpleTest Awake");

        ResourcesResMgr.Instance.Init();

        GameObject cube = await AddressablesResMgr.Instance.LoadAssetAsync<GameObject>("LevelPrefabs/Cube.prefab");
        Instantiate(cube, Vector3.zero, Quaternion.identity);


        WwiseAudioManager.Instance.PlayCommonBgm("Play_TestMusic");
    }

    private void Start()
    {
        
    }

    private int bgmPosition;

    private void Update()
    {
        //得到的是在Source上的位置（即每次循环清空&暂停不计时），单位是毫秒
        AkUnitySoundEngine.GetSourcePlayPosition(MusicSyncTool.curGameBgmPlayingId, out bgmPosition);
        CmgmLog.fNormal($"音乐播放的进度为：{bgmPosition}");
    }

}
