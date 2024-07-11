using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using OpenUtau.Api;
using OpenUtau.Core.Ustx;
using static OpenUtau.Api.Phonemizer;

namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("Korean CVVC-VVC Phonemizer", "KO CVVC-VVC", "NoelKM", language: "KO")]
    public class KoreanCVVCVVCPhonemizer: Phonemizer {
        private USinger singer;

        /*
        private static readonly string[] ChoSung = new string[]
        {
            "ㄱ", "ㄲ", "ㄴ", "ㄷ", "ㄸ", "ㄹ", "ㅁ", "ㅂ", "ㅃ", "ㅅ", "ㅆ",
            "ㅇ", "ㅈ", "ㅉ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ"
        };

        private static readonly string[] JungSung = new string[]
        {
            "ㅏ", "ㅐ", "ㅑ", "ㅒ", "ㅓ", "ㅔ", "ㅕ", "ㅖ", "ㅗ", "ㅘ", "ㅙ",
            "ㅚ", "ㅛ", "ㅜ", "ㅝ", "ㅞ", "ㅟ", "ㅠ", "ㅡ", "ㅢ", "ㅣ"
        };


        private static readonly string[] JongSung = new string[]
        {
            "", "ㄱ", "ㄲ", "ㄳ", "ㄴ", "ㄵ", "ㄶ", "ㄷ", "ㄹ", "ㄺ", "ㄻ",
            "ㄼ", "ㄽ", "ㄾ", "ㄿ", "ㅀ", "ㅁ", "ㅂ", "ㅄ", "ㅅ", "ㅆ", "ㅇ",
            "ㅈ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ"
        };
        */

        private static readonly string[] onsets = new string[]
        {
            "g", "kk", "n", "d", "tt", "r", "m", "b", "pp", "s", "ss",
            "", "j", "jj", "ch", "k", "t", "p", "h"
        };
        private static readonly string[] vowels = new string[]
        {
            "a", "e", "ya", "ye", "eo", "e", "yeo", "ye", "o", "wa", "we",
            "we", "yo", "u", "weo", "we", "wi", "yu", "eu", "eui", "i"
        };
        private static readonly string[] codas = new string[]
        {
            "", "kcl", "kcl", "kcl", "n", "n", "n", "tcl", "l", "l", "l",
            "l", "l", "l", "l", "l", "m", "pcl", "pcl", "tcl", "tcl", "ng",
            "tcl", "tcl", "kcl", "tcl", "pcl", "tcl"
        };

        const int BASECODE = 0xAC00;

        private int[] Vectorize(char character) {
            int code = character - BASECODE;

            int ons = code / (21 * 28);
            int vow = (code % (21 * 28)) / 28;
            int cod = code % 28;

            return new int[] { ons, vow, cod };
        }
        
        public KoreanCVVCVVCPhonemizer() {

        }

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevs) {
            var note = notes[0];
            var unicode = ToUnicodeElements(note.lyric);
            char lyric = char.Parse(note.lyric);
            int[] vec = Vectorize(lyric);
            string phones = "";
            Phoneme[] phone_array = new Phoneme[vec.Length];
            phone_array.Append(new Phoneme{
                phoneme=onsets[vec[0]] 
            });
            phone_array.Append(new Phoneme {
                phoneme=vowels[vec[1]] 
            });
            phone_array.Append(new Phoneme {
                phoneme=codas[vec[2]] 
            });

            return new Result {
                phonemes = phone_array
            };
        }

        public override void SetSinger(USinger singer) {
            this.singer = singer;
        }
    }
}
