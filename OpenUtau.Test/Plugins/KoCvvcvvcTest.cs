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
            new string[] { "- a", "a n", "nyeo", "eo ng" })]
        [InlineData("ko_cvvcvvc",
            new string[] { "R", "가", "R", "나" },
            new string[] { "C4", "C4", "C4", "C4" },
            new string[] { "", "", "", "" },
            new string[] { "R", "- ga", "R", "- na" })]
        public void PhonemizeTest(string singerName, string[] lyrics, string[] tones, string[] colors, string[] aliases) {
            RunPhonemizeTest(singerName, lyrics, RepeatString(lyrics.Length, ""), tones, colors, aliases);
        }
    }
}

