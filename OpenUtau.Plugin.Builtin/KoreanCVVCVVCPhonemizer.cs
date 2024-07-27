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
            public int position { get; set; }

            public PhoneticUnit(string nucleus, int position, string prefix = "") {
                this.nucleus = nucleus;
                this.prefix = prefix;
            }
        }

        public class CVUnit : PhoneticUnit {
            public string onset { get; set; }

            public CVUnit(string onset, string nucleus, int position, string prefix = "") : base(nucleus, position, prefix) {
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

            public VCUnit(string nucleus, string coda, int position) : base(nucleus, position) {
                this.coda = coda;
            }

            public override string ToString() {
                string result = coda != "null" ? $"{nucleus} {coda}" : nucleus;
                return result;
            }
        }

        public class VVCUnit : VCUnit {
            public string coda2 { get; set; }

            public VVCUnit(string coda2, string nucleus, string coda, int position) : base(nucleus, coda, position) {
                this.coda2 = coda2;
            }

            public VVCUnit(string coda2, VCUnit vc) : base(vc.nucleus, vc.coda, vc.position) {
               this.coda2 = coda2;
            }

            public VVCUnit(VCUnit vc, CVUnit cv) : base(vc.nucleus, vc.coda, vc.position) {
               this.coda2 = cv.onset;
            }

            public override string ToString() { 
                return $"{nucleus}{coda} {coda2}";
            }
        }

        public class VVUnit : PhoneticUnit {
            public string nucleus2 { get; set; }

            public VVUnit(string nucleus, string nucleus2, int position) : base(nucleus, position) {
                this.nucleus2 = nucleus2;
            }

            public VVUnit(VCUnit vc, CVUnit cv) : base(vc.nucleus, cv.position) {
                this.nucleus2 = cv.nucleus;
            }

            public override string ToString() {
                return $"{nucleus} {nucleus2}";
            }
        }

        public struct Lyrics {
            public string onset;
            public string nucleus;
            public string coda;
            public int position;

            public Lyrics(string[] phonemes, int position) {
                this.onset = phonemes[0];
                this.nucleus = phonemes[1];
                this.coda = phonemes[2];
                this.position = position;
            }
        }

        public struct PhoneticContext {
            public Lyrics note;
            public Lyrics? prev;
            public List<PhoneticUnit> units;
            public bool isNeighbour;
            public bool isEnding;
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
            { "ㅉ", "jj" },
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

        private readonly Dictionary<string, string> n_coda_symbol = new Dictionary<string, string>
        {
            { "ㄴ", "n" },
            { "ㄹ", "l" },
            { "ㅁ", "m" },
            { "ㅇ", "ng" },
        };

        private readonly Dictionary<string, string> k_coda_symbol = new Dictionary<string, string>
        {
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
        };

        private readonly Dictionary<string, string> vc2vvc_symbol = new Dictionary<string, string> {
            {"gh", "kcl" },
            {"k", "kcl" },
            {"kk", "kcl" },
            {"dh", "tcl" },
            {"t", "tcl" },
            {"tt", "tcl" },
            {"jh", "tcl" },
            {"jj", "tcl" },
            {"bh", "pcl" },
            {"pp", "pcl" },
            {"p", "pcl" },
            {"ss", "ss" },
        };

        private readonly HashSet<String> cy_symbol = new HashSet<string> {
            "s", "j", "ss"
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

            try {
                coda = n_coda_symbol[lyrics[2]];
            } catch(KeyNotFoundException ne) {
                try {
                    coda = k_coda_symbol[lyrics[2]];
                } catch(KeyNotFoundException ke) {
                    coda = "null";
                }
            }

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
        /// 포네마이저 메인 루틴
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

            // R 노트에 대한 예외처리
            if (note.lyric == "R") {
                return MakeSimpleResult("R");
            }

            PhoneticContext context = InitContext(note, prev);
            
            // 이전 노드가 있는 경우
            if (prevNeighbour.HasValue) {
                context = MakeEnding(context);
            }

            context = MakePhone(context);

            // 다음 노드가 없는 경우
            if (!nextNeighbour.HasValue) {
                context.isEnding = true;
                context = MakeEnding(context);
            }

            return MakeResult(context);
        }

        /// <summary>
        /// 노트의 앞부분에 해당하는 음소를 처리합니다.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public PhoneticContext MakePhone(PhoneticContext context) {
            context = AddCVUnit(context);
            context = CV2VV(context);

            return context;
        }

        /// <summary>
        /// 노트의 뒷부분에 해당하는 음소를 처리합니다.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public PhoneticContext MakeEnding(PhoneticContext context) {
            context = AddVCUnit(context);
            context = VC2VCy(context);
            context = VC2VVC(context);

            return context;
        }

        /// <summary>
        /// 파이프라인을 실행하기 위한 문맥 정보를 생성합니다.
        /// </summary>
        /// <param name="note"></param>
        /// <param name="prev"></param>
        /// <returns></returns>
        public PhoneticContext InitContext(Note note, Note? prev) {
            Hashtable lyrics = KoreanPhonemizerUtil.Separate(note.lyric.Normalize());
            string[] phonemes = ConvertPhonemes(new string[] {
                (string)lyrics[0],
                (string)lyrics[1],
                (string)lyrics[2]
            });
            string[]? prevPhonemes = null;
            if (prev != null && prev.Value.lyric != "R") {
                Hashtable prevLyrics = KoreanPhonemizerUtil.Separate(prev.Value.lyric.Normalize());
                prevPhonemes = ConvertPhonemes(new string[] {
                    (string)prevLyrics[0],
                    (string)prevLyrics[1],
                    (string)prevLyrics[2]
                });
            }
            
            return new PhoneticContext { 
                note = new Lyrics(phonemes, note.position),
                prev = prevPhonemes != null ? new Lyrics(prevPhonemes, prev.Value.position) : (Lyrics?)null,
                units = new List<PhoneticUnit>()
            };
        }

        /// <summary>
        /// 초성과 중성을 합쳐 CVUnit으로 변환합니다.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public PhoneticContext AddCVUnit(PhoneticContext context) {
            context.units.Add(new CVUnit(context.note.onset, context.note.nucleus, context.note.position));
            if (!context.prev.HasValue) {
                context.units.Last().prefix = "-";
            }

            return context;
        }

        /// <summary>
        /// 중성과 종성을 합쳐 VCUnit으로 변환합니다.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public PhoneticContext AddVCUnit(PhoneticContext context) {
            if (context.isEnding) {
                if (context.note.coda != "null") {
                    context.units.Add(new VCUnit(context.note.nucleus, context.note.coda, context.note.position + context.note.position/2));
                }
            } else {
                if (context.prev != null) {
                    if (context.prev.Value.coda != "null") {
                        context.units.Add(new VCUnit(context.prev.Value.nucleus, context.prev.Value.coda, context.note.position - context.prev.Value.position/2));
                    } else {
                        context.units.Add(new VCUnit(context.prev.Value.nucleus, context.note.onset, context.note.position - context.prev.Value.position / 2));
                    }
                }
            }

            return context;
        }

        /// <summary>
        /// 모음 뒤에 si, ji, ssi가 오는 경우 y추가
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public PhoneticContext VC2VCy(PhoneticContext context) {
            if (context.isEnding || (context.note.nucleus != "i" || context.prev.Value.coda != "null")) {
                return context;
            }

            if (cy_symbol.Contains(context.note.onset)) {
                if (context.units.Last() is VCUnit vc) {
                    vc.coda += "y";
                } else if (context.units.Last() is VVCUnit vvc) {
                    vvc.coda2 += "y";
                }
            }
            return context;
        }

        /// <summary>
        /// m, n, l, ng 뒤에 gh, k, dh, t, jh, bh, p, kk, tt, ss, jj, pp가 오는 경우 VVC로 변환
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public PhoneticContext VC2VVC(PhoneticContext context) {
            if (!context.isEnding && context.prev != null) {
                if (n_coda_symbol.ContainsValue(context.prev.Value.coda) && vc2vvc_symbol.ContainsKey(context.note.onset)) {
                    VCUnit vc = (VCUnit)context.units.Last();
                    VVCUnit vcc = new VVCUnit(vc2vvc_symbol[context.note.onset], vc);
                    context.units.RemoveAt(context.units.Count - 1);
                    context.units.Add(vcc);
                }
            }
            return context;
        }

        /// <summary>
        /// VCUnit에 coda가 없고, CVUnit에 onset이 없는 경우 VV로 변환
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public PhoneticContext CV2VV(PhoneticContext context) {
            if (context.prev.HasValue) {
                if (context.note.onset == "null" && context.prev.Value.coda == "null") {
                    var vv = new VVUnit(context.prev.Value.nucleus, context.note.nucleus, context.note.position);
                    context.units.Clear();
                    context.units.Add(vv);
                }
            }
            return context;
        }

        public Result MakeResult(PhoneticContext context) {
            return new Result {
                phonemes = context.units.Select(p => new Phoneme { phoneme = p.ToString(), position = p.position }).ToArray(),
            };
        }
    }
}
