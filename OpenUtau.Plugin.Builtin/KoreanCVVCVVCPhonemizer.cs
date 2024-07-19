using System;
using System.Collections;
using System.Collections.Generic;
using OpenUtau.Api;
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
            { "ㅇ", "null" }
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
            { "ㅇ", "y" }
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
            { "ㅇ", "w" }
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
        };

        private readonly Dictionary<string, string> y_nucleus_symbol = new Dictionary<string, string>
        {
            { "ㅑ", "a" },
            { "ㅕ", "eo" },
            { "ㅒ", "e" },
            { "ㅖ", "e" },
            { "ㅛ", "o" },
            { "ㅠ", "u" },
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
            { " ", "null"}
        };



        private USinger singer;

        /// <summary>
        /// 가수를 설정합니다.
        /// </summary>
        /// <param name="singer">설정할 가수입니다.</param>
        public override void SetSinger(USinger singer)
        {
            if (this.singer == singer || singer == null || singer.SingerType != USingerType.Classic)
            {
                return;
            }

            this.singer = singer;
        }

        private string? FindInOto(String phoneme, Note note, bool nullIfNotFound = false)
        {
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
            string[] thisLyric = new string[]
            {
                    (string)lyrics[3],
                    (string)lyrics[4],
                    (string)lyrics[5],
            };
            string[] nextLyric = new string[]
            {
                    (string)lyrics[6],
                    (string)lyrics[7],
                    (string)lyrics[8],
            };

            if (thisLyric[0] == "null")
            {
                return GenerateResult(FindInOto(note.lyric, note));
            }
            else
            {
                // 함수 작성이 완료되면 교체 바람
                return CovertForCVVCVVC(notes, prevLyric, thisLyric, nextLyric, nextNeighbour);
            }
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
            if (prevNeighbour == null)
            {
                return GenerateResult(FindInOto(note.lyric, note));
            }

            Hashtable lyrics = KoreanPhonemizerUtil.Separate(((Note)prevNeighbour).lyric);
            string[] prevLyric = new string[]
            {
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
        private Result CovertForCVVCVVC(Note[] notes, string[] prevLyric, string[] thisLyric, string[] nextLyric, Note? nextNeighbour) 
        {
            string onset, nucleus, coda;

            // 0. g2p 변환
            if (y_nucleus_symbol.ContainsKey(thisLyric[1]))
            {
                onset = y_onset_symbol[thisLyric[0]];
                nucleus = y_nucleus_symbol[thisLyric[1]];
            } 
            else if (w_nucleus_symbol.ContainsKey(thisLyric[1]))
            {
                onset = w_onset_symbol[thisLyric[0]];
                nucleus = w_nucleus_symbol[thisLyric[1]];
                
            }
            else if (thisLyric[1] == "ㅢ") // 추후 예외처리를 위한 코드
            { 
                onset = w_onset_symbol[thisLyric[0]];
                nucleus = w_nucleus_symbol[thisLyric[1]];
            }
            else 
            {
                onset = onset_symbol[thisLyric[0]];
                nucleus = nucleus_symbol[thisLyric[1]];
            }
            coda = coda_symbol[thisLyric[2]];

            // 좀 더 깔끔한 표현 필요 할 듯
            string phone = "";
            if (onset != "null") {
                phone += $"{onset} ";
            } 
            phone += $"{nucleus}";
            if (coda != "null") {
                phone += $" {coda}";
            }

            return GenerateResult(FindInOto(phone, notes[0]));
        }

        
    }
}
