using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace MemoARCenter.Services.Helpers
{
    public static class Helper
    {
        public static string GetValidExtension(string extension, List<string> extsToCheck)
        {
            return extsToCheck.Contains(extension) ? extension : null;
        }


        public static IConversion ChangeBitRateAndFormat(this IConversion conversion, long bitRate)
        {
            if (bitRate >= 2000000)
            {
                conversion.SetPreset(ConversionPreset.Medium)
                    .SetPixelFormat(PixelFormat.yuv420p)
                    //                   .SetOutputFormat(Format.hevc)
                    .SetVideoBitrate(2000000);
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

        
    }
}
