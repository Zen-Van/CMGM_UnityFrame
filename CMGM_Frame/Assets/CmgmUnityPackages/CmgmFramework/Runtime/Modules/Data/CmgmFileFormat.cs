using System;
using System.IO;
using System.Text;

namespace CMGM.Data
{
    /// <summary>
    /// .cmgm 磁盘文件种类；写入文件头 offset 8 处的 uint8。
    /// </summary>
    public enum CmgmFileKind : byte
    {
        Archive = 0,
        Config = 1,
    }

    /// <summary>
    /// v1 容器头只读视图（不含 payload）。
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
    /// .cmgm 统一容器：固定文件头 + 分类型 payload。
    /// <para>读写顺序（接入方）：<c>Pack</c> → <see cref="CipherTool"/> → 写盘；读盘反向。</para>
    /// <para>本类不处理加解密，也不解析 payload 正文。</para>
    /// </summary>
    public static class CmgmFileFormat
    {
        public const uint ContainerVersion = 1;

        /// <summary>文件头总字节数：magic(4) + version(4) + kind(1)。</summary>
        public const int HeaderSize = 4 + 4 + 1;

        private const int MagicOffset = 0;
        private const int VersionOffset = 4;
        private const int KindOffset = 8;

        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes("CMGM");

        /// <summary>
        /// 将 payload 封装为带 v1 文件头的完整容器字节（未加密）。
        /// </summary>
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
        /// 解析容器字节，返回头信息与 payload（未加密输入）。
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="containerBytes"/> 为 null。</exception>
        /// <exception cref="InvalidDataException">魔数、版本或 kind 不合法。</exception>
        public static CmgmFileHeader Unpack(byte[] containerBytes, out byte[] payload)
        {
            if (containerBytes == null)
                throw new ArgumentNullException(nameof(containerBytes));

            var header = ReadHeader(containerBytes, out payload);
            return header;
        }

        /// <summary>
        /// 尝试解析；失败时 <paramref name="payload"/> 为 null。
        /// </summary>
        public static bool TryUnpack(byte[] containerBytes, out CmgmFileHeader header, out byte[] payload)
        {
            header = default;
            payload = null;

            if (!TryReadHeader(containerBytes, out header, out payload))
                return false;

            return true;
        }

        /// <summary>
        /// 缓冲区开头是否为 v1 魔数 <c>CMGM</c>（用于区分无头旧档）。
        /// </summary>
        public static bool HasMagic(ReadOnlySpan<byte> bytes)
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

        /// <summary>
        /// 仅读取文件头，不拷贝 payload。
        /// </summary>
        public static CmgmFileHeader ReadHeaderOnly(ReadOnlySpan<byte> containerBytes)
        {
            if (!TryReadHeader(containerBytes, out var header, out _))
                throw new InvalidDataException("Not a valid CMGM container file.");

            return header;
        }

        private static void WriteHeader(byte[] buffer, uint version, CmgmFileKind kind)
        {
            Buffer.BlockCopy(MagicBytes, 0, buffer, MagicOffset, MagicBytes.Length);
            WriteUInt32(buffer, VersionOffset, version);
            buffer[KindOffset] = (byte)kind;
        }

        private static CmgmFileHeader ReadHeader(byte[] containerBytes, out byte[] payload)
        {
            if (!TryReadHeader(containerBytes, out var header, out payload))
                throw new InvalidDataException("Not a valid CMGM container file.");

            return header;
        }

        private static bool TryReadHeader(ReadOnlySpan<byte> containerBytes, out CmgmFileHeader header, out byte[] payload)
        {
            header = default;
            payload = null;

            if (!HasMagic(containerBytes))
                return false;

            if (containerBytes.Length < HeaderSize)
                return false;

            uint version = ReadUInt32(containerBytes, VersionOffset);
            if (version != ContainerVersion)
                return false;

            byte kindValue = containerBytes[KindOffset];
            if (!Enum.IsDefined(typeof(CmgmFileKind), kindValue))
                return false;

            header = new CmgmFileHeader(version, (CmgmFileKind)kindValue);

            int payloadLength = containerBytes.Length - HeaderSize;
            if (payloadLength <= 0)
            {
                payload = Array.Empty<byte>();
                return true;
            }

            payload = containerBytes.Slice(HeaderSize, payloadLength).ToArray();
            return true;
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
