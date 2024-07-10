using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Core.Ustx;
using OpenUtau.Core;

namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("Korean CVVC-VVC Phonemizer", "KO CVVC-VVC", "NoelKM", language: "KO")]

    public class KoreanCVVCVVCPhonemizer: Phonemizer {
        public KoreanCVVCVVCPhonemizer() {

        }

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevs) {
            throw new NotImplementedException();
        }

        public override void SetSinger(USinger singer) {
            throw new NotImplementedException();
        }
    }
}
