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
        [InlineData("ko_cvvcvvc",
            new string[] { "가", "나" },
            new string[] { "C4", "C4" },
            new string[] { "", "" },
            new string[] { "- ga", "a n", "na" })]
        [InlineData("ko_cvvcvvc",
            new string[] { "라", "자" },
            new string[] { "C4", "C4" },
            new string[] { "", "" },
            new string[] { "- ra", "a j", "ja" })]
        [InlineData("ko_cvvcvvc",
            new string[] { "가", "다" },
            new string[] { "C4", "C4" },
            new string[] { "", "" },
            new string[] { "- ga", "a d", "da" })]
        [InlineData("ko_cvvcvvc",
            new string[] { "사", "지" },
            new string[] { "C4", "C4" },
            new string[] { "", "" },
            new string[] { "- sa", "a j", "ji" })]
        /*[InlineData("ko_cvvcvvc",
            new string[] { "가", "이" },
            new string[] { "C4", "C4" },
            new string[] { "", "" },
            new string[] { "- ga", "a i" })]
        [InlineData("ko_cvvcvvc",
            new string[] { "어", "우" },
            new string[] { "C4", "C4" },
            new string[] { "", "" },
            new string[] { "- eo", "eo u" })]
        [InlineData("ko_cvvcvvc",
            new string[] { "안", "아" },
            new string[] { "C4", "C4" },
            new string[] { "", "" },
            new string[] { "- a", "a n", "na" })] */
        public void PhonemizeTest(string singerName, string[] lyrics, string[] tones, string[] colors, string[] aliases) {
            RunPhonemizeTest(singerName, lyrics, RepeatString(lyrics.Length, ""), tones, colors, aliases);
        }
    }
}

