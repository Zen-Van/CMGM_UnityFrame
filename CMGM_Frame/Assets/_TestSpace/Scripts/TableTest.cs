using System.Collections;
using System.Collections.Generic;
using CMGM.Bootstrap;
using CMGM.Data;
using CMGM.Workspace;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TableTest : MonoBehaviour
{

    private void Awake()
    {
        
    }

    void Start()
    {
        RoleInfo roleInfo = ConfigTableManager.Instance.GetTable<RoleInfo>();
        foreach (var key in roleInfo.dataDic.Keys)
        {
            Debug.Log($"{key},{roleInfo.dataDic[key].name}");
        }
    }

    private void Update()
    {
        
    }
}
