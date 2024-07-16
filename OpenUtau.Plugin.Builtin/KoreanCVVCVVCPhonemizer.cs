using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using OpenUtau.Api;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;


namespace OpenUtau.Plugin.Builtin {
    /// <summary>
    /// 한국어 CVVC-VVC Phonemizer입니다.
    /// </summary>
    [Phonemizer("한국어 CVVC-VVC 음소화기", "KO CVVC-VVC", "NoelKM", language: "KO")]
    public class KoreanCVVCVVCPhonemizer : BaseKoreanPhonemizer
    {
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
                return GenerateResult(FindInOto(note.lyric, note));
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

        private Result CovertForCVVCVVC(Note[] notes, string[] prevLyric, string[] thisLyric, string[] nextLyric, Note? nextNeighbour) 
        {
            // 이곳에 메인 루틴을 추가하면 될 것 같습니다.
            // 1. g2p 변환
            // 2. 그 이후 정해진 룰 수행
            // below return is Dummy
            return new Result() {
                phonemes = new Phoneme[] {
                        new Phoneme { phoneme = $""},
                    }
            };
        }
    }
}
