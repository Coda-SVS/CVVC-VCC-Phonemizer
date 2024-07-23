using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Plugin.Builtin {
    /// <summary>
    /// 한국어 CVVC-VVC Phonemizer입니다.
    /// </summary>
    [Phonemizer("Korean CVVC-VVC Phonemizer", "KO CVVC-VVC", "NoelKM", language: "KO")]
    public class KoreanCVVCVVCPhonemizer : Phonemizer
    {
        public class PhoneticUnit {
            public string nucleus { get; set; }
            public string prefix { get; set; }

            public PhoneticUnit(string nucleus, string prefix = "") {
                this.nucleus = nucleus;
                this.prefix = prefix;
            }
        }

        public class CVUnit : PhoneticUnit {
            public string onset { get; set; }

            public CVUnit(string onset, string nucleus, string prefix = "") : base(nucleus, prefix) {
                this.onset = onset;
            }
            
            public override string ToString() {
                string result = prefix != "" ? $"{prefix} " : "";
                result += onset != "null" ? $"{onset}{nucleus}" : nucleus;
                return result;
            }
        }

        public class VCUnit : PhoneticUnit {
            public string coda { get; set; }

            public VCUnit(string nucleus, string coda) : base(nucleus) {
                this.coda = coda;
            }

            public override string ToString() {
                string result = coda != "null" ? $"{nucleus} {coda}" : nucleus;
                return result;
            }
        }

        public class VVCUnit : VCUnit {
            public string coda2 { get; set; }

            public VVCUnit(string coda2, string nucleus, string coda) : base(nucleus, coda) {
                this.coda2 = coda2;
            }

            public VVCUnit(string coda2, VCUnit vc) : base(vc.nucleus, vc.coda) {
               this.coda2 = coda2;
            }

            public override string ToString() { 
                return $"{nucleus}{coda} {coda2}";
            }
        }

        public class VVUnit : PhoneticUnit {
            public string nucleus2 { get; set; }

            public VVUnit(string nucleus, string nucleus2) : base(nucleus) {
                this.nucleus2 = nucleus2;
            }

            public override string ToString() {
                return $"{nucleus} {nucleus2}";
            }
        }

        private readonly Dictionary<string, string> onset_symbol = new Dictionary<string, string>
        {
            { "ㄱ", "g" },
            { "ㄲ", "kk" },
            { "ㄴ", "n" },
            { "ㄷ", "d" },
            { "ㄸ", "tt" },
            { "ㄹ", "r" },
            { "ㅁ", "m" },
            { "ㅂ", "b" },
            { "ㅃ", "pp" },
            { "ㅅ", "s" },
            { "ㅆ", "ss" },
            { "ㅈ", "j" },
            { "ㅊ", "ch" },
            { "ㅋ", "k" },
            { "ㅌ", "t" },
            { "ㅍ", "p" },
            { "ㅎ", "h" },
            { "ㅇ", "null" },
            { "null", "null"}
        };

        private readonly Dictionary<string, string> y_onset_symbol = new Dictionary<string, string>
        {
            { "ㄱ", "gy" },
            { "ㄲ", "kky" },
            { "ㄴ", "ny" },
            { "ㄷ", "dy" },
            { "ㄸ", "tty" },
            { "ㄹ", "ry" },
            { "ㅁ", "my" },
            { "ㅂ", "by" },
            { "ㅃ", "ppy" },
            { "ㅅ", "sy" },
            { "ㅆ", "ssy" },
            { "ㅈ", "jy" },
            { "ㅊ", "chy" },
            { "ㅋ", "ky" },
            { "ㅌ", "ty" },
            { "ㅍ", "py" },
            { "ㅎ", "hy" },
            { "ㅇ", "y" },
            { "null", "null"}
        };

        private readonly Dictionary<string, string> w_onset_symbol = new Dictionary<string, string>
        {
            { "ㄱ", "gw" },
            { "ㄲ", "kkw" },
            { "ㄴ", "nw" },
            { "ㄷ", "dw" },
            { "ㄸ", "ttw" },
            { "ㄹ", "rw" },
            { "ㅁ", "mw" },
            { "ㅂ", "bw" },
            { "ㅃ", "ppw" },
            { "ㅅ", "sw" },
            { "ㅆ", "ssw" },
            { "ㅈ", "jw" },
            { "ㅊ", "chw" },
            { "ㅋ", "kw" },
            { "ㅌ", "tw" },
            { "ㅍ", "pw" },
            { "ㅎ", "hw" },
            { "ㅇ", "w" },
            { "null", "null"}
        };

        private readonly Dictionary<string, string> nucleus_symbol = new Dictionary<string, string>
        {
            { "ㅏ", "a" },
            { "ㅐ", "e" },
            { "ㅓ", "eo" },
            { "ㅔ", "e" },
            { "ㅗ", "o" },
            { "ㅜ", "u" },
            { "ㅡ", "eu" },
            { "ㅣ", "i" },
            { "null", "null"},
        };

        private readonly Dictionary<string, string> y_nucleus_symbol = new Dictionary<string, string>
        {
            { "ㅑ", "a" },
            { "ㅕ", "eo" },
            { "ㅒ", "e" },
            { "ㅖ", "e" },
            { "ㅛ", "o" },
            { "ㅠ", "u" },
            { "null", "null"},
        };

        private readonly Dictionary<string, string> w_nucleus_symbol = new Dictionary<string, string>
        {
            { "ㅘ", "a" },
            { "ㅙ", "e" },
            { "ㅚ", "e" },
            { "ㅝ", "o" },
            { "ㅞ", "e" },
            { "ㅟ", "i" },
            { "ㅢ", "i" },
            { "null", "null"},
        };

        private readonly Dictionary<string, string> coda_symbol = new Dictionary<string, string>
        {
            { "ㄴ", "n" },
            { "ㄹ", "l" },
            { "ㅁ", "m" },
            { "ㅇ", "ng" },
            { "ㄱ", "kcl" },
            { "ㅋ", "kcl" },
            { "ㄲ", "kcl" },
            { "ㄷ", "tcl" },
            { "ㅌ", "tcl" },
            { "ㄸ", "tcl" },
            { "ㅅ", "tcl" },
            { "ㅆ", "tcl" },
            { "ㅈ", "tcl" },
            { "ㅊ", "tcl" },
            { "ㅉ", "tcl" },
            { "ㅎ", "tcl" },
            { "ㅂ", "pcl" },
            { "ㅃ", "pcl" },
            { "ㅍ", "pcl" },
            { " ", "null"},
            { "null", "null"},
        };

        private USinger singer;

        /// <summary>
        /// 가수를 설정합니다.
        /// </summary>
        /// <param name="singer">설정할 가수입니다.</param>
        public override void SetSinger(USinger singer) => this.singer = singer;

        /// <summary>
        /// 문자열 배열에서 CVUnit과 VCUnit으로 변환합니다.
        /// </summary>
        /// <param name="lyrics">자모가 나누어져 있는 가사</param>
        /// <returns>CVUnit과 VCUnit을 반환합니다.</returns>
        private string[] ConvertPhonemes(string[] lyrics) {
            string onset, nucleus, coda;

            if (y_nucleus_symbol.ContainsKey(lyrics[1])) {
                onset = y_onset_symbol[lyrics[0]];
                nucleus = y_nucleus_symbol[lyrics[1]];
            } else if (w_nucleus_symbol.ContainsKey(lyrics[1])) {
                onset = w_onset_symbol[lyrics[0]];
                nucleus = w_nucleus_symbol[lyrics[1]];

            } else if (lyrics[1] == "ㅢ") // 추후 예외처리를 위한 코드
              {
                onset = w_onset_symbol[lyrics[0]];
                nucleus = w_nucleus_symbol[lyrics[1]];
            } else {
                onset = onset_symbol[lyrics[0]];
                nucleus = nucleus_symbol[lyrics[1]];
            }
            coda = coda_symbol[lyrics[2]];

            return new string[] { onset, nucleus, coda };
        }


        /// <summary>
        /// Oto에서 노트에 맞는 음성 유닛이 있는지 확인
        /// </summary>
        /// <param name="input">발음 기호</param>
        /// <param name="note">노트 (color등 정보를 가져오기 위해)</param>
        /// <param name="oto">oto 설정 파일</param>
        /// <returns>oto에 존재하는지 확인</returns>
        private bool CheckOtoUntilHit(string[] input, Note note, out UOto oto) {
            oto = default;
            var attr = note.phonemeAttributes?.FirstOrDefault(a => a.index == 0) ?? default;
            string color = attr.voiceColor ?? "";

            foreach (string test in input) {
                string testWithAlternate = test + attr.alternate;
                int toneShifted = note.tone + attr.toneShift;

                if (TryGetOto(testWithAlternate, toneShifted, color, out oto) || TryGetOto(test, toneShifted, color, out oto)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phoneme"></param>
        /// <param name="tone"></param>
        /// <param name="color"></param>
        /// <param name="oto"></param>
        /// <returns></returns>
        private bool TryGetOto(string phoneme, int tone, string color, out UOto oto) {
            return singer.TryGetMappedOto(phoneme, tone, color, out oto) && (string.IsNullOrEmpty(color) || oto.Color == color);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notes">변환 할 노트</param>
        /// <param name="prev">이전 노트</param>
        /// <param name="next">다음 노트</param>
        /// <param name="prevNeighbour">이전 인접 노트 (prev가 note와 붙어 있는 경우, prev와 동일)</param>
        /// <param name="nextNeighbour">다음 인접 노트 (next가 note와 붙어 있는 경우, next와 동일)</param>
        /// <param name="prevs">+나 -와 같은 extended notes임</param>
        /// <returns></returns>
        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevs) {
            Note note = notes[0];

            // phoneticHint가 존재할 경우, 해당 힌트를 통해 직접 반환합니다.
            if (!string.IsNullOrEmpty(note.phoneticHint)) {
                if (CheckOtoUntilHit(new string[] { note.phoneticHint.Normalize() }, note, out var ph)) {
                    return new Result {
                        phonemes = new Phoneme[] {
                            new Phoneme {
                                phoneme = ph.Alias,
                            }
                        },
                    };
                }
            }

            List<PhoneticUnit> phoneticUnits = new List<PhoneticUnit>();
            

            // 이전 노드가 있는 경우
            if (prevNeighbour.HasValue) {
                phoneticUnits.AddRange(MakeEnding((Note)prevNeighbour, MakePhone(note)));
            } else {
               phoneticUnits.AddRange(MakePhone(note));
            }

            // 다음 노드가 없는 경우
            if (!nextNeighbour.HasValue) {
                phoneticUnits.AddRange(MakeEnding(note, new List<PhoneticUnit>()));
            }

            return new Result {
                phonemes = phoneticUnits.Select(p => new Phoneme { phoneme = p.ToString() }).ToArray(),
            };
        }

        public List<PhoneticUnit> MakePhone(Note note) {
            Hashtable lyrics = KoreanPhonemizerUtil.Separate(note.lyric.Normalize());
            string[] phonemes = ConvertPhonemes(new string[] {
                (string) lyrics[0],
                (string) lyrics[1],
                (string) lyrics[2]
            });

            // 아래에 추가 구현 하면 될 듯

            return new List<PhoneticUnit>();
        }

        public List<PhoneticUnit> MakeEnding(Note note, List<PhoneticUnit> next) {
            Hashtable lyrics = KoreanPhonemizerUtil.Separate(note.lyric.Normalize());
            string[] phonemes = ConvertPhonemes(new string[] {
                (string) lyrics[0],
                (string) lyrics[1],
                (string) lyrics[2]
            });

            // 아래에 추가 구현 하면 될 듯

            // Functional 파이프라인 PoC
            List<Func<List<PhoneticUnit>, List<PhoneticUnit>>> pipeline = new List<Func<List<PhoneticUnit>, List<PhoneticUnit>>>();

            CVUnit cv = new CVUnit(phonemes[0], phonemes[1]);
            VCUnit vc = new VCUnit(phonemes[1], phonemes[2]);
            List<PhoneticUnit> phoneticUnits = new List<PhoneticUnit>();
            phoneticUnits.Add(cv);
            phoneticUnits.Add(vc);

            pipeline.Add(sampleProcess1);
            if (true) {
                pipeline.Add(sampleProcess2);
            } else {
               pipeline.Add(sampleProcess3);
            }

            foreach (var proc in pipeline) {
               phoneticUnits = proc(phoneticUnits);
            }

            // PoC 끝

            return new List<PhoneticUnit>();
        }

        public List<PhoneticUnit> sampleProcess1(List<PhoneticUnit> phoneticUnits) {
            return new List<PhoneticUnit>();
        }
        public List<PhoneticUnit> sampleProcess2(List<PhoneticUnit> phoneticUnits) {
            return new List<PhoneticUnit>();
        }
        public List<PhoneticUnit> sampleProcess3(List<PhoneticUnit> phoneticUnits) {
            return new List<PhoneticUnit>();
        }
    }
}
