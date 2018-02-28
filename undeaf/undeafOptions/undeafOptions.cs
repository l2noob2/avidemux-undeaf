using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace undeaf
{
    public class undeafOptions
    {
        public undeafOptionsHelp Help;
        public undeafOptionsLanguage Language;
        public undeafOptionsSubtitles Subtitles;
        public undeafOptionsInput Input;
        public undeafOptionsOutput Output;

        public undeafOptions()
        {
            Init();
        }

        public undeafOptions(string[] args, undeafLogger logger)
        {
            Init();
            ParseOptions(args, logger);
        }

        public void PrintOptions(undeafLogger.MessageType messageType, undeafLogger logger)
        {
            Language.Print(messageType, logger);
            Subtitles.Print(messageType, logger);
            Input.Print(messageType, logger);
            Output.Print(messageType, logger);
        }

        public void PrintWarnings(undeafLogger.MessageType messageType, undeafLogger logger)
        {
            if (string.IsNullOrWhiteSpace(Output.OverrideOutputDirectory))
            {
                logger.Log(messageType, "No output directory specified, will attempt to use SRT path");
            }
            if (string.IsNullOrWhiteSpace(Input.OverrideSrtFile))
            {
                logger.Log(messageType, "No SRT file specified, will attempt to resolve from program map");
            }
        }

        public bool PrintErrors(undeafLogger.MessageType messageType, undeafLogger logger)
        {
            bool error = false;

            if (!string.IsNullOrWhiteSpace(Input.AvidemuxPy))
            {
                if (!System.IO.File.Exists(Input.AvidemuxPy))
                {
                    logger.Log(messageType, "Program map: File not found (" + Input.AvidemuxPy + ")");
                    error = true;
                }
            } else
            {
                logger.Log(messageType, "Program map: Input file not specified.");
                error = true;
            }
            return error;
        }

        public bool ValidateOptions(undeafLogger.MessageType warn, undeafLogger.MessageType error, undeafLogger logger)
        {
            PrintWarnings(warn, logger);
            return !PrintErrors(error, logger);
        }

        public void ParseOptions(string[] args, undeafLogger logger)
        {
            logger.Log(logger.GetByName("DEBUG"), "Parsing command line options:");

            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i].PadRight(20).Substring(0, 20);
                switch (a.TrimEnd(' '))
                {
                    case "-help":
                        Help.DisplayHelp = true;                
                        break;
                    case "-h":
                        Help.DisplayHelp = true;
                        break;
                    case "-?":
                        Help.DisplayHelp = true;
                        break;
                    case "/?":
                        Help.DisplayHelp = true;
                        break;

                    //Language
                    case "-lang:chpfmt":
                        if (args.Length - 1 >= i + 1)
                        {
                            Language.ChapterNameFormat = args[i + 1];
                            logger.Log(logger.GetByName("DEBUG"), "   - Chapter format: " + Language.ChapterNameFormat);
                        }
                        break;
                    case "-lang:lang":
                        if (args.Length - 1 >= i + 1)
                        {
                            Language.UseLanguage = args[i + 1];
                            logger.Log(logger.GetByName("DEBUG"), "   - Setting language: " + Language.UseLanguage);
                        }
                        break;
                    
                    //Subtitles
                    case "-subs:sentcase":
                        Subtitles.SentenceCase = true;
                        logger.Log(logger.GetByName("DEBUG"), "   - Converting subtitles to sentence case");
                        break;
                    case "-subs:rmbrkt":
                        Subtitles.RemoveBracketedText = true;
                        logger.Log(logger.GetByName("DEBUG"), "   - Removing bracketed text");
                        break;
                    case "-subs:rmlf":
                        Subtitles.RemoveEmptyLines = true;
                        logger.Log(logger.GetByName("DEBUG"), "   - Removing empty lines");
                        break;
                    case "-subs:swpsym":
                        Subtitles.ReplaceUnknownWithNote = true;
                        logger.Log(logger.GetByName("DEBUG"), "   - Replacing unknown with eighth note symbol");
                        break;
                    case "-subs:rmsym":
                        Subtitles.RemoveUnicodeSymbols = true;
                        logger.Log(logger.GetByName("DEBUG"), "   - Removing unicode symbols");
                        break;
                    case "-subs:default":
                        logger.Log(logger.GetByName("DEBUG"), "   - Using default subtitle options:");
                        Subtitles.RemoveBracketedText = true;
                        logger.Log(logger.GetByName("DEBUG"), "      - Removing empty lines");
                        Subtitles.RemoveUnicodeSymbols = true;
                        logger.Log(logger.GetByName("DEBUG"), "      - Replacing unknown with eighth note symbol");
                        break;
                    case "-subs:delay":
                        try
                        {
                            if (args.Length - 1 <= i - 1)
                            {
                                int t = int.Parse(args[i + 1]);
                                Subtitles.Delay = t;
                                logger.Log(logger.GetByName("DEBUG"), "   - Delaying subtitles by " + Subtitles.Delay + "ms");
                            }
                        }
                        catch
                        {
                            Subtitles.Delay = 0;
                        }
                        break;

                    //Input
                    case "-in:py":
                        if (args.Length - 1 >= i + 1)
                        {
                            Input.AvidemuxPy = args[i + 1];
                            logger.Log(logger.GetByName("DEBUG"), "   - Using input Avidemux save file: " + Input.AvidemuxPy);
                        }
                        break;
                    case "-in:srt":
                        if (args.Length - 1 >= i + 1)
                        {
                            Input.OverrideSrtFile = args[i + 1];
                            logger.Log(logger.GetByName("DEBUG"), "   - Overriding input SRT file: " + Input.OverrideSrtFile);
                        }
                        break;

                    //Output
                    case "-out:dir":
                        if (args.Length - 1 >= i + 1)
                        {
                            Output.OverrideOutputDirectory = args[i + 1];
                            logger.Log(logger.GetByName("DEBUG"), "   - Output directoy: " + Output.OverrideOutputDirectory);
                        }
                        break;
                    case "-out:dump":
                        Output.AlsoExportAllUnmodified = true;
                        logger.Log(logger.GetByName("DEBUG"), "   - Dumping unmodified subtitles");
                        break;
                    case "-out:dumpname":
                        if (args.Length - 1 >= i + 1)
                        {
                            Output.AlsoExportAllUnmodifiedName = args[i + 1];
                            logger.Log(logger.GetByName("DEBUG"), "   - Dumping unmodified subtitles as: " + Output.AlsoExportAllUnmodifiedName);
                        }
                        break;
                    case "-out:genchp":
                        Output.GenerateChapterFile = true;
                        logger.Log(logger.GetByName("DEBUG"), "   - Generating chapter file");
                        break;
                }
            }
        }

        private void Init()
        {
            Help = new undeafOptionsHelp();
            Language = new undeafOptionsLanguage();
            Subtitles = new undeafOptionsSubtitles();
            Input = new undeafOptionsInput();
            Output = new undeafOptionsOutput();
        }

        public class undeafOptionsHelp
        {
            public bool DisplayHelp = false;

            public void PrintHelp(undeafLogger.MessageType messageType, undeafLogger logger)
            {
                logger.Log(messageType, "General:  -help               Displays this help.");
                logger.Log(messageType, "Language: -lang:lang <str>    Sets the language, default \"en\"");
                logger.Log(messageType, "          -lang:chpfmt <str>  Displays this help, default \"Chapter {0:0}\"");
                logger.Log(messageType, "Subtitle: -subs:sentcase      Forces sentence case to all incoming subs");
                logger.Log(messageType, "          -subs:rmbrkt        Remove all content between brackets");
                logger.Log(messageType, "          -subs:rmlf          Remove empty lines");
                logger.Log(messageType, "          -subs:swpsym        Swaps unknown symbols with note symbol");
                logger.Log(messageType, "          -subs:rmsym         Removes all unknown unicode symbols");
                logger.Log(messageType, "          -subs:default       Equivelent to specifying: -rmlf -swpsym");
                logger.Log(messageType, "          -subs:delay <num>   Delays incoming subs by, in ms, default 0");
                logger.Log(messageType, "Input:    -in:py <str>        Input program map as an Avidemux save file");
                logger.Log(messageType, "          -in:srt <str>       Input SRT to use");
                logger.Log(messageType, "Output :  -out:dir <str>      Directory for output files");
                logger.Log(messageType, "          -out:dump           Save an extra copy of unmodified subs");
                logger.Log(messageType, "          -out:dumpname <str> Unmodified subs filename suffix, default \"-sdh\"");
                logger.Log(messageType, "          -out:genchp         Generate chapter file based on program map");
            }
        }

        public class undeafOptionsLanguage
        {
            public string ChapterNameFormat = "Chapter {0:0}";
            public string UseLanguage = "eng";

            public void Print(undeafLogger.MessageType messageType, undeafLogger logger)
            {
                string s = "Language Options:[";
                s += "ChapterNameFormat:(" + ChapterNameFormat + ") ";
                s += "UseLanguage:(" + UseLanguage + ") ";
                s += "]";
                logger.Log(messageType, s);
            }
        }

        public class undeafOptionsSubtitles
        {
            public bool SentenceCase = false;
            public bool ReplaceUnknownWithNote = false;
            public bool RemoveBracketedText = false;
            public bool RemoveEmptyLines = false;
            public bool RemoveUnicodeSymbols = false;
            public int Delay = 0;

            public void Print(undeafLogger.MessageType messageType, undeafLogger logger)
            {
                string s = "SubtitleOptions:[";
                s += "SentenceCase:(" + SentenceCase.ToString() + ") ";
                s += "ReplaceUnknownUnicode:(" + ReplaceUnknownWithNote.ToString() + ") ";
                s += "RemoveBracketedText:(" + RemoveBracketedText.ToString() + ") ";
                s += "RemoveEmptyLines:(" + RemoveEmptyLines.ToString() + ") ";
                s += "RemoveUnicodeSymbols:(" + RemoveUnicodeSymbols.ToString() + ") ";
                s += "Delay:(" + Delay + "ms) ";
                s += "]";
                logger.Log(messageType, s);
            }
        }

        public class undeafOptionsInput
        {
            public string AvidemuxPy = string.Empty;
            public string OverrideSrtFile = string.Empty;

            public void Print(undeafLogger.MessageType messageType, undeafLogger logger)
            {
                string s = "InputOptions:[";
                s += "AvidemuxPy:(" + AvidemuxPy + ") ";
                s += "OverrideSrtFile:(" + OverrideSrtFile + ") ";
                s += "]";
                logger.Log(messageType, s);
            }
        }

        public class undeafOptionsOutput
        {
            public string OverrideOutputDirectory = string.Empty;
            public bool AlsoExportAllUnmodified = false;
            public string AlsoExportAllUnmodifiedName = "-sdh";
            public bool GenerateChapterFile = false;

            public void Print(undeafLogger.MessageType messageType, undeafLogger logger)
            {
                string s = "OutputOptions:[";
                s += "OverrideOutputDirectory:(" + OverrideOutputDirectory + ") ";
                s += "AlsoExportAllUnmodified:(" + AlsoExportAllUnmodified.ToString() + ") ";
                s += "AlsoExportAllUnmodifiedName:(" + AlsoExportAllUnmodifiedName + ") ";
                s += "GenerateChapterFile:(" + GenerateChapterFile.ToString() + ") ";
                s += "]";
                logger.Log(messageType, s);
            }
        }
    }
}
