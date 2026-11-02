using System;
using System.Collections.Generic;
using System.Text;
using System.Speech.Synthesis;

namespace Neuro_Link.Engine
{
    private SpeechSynthesizer _synth;

    public VoiceManager()
    {
        _synth = new SpeechSynthesizer();
        _synth.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult);
    }

    public void Speak(string character, string text)
    {
        string ssml;
        switch (character)
        {
            case "Newton":
                ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='ru-RU'><prosody rate='110%' pitch='+5%'>{text}</prosody></speak>";
                break;
            case "CORTEXIS":
                ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='ru-RU'><prosody rate='80%' pitch='-15%'>{text}</prosody></speak>";
                break;
            case "Eli":
                ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='ru-RU'><prosody rate='95%' pitch='+10%'>{text}</prosody></speak>";
                break;
            case "Rat":
                ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='ru-RU'><prosody rate='120%' pitch='-5%'>{text}</prosody></speak>";
                break;
            default:
                ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='ru-RU'><prosody rate='100%'>{text}</prosody></speak>";
                break;
        }
        _synth.SpeakAsyncCancelAll();
        _synth.SpeakSsmlAsync(ssml);
    }
}