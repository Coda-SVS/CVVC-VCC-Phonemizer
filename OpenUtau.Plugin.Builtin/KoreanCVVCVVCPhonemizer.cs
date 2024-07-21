using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Classic;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Plugin.Builtin {
    /// <summary>
    /// 한국어 CVVC-VVC Phonemizer입니다.
    /// </summary>
    [Phonemizer("Korean CVVC-VVC Phonemizer", "KO CVVC-VVC", "NoelKM", language: "KO")]
    public class KoreanCVVCVVCPhonemizer : BaseKoreanPhonemizer
    {
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

        public class PhoneticUnit {
            public string nucleus { get; protected set; }
            public string prefix { get; protected set; }

            public PhoneticUnit(string nucleus, string prefix = "") {
                this.nucleus = nucleus;
                this.prefix = prefix;
            }

            public void ChangePrefix(string prefix) {
                this.prefix = prefix;
            }
        }

        public class CVUnit : PhoneticUnit {
            public string onset { get; private set; }

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
            public string coda { get; private set; }

            public VCUnit(string nucleus, string coda) : base(nucleus) {
                this.coda = coda;
            }

            public override string ToString() {
                string result = coda != "null" ? $"{nucleus} {coda}" : nucleus;
                return result;
            }
        }

        public class VVCUnit : VCUnit {
            public string coda2 { get; private set; }

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
            public string nucleus2 { get; private set; }

            public VVUnit(string nucleus, string nucleus2) : base(nucleus) {
                this.nucleus2 = nucleus2;
            }

            public override string ToString() {
                return $"{nucleus} {nucleus2}";
            }
        }

        /// <summary>
        /// 가수를 설정합니다.
        /// </summary>
        /// <param name="singer">설정할 가수입니다.</param>
        public override void SetSinger(USinger singer) {
            if (this.singer == singer || singer == null || singer.SingerType != USingerType.Classic) {
                return;
            }

            this.singer = singer;
        }

        private string? FindInOto(String phoneme, Note note, bool nullIfNotFound = false) {
            return BaseKoreanPhonemizer.FindInOto(singer, phoneme, note, nullIfNotFound);
        }

        /// <summary>
        /// 주어진 음표들을 음소로 변환합니다.
        /// </summary>
        /// <param name="notes">변환할 노트들의 배열입니다.</param>
        /// <param name="prev">이전 노트입니다.</param>
        /// <param name="next">다음 노트입니다.</param>
        /// <param name="prevNeighbour">이전 인접 노트입니다.</param>
        /// <param name="nextNeighbour">다음 인접 노트입니다.</param>
        /// <param name="prevNeighbours">이전 인접 음표들의 배열입니다.</param>
        /// <returns>변환된 음소 결과입니다.</returns>
        public override Result ConvertPhonemes(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours)
        {
            Note note = notes[0];
            Hashtable lyrics = KoreanPhonemizerUtil.Variate(prevNeighbour, note, nextNeighbour);

            string[] prevLyric = new string[] {
                    (string)lyrics[0],
                    (string)lyrics[1],
                    (string)lyrics[2],
            };
            string[] thisLyric = new string[] {
                    (string)lyrics[3],
                    (string)lyrics[4],
                    (string)lyrics[5],
            };
            string[] nextLyric = new string[] {
                    (string)lyrics[6],
                    (string)lyrics[7],
                    (string)lyrics[8],
            };
            return CovertForCVVCVVC(notes, prevLyric, thisLyric, nextLyric, prevNeighbour, nextNeighbour);
            /*

            if (thisLyric[0] == "null")
            {
                return GenerateResult(FindInOto(note.lyric, note));
            }
            else
            {
                // 함수 작성이 완료되면 교체 바람
                return CovertForCVVCVVC(notes, prevLyric, thisLyric, nextLyric, nextNeighbour);
            }*/
        }

        /// <summary>
        /// 주어진 음표들의 끝 소리를 생성합니다.
        /// </summary>
        /// <param name="notes">생성할 노트들의 배열입니다.</param>
        /// <param name="prev">이전 노트입니다.</param>
        /// <param name="next">다음 노트입니다.</param>
        /// <param name="prevNeighbour">이전 인접 노트입니다.</param>
        /// <param name="nextNeighbour">다음 인접 노트입니다.</param>
        /// <param name="prevNeighbours">이전 인접 음표들의 배열입니다.</param>
        /// <returns>생성된 음소 결과입니다.</returns>
        public override Result GenerateEndSound(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours)
        {
            Note note = notes[0];
            if (prevNeighbour == null) {
                return GenerateResult(FindInOto(note.lyric, note));
            }

            Hashtable lyrics = KoreanPhonemizerUtil.Separate(((Note)prevNeighbour).lyric);
            string[] prevLyric = new string[] {
                    (string)lyrics[0],
                    (string)lyrics[1],
                    (string)lyrics[2],
            };
            // 함수 작성이 완료되면 교체 바람
            return GenerateResult(FindInOto(note.lyric, note));
        }

        /// <summary>
        /// CVVC-VVC 형식에 맞는 음소로 변환합니다.
        /// </summary>
        /// <param name="notes">변환할 노트들의 배열입니다.</param>
        /// <param name="prevLyric">이전 노트의 자모입니다.</param>
        /// <param name="thisLyric"> 현재 노트의 자모입니다.<</param>
        /// <param name="nextLyric">다음 노트의 자모입니다.<</param>
        /// <param name="nextNeighbour">다음 인접 노트입니다.</param>
        /// <returns>변환된 음소 결과입니다.</returns>
        private Result CovertForCVVCVVC(Note[] notes, string[] prevLyric, string[] thisLyric, string[] nextLyric, Note? prevNeighbour, Note? nextNeighbour) 
        {
            string onset, nucleus, coda;

            var (phonemes, isR) = ConvertPhonemes(thisLyric);
            var (prevPhonemes, isPrevR) = ConvertPhonemes(prevLyric);
            var (nextPhonemes, isNextR) = ConvertPhonemes(nextLyric);

            /*
             * CV 구성 단계
             * 
             * 1. - CV
             * 공백 다음의 자음은 자음 앞에 -가 붙는다.
             * 예시) 가, 나 -> R [- ga] R [- na]
             * #R = 공백
             * 
             * 2. CV
             * 자음 + 자음 또는 받침 + 자음은 뒤 자음 앞에 -를 붙이지 않는다.
             * 예시) 가나 -> [- ga][a n][na], 라자 -> [- ra][a j][ja]
             */

            CVUnit cv = new CVUnit(phonemes[0], phonemes[1]);
            if (isPrevR) {
                cv.ChangePrefix("-");
            }

            /*
             * 3. VC
             * 자음과 자음 사이엔 VC 노트가 추가된다.
             * 예시) 가다 -> [- ga][a d][da] , 사지 -> [- sa][a j][ji]
             * 
             * 5. VV
             * 자음과 모음 또는 모음과 모음 또는 받침과 모음은 VCV노트로 할당한다.
             * 예시) 가이 - > [- ga][a i], 어우 -> [- eo][eo u], 안아 -> [- a][a n][n a]
             */
            PhoneticUnit? vc;
            if (phonemes[2] != "null") {
                vc = new VCUnit(phonemes[1], phonemes[2]);
            } else if (nextPhonemes[0] != "null") {
                vc = new VCUnit(phonemes[1], nextPhonemes[0]);
            } else if (!isNextR) {
                vc = new VVUnit(phonemes[1], nextPhonemes[1]);
            } else {
                vc = null;
            }

            /*
             * 3-2 받침이 kcl(ㄱ), tcl(ㄷ), pcl(ㅂ)의 경우 뒤의 자음 노트는 -가 붙는다.
             * 예시) 갓나 -> [- ga][a tcl][- na]
             * 
             * 3-3
             * 모음 뒤에 si,ji,ssi 발음이 올 경우 VC는 V C가 아닌 V Cy로 표기한다. #C = 자음
             * 예시) 아시 -> [- a][a sy][si], 익싸 -> [- i][ikcl ssy][ssi]
             */
            if (vc != null) {
                return GenerateResult(
                    FindInOto(cv.ToString(), notes[0]),
                    FindInOto(vc.ToString(), notes[0]),
                    notes.Sum(n => n.duration)
                );
            } else {
                return GenerateResult(FindInOto(cv.ToString(), notes[0]));
            }
            
        }

        /// <summary>
        /// 문자열 배열에서 CVUnit과 VCUnit으로 변환합니다.
        /// </summary>
        /// <param name="lyrics">자모가 나누어져 있는 가사</param>
        /// <returns>CVUnit과 VCUnit을 반환합니다.</returns>
        private (string[], bool) ConvertPhonemes(string[] lyrics) {
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

            return (new string[] { onset, nucleus, coda }, onset == "null" && nucleus == "null" && coda == "null");
        }
        
    }
}
