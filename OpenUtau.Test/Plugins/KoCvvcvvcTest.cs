using OpenUtau.Api;
using OpenUtau.Plugin.Builtin;
using Xunit;
using Xunit.Abstractions;

namespace OpenUtau.Plugins {
    public class KoCvvcvvcTest : PhonemizerTestBase {
        public KoCvvcvvcTest(ITestOutputHelper output) : base(output) { }

        protected override Phonemizer CreatePhonemizer() {
            return new KoreanCVVCVVCPhonemizer();
        }

        [Theory]
        [InlineData("ko_cvvcvvc",
            new string[] { "안", "녕"},
            new string[] { "C4", "C4"},
            new string[] { "", "" },
            new string[] {"안", "녕"})]
        public void PhonemizeTest(string singerName, string[] lyrics, string[] tones, string[] colors, string[] aliases) {
            RunPhonemizeTest(singerName, lyrics, RepeatString(lyrics.Length, ""), tones, colors, aliases);
        }
    }
}

