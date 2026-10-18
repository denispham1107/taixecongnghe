# Navigation voice

Bundled Vietnamese female navigation phrases, generated for Phố Việt with
Microsoft Edge text-to-speech voice `vi-VN-HoaiMyNeural` using edge-tts 7.3.1.
The game plays these local clips; it does not request speech or location services.

- `left.mp3`: Quẹo trái, quẹo trái.
- `right.mp3`: Quẹo phải, quẹo phải.
- `straight.mp3`: Đi thẳng, đi thẳng.

Rebuild with `python -m edge_tts --voice vi-VN-HoaiMyNeural --text "..." --write-media path.mp3`.
Voice catalogue: https://learn.microsoft.com/en-us/azure/cognitive-services/speech-service/language-support?tabs=tts.
