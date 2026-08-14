using Network.Header;

namespace BackendJX3D.Core.Utils;


public static class ScriptContent
{
    public static string[] Split(byte[]? content, int bufferLen)
    {
        if (content == null || bufferLen <= 0)
            return [];

        // m_nBufferLen do server khai, đừng tin: kẹp lại theo mảng thật (2048 byte)
        var end = Math.Min(bufferLen, content.Length);

        var segments = new List<string>();
        var start = 0;

        // i == end là mốc xả đoạn cuối, nên vòng lặp chạy tới hết chứ không dừng ở end - 1
        for (var i = 0; i <= end; i++)
        {
            if (i < end && content[i] != 0) continue;

            if (i > start)
            {
                var chunk = new byte[i - start];

                Array.Copy(content, start, chunk, 0, chunk.Length);

                // Decode từng đoạn: đoạn đã không còn byte 0 nên DecodeBytes không cắt oan
                var text = Converter.DecodeBytes(chunk);

                if (!string.IsNullOrEmpty(text))
                    segments.Add(text);
            }

            start = i + 1;
        }

        return segments.ToArray();
    }
}
