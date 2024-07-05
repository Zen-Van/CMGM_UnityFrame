using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 加密器，静态类，只有两个接口，加密和解密
/// </summary>
public static class CipherTool
{
    //重要！！：每次修改加密算法后必须清空本地存档文件，不然存档文件将会读出乱码。
    //可以通过编辑器窗口上方Tab栏中的【草木句萌】-->【清空所有存档数据】来清除
    //所有配置数据也必须重导

    private const byte passkey = 238;

    //加密解密方法已被数据管理系统自动调用
    //如果需要添加在这两个函数中添加内容即可
    //默认为空
    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="bytes">需要加密的字节码</param>
    public static void Encryption(ref byte[] bytes)
    {
        //for (int i = 0; i < bytes.Length; i++)
        //{
        //    bytes[i] ^= passkey;
        //}
    }
    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="bytes">需要解密的字节码</param>
    public static void Decryption(ref byte[] bytes)
    {
        //for (int i = 0; i < bytes.Length; i++)
        //{
        //    bytes[i] ^= passkey;
        //}
    }
}
