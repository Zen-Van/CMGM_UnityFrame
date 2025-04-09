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
    }

}
