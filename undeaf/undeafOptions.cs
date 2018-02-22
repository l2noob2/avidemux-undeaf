using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace undeaf
{
    public class undeafOptions
    {
        public undeafOptionsLanguage Language;
        public undeafOptionsSubtitles Subtitles;
        public undeafOptionsInput Input;
        public undeafOptionsOutput Output;

        public undeafOptions()
        {
            Init();
        }

        public undeafOptions(string[] args)
        {
            Init();

            for (int i = 0; i < args.Length - 1; i++)
            {
                string a = args[i].PadRight(4).Substring(0, 4);
                switch (a)
                {
                    //Language
                    case "-nf ":
                        if (args.Length - 1 >= i + 1) { Language.ChapterNameFormat = args[i + 1]; }
                        break;
                    case "-ul ":
                        if (args.Length - 1 >= i + 1) { Language.UseLanguage = args[i + 1]; }
                        break;
                    
                    //Subtitles
                    case "-sc ":
                        Subtitles.SentenceCase = true;
                        break;
                    case "-rb ":
                        Subtitles.RemoveBracketedText = true;
                        break;
                    case "-re ":
                        Subtitles.RemoveEmptyLines = true;
                        break;
                    case "-ru ":
                        Subtitles.ReplaceUnknownWithNote = true;
                        break;
                    case "-rs ":
                        Subtitles.RemoveUnicodeSymbols = true;
                        break;
                    case "-rd ":
                        Subtitles.RemoveBracketedText = true;
                        Subtitles.RemoveEmptyLines = true;
                        Subtitles.RemoveUnicodeSymbols = true;
                        Subtitles.ReplaceUnknownWithNote = true;
                        break;
                    case "-d  ":
                        try
                        {
                            if (args.Length - 1 <= i - 1)
                            {
                                int t = int.Parse(args[i + 1]);
                                Subtitles.Delay = t;
                            }
                        }
                        catch
                        {
                            Subtitles.Delay = 0;
                        }
                        break;

                    //Input
                    case "-i  ":
                        if (args.Length - 1 >= i + 1) { Input.AvidemuxPy = args[i + 1]; }
                        break;
                    case "-s  ":
                        if (args.Length - 1 >= i + 1) { Input.OverrideSrtFile = args[i + 1]; }
                        break;

                    //Output
                    case "-od ":
                        if (args.Length - 1 >= i + 1) { Output.OverrideOutputDirectory = args[i + 1]; }
                        break;
                    case "-a  ":
                        Output.AlsoExportAllUnmodified = true;
                        break;
                    case "-an ":
                        if (args.Length - 1 >= i + 1) { Output.AlsoExportAllUnmodifiedName = args[i + 1]; }
                        break;
                    case "-c  ":
                        Output.GenerateChapterFile = true;
                        break;
                }
            }
        }

        private void Init()
        {
            Language = new undeafOptionsLanguage();
            Subtitles = new undeafOptionsSubtitles();
            Input = new undeafOptionsInput();
            Output = new undeafOptionsOutput();
        }

        public class undeafOptionsLanguage
        {
            public string ChapterNameFormat = "Chapter {0:0}";
            public string UseLanguage = "eng";
        }

        public class undeafOptionsSubtitles
        {
            public bool SentenceCase = false;
            public bool ReplaceUnknownWithNote = false;
            public bool RemoveBracketedText = false;
            public bool RemoveEmptyLines = false;
            public bool RemoveUnicodeSymbols = false;
            public int Delay = 0;
        }

        public class undeafOptionsInput
        {
            public string AvidemuxPy = string.Empty;
            public string OverrideSrtFile = string.Empty;
        }

        public class undeafOptionsOutput
        {
            public string OverrideOutputDirectory = string.Empty;
            public bool AlsoExportAllUnmodified = false;
            public string AlsoExportAllUnmodifiedName = "-sdh";
            public bool GenerateChapterFile = false;
        }
    }
}
