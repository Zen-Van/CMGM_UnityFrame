using System;
using System.IO;
using System.Text;
using CMGM.Core;

namespace CMGM.Data
{
    public enum CmgmFileKind : byte
    {
        Archive = 0,
        Config = 1,
    }

    /// <summary>
    /// 从文件头读出的 version + kind（不含 payload）。
    /// </summary>
    public readonly struct CmgmFileHeader
    {
        public uint Version { get; }
        public CmgmFileKind Kind { get; }

        public CmgmFileHeader(uint version, CmgmFileKind kind)
        {
            Version = version;
            Kind = kind;
        }
    }

    /// <summary>
    /// .cmgm 统一容器：9 字节头 + payload。不处理 CipherTool，不解析 payload 正文。
    /// <para>写：<see cref="Pack"/> → CipherTool → 写盘。</para>
    /// <para>读：读盘 → CipherTool → <see cref="Unpack"/> → 各 Codec。</para>
    /// </summary>
    public static class CmgmFileFormat
    {
        public const uint ContainerVersion = 1;
        public const int HeaderSize = 4 + 4 + 1;

        private const int VersionOffset = 4;
        private const int KindOffset = 8;

        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes("CMGM");

        /// <summary>拼容器字节（未加密）。</summary>
        public static byte[] Pack(CmgmFileKind kind, byte[] payload)
        {
            payload ??= Array.Empty<byte>();

            var container = new byte[HeaderSize + payload.Length];
            WriteHeader(container, ContainerVersion, kind);
            if (payload.Length > 0)
                Buffer.BlockCopy(payload, 0, container, HeaderSize, payload.Length);
            return container;
        }

        /// <summary>
        /// 校验头并取出 payload。kind 不合法时抛异常；version 与运行时不一致时仅警告并仍返回 payload。
        /// </summary>
        /// <param name="header">读出的容器头（含文件中的 version）。</param>
        public static byte[] Unpack(
            byte[] containerBytes,
            CmgmFileKind expectedKind,
            string fileLabel,
            out CmgmFileHeader header)
        {
            if (containerBytes == null)
                throw new ArgumentNullException(nameof(containerBytes));

            if (!TryReadHeader(containerBytes, out header))
            {
                CmgmLog.fError($"文件 {fileLabel} 不是合法的 CMGM 容器，无法读取。");
                throw new InvalidDataException($"File '{fileLabel}' is not a valid CMGM container.");
            }

            if (header.Kind != expectedKind)
            {
                CmgmLog.fError($"文件 {fileLabel} 的 kind 为 {header.Kind}，期望 {expectedKind}。");
                throw new InvalidDataException(
                    $"File '{fileLabel}' has kind {header.Kind}, expected {expectedKind}.");
            }

            if (header.Version != ContainerVersion)
            {
                CmgmLog.fWarning(
                    $"文件 {fileLabel} 的 CMGM 容器 version={header.Version}，当前运行时期望 version={ContainerVersion}；"
                    + "仍将按当前 payload Codec 尝试解码，结果可能错误，解码后将打印到控制台供核对。");
            }

            return SlicePayload(containerBytes);
        }

        /// <summary>能否读出 magic + version + kind（不表示 version 受支持）。</summary>
        private static bool TryReadHeader(ReadOnlySpan<byte> bytes, out CmgmFileHeader header)
        {
            header = default;

            if (bytes.Length < HeaderSize || !HasMagic(bytes))
                return false;

            byte kindValue = bytes[KindOffset];
            if (!Enum.IsDefined(typeof(CmgmFileKind), kindValue))
                return false;

            header = new CmgmFileHeader(ReadUInt32(bytes, VersionOffset), (CmgmFileKind)kindValue);
            return true;
        }

        private static bool HasMagic(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < MagicBytes.Length)
                return false;

            for (int i = 0; i < MagicBytes.Length; i++)
            {
                if (bytes[i] != MagicBytes[i])
                    return false;
            }

            return true;
        }

        private static void WriteHeader(byte[] buffer, uint version, CmgmFileKind kind)
        {
            Buffer.BlockCopy(MagicBytes, 0, buffer, 0, MagicBytes.Length);
            WriteUInt32(buffer, VersionOffset, version);
            buffer[KindOffset] = (byte)kind;
        }

        private static byte[] SlicePayload(ReadOnlySpan<byte> containerBytes)
        {
            int length = containerBytes.Length - HeaderSize;
            if (length <= 0)
                return Array.Empty<byte>();

            return containerBytes.Slice(HeaderSize, length).ToArray();
        }

        private static void WriteUInt32(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        private static uint ReadUInt32(ReadOnlySpan<byte> buffer, int offset)
        {
            return (uint)(buffer[offset]
                | (buffer[offset + 1] << 8)
                | (buffer[offset + 2] << 16)
                | (buffer[offset + 3] << 24));
        }
    }
}
