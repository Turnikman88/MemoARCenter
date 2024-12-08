using System.Text;
using Xabe.FFmpeg;

namespace MemoARCenter.Helpers
{
    public static class Helper
    {
        public static string GetValidExtension(string extension, List<string> extsToCheck)
        {
            return extsToCheck.Contains(extension) ? extension : null;
        }


        public static IConversion ChangeBitRateAndFormat(this IConversion conversion, long bitRate)
        {
            if (bitRate > 1500000)
            {
                conversion.SetPreset(ConversionPreset.Medium)
                    .SetPixelFormat(PixelFormat.yuv420p)
                    .SetOutputFormat(Format.mp4)
                    .SetVideoBitrate(1500000);
            }

            return conversion;
        }

        public static IConversion AddAudioStream(this IConversion conversion, IMediaInfo mediaInfo)
        {
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

            if (audioStream != null)
            {
                conversion.AddStream(audioStream);
            }
            return conversion;
        }

        public static string EncodeToBase64(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(input);

            var base64 = Convert.ToBase64String(bytes);

            return base64;
        }
    }
}
