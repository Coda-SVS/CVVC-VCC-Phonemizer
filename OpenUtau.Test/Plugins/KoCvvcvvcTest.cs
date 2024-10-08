﻿using System;
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

        protected KoreanCVVCVVCPhonemizer.PhoneticContext GetDummyContext(string lyric, string prevLyric="null", string nextLyric="null") {
            KoreanCVVCVVCPhonemizer phonemizer = (KoreanCVVCVVCPhonemizer)CreatePhonemizer();
            return phonemizer.InitContext(
                new Phonemizer.Note() { lyric = lyric, position = 480, duration = 240 }, 
                (prevLyric != "null") ? new Phonemizer.Note() { lyric = prevLyric, position = 240, duration = 240 } : null,
                (nextLyric != "null") ? new Phonemizer.Note() { lyric = nextLyric, position = 720, duration = 240 } : null
            );
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
            new string[] { "- sa", "a jy", "ji" })]
        [InlineData("ko_cvvcvvc",
            new string[] { "아", "시"},
            new string[] { "C4", "C4"},
            new string[] { "", "" },
            new string[] { "- a", "a sy", "si" })]
        [InlineData("ko_cvvcvvc",
            new string[] { "안", "까" },
            new string[] { "C4", "C4" },
            new string[] { "", "" },
            new string[] { "- a", "an kcl", "kka" })]
        [InlineData("ko_cvvcvvc",
            new string[] { "알", "찌" },
            new string[] { "C4", "C4" },
            new string[] { "", "" },
            new string[] { "- a", "al tcl", "jji" })]
        [InlineData("ko_cvvcvvc",
            new string[] { "잉", "싸" },
            new string[] { "C4", "C4" },
            new string[] { "", "" },
            new string[] { "- i", "ing ss", "ssa" })]
        [InlineData("ko_cvvcvvc",
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
            new string[] { "- a", "a n", "a" })] 
        public void PhonemizeTest(string singerName, string[] lyrics, string[] tones, string[] colors, string[] aliases) {
            RunPhonemizeTest(singerName, lyrics, RepeatString(lyrics.Length, ""), tones, colors, aliases);
        }

        [Fact]
        public void AddCVUnitCase() {
            // 일반적인 상황에서 CV 유닛 추가
            var phonemizer = (KoreanCVVCVVCPhonemizer)CreatePhonemizer();
            var context = GetDummyContext("가", "나");
            context = phonemizer.AddCVUnit(context);
            
            Assert.Equal("ga", context.units[0].ToString());
        }

        [Fact]
        public void AddCVUnitCaseWithRNote() {
            // 이전 유닛이 R 노트일 경우 CV 유닛에 prefix 추가
            var phonemizer = (KoreanCVVCVVCPhonemizer)CreatePhonemizer();
            var context = GetDummyContext("가", "R");
            context = phonemizer.AddCVUnit(context);

            Assert.Equal("-", context.units[0].prefix);
            Assert.Equal("- ga", context.units[0].ToString());
        }

        [Fact]
        public void AddVCUnitCase() {
            var phonemizer = (KoreanCVVCVVCPhonemizer)CreatePhonemizer();
            var context = GetDummyContext("안");

            context = phonemizer.AddVCUnit(context);

            var vc = (KoreanCVVCVVCPhonemizer.VCUnit)context.units[0];
            Console.WriteLine(vc.ToString());
            Assert.Equal("a", vc.nucleus);
            Assert.Equal("n", vc.coda);
        }

        [Fact]
        public void VC2VCyCase() {
            var phonemizer = (KoreanCVVCVVCPhonemizer)CreatePhonemizer();
            var context = GetDummyContext("아", "null", "시");

            context = phonemizer.AddVCUnit(context);
            context = phonemizer.VC2VCy(context);

            var vc = (KoreanCVVCVVCPhonemizer.VCUnit)context.units[0];

            Assert.Equal("sy", vc.coda);
        }

        [Fact]
        public void VC2VVCUnitCase() {
            var phonemizer = (KoreanCVVCVVCPhonemizer)CreatePhonemizer();
            var context = GetDummyContext("밤", "null", "피");

            context = phonemizer.AddVCUnit(context);
            context = phonemizer.VC2VCy(context);
            context = phonemizer.VC2VVC(context);

            Assert.True(context.units[0] is KoreanCVVCVVCPhonemizer.VVCUnit);

            var vvc = (KoreanCVVCVVCPhonemizer.VVCUnit)context.units[0];

            Assert.Equal("pcl", vvc.coda2);
        }

        [Fact]
        public void CV2VVUnitCase() {
            var phonemizer = (KoreanCVVCVVCPhonemizer)CreatePhonemizer();
            var context = GetDummyContext("이", "아");

            context = phonemizer.AddVCUnit(context);
            context = phonemizer.AddCVUnit(context);
            context = phonemizer.CV2VV(context);

            Assert.True(context.units[0] is KoreanCVVCVVCPhonemizer.VVUnit);
        }
    }
}

