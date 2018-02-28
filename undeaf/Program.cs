using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Humanizer;
using SubtitlesParser.Classes;

namespace undeaf
{
    class Program
    {
        const string cmd_adm_addSegment = "adm.addSegment";
        const string cmd_adm_loadVideo = "adm.loadVideo";

        static undeafOptions options = null;

        static DateTime appStart = DateTime.Now;
        static List<SubtitleItem> subsOriginal;
        static List<SubtitleItem> subsOutput = new List<SubtitleItem>();
        static List<SubtitleItem> subsModTrim = new List<SubtitleItem>();

        static undeafLogger logger = null;

        static void Main(string[] args)
        {
            undeafStatus status = new undeafStatus();
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " - " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            logger = new undeafLogger();

            logger.Log(logger.DefaultMessageType, appName + " started " + DateTime.Today.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());

            options = new undeafOptions(args, logger);
            if (options.Help.DisplayHelp)
            {
                options.Help.PrintHelp(logger.GetByName("HELP"), logger);
            }
            else
            {
                options.PrintOptions(logger.GetByName("DEBUG"), logger);
                if (!options.ValidateOptions(logger.GetByName("WARN"), logger.GetByName("ERROR"), logger))
                {
                    logger.Log(logger.GetByName("HELP"), "Use command line option -help for more information.");
                    status.HasError = true;
                }
                else
                {
                    readADMFile(options, status);
                }
                
            }
        }

        static void ParseOptions()
        {

        }

        static bool FileNameValid(string fileSpec)
        {
            return (System.IO.File.Exists(fileSpec));
        }



        static void PrintUsage()
        {
            logger.Log(logger.GetByName("INFO"), "Usage: undeaf <input_file> <offset>");
            logger.Log(logger.GetByName("INFO"), "     <input_file> AVIDemux save file, .py");
            logger.Log(logger.GetByName("INFO"), "     <offset>     Subtitle offset in ms");
        }

        //static void Log(string line)
        //{
        //    var dur = DateTime.Now - appStart;
        //    Console.WriteLine("[" + string.Format("0000", dur.Seconds) + "] " + line);
        //}

        static TimeSpan DFtoTimeSpan(string df)
        {
            string time = string.Format("{0:000000000000}", Int64.Parse(df));

            var u = int.Parse(time.Substring(0, time.Length - 3));
            //var m = int.Parse(time.Substring(time.Length - 10, 2));
            //var h = int.Parse(time.Substring(time.Length - 12, 2));

            return new TimeSpan(0, 0, 0, 0, u);
        }

        static int DFtoInt(string df)
        {
            return TimeSpanToInt(DFtoTimeSpan(df));
        }

        static TimeSpan IntToTimeSpan(int t)
        {
            return new TimeSpan(0, 0, 0, 0, t);
        }

        static int TimeSpanToInt(TimeSpan ts)
        {
            return Convert.ToInt32(ts.TotalMilliseconds);
        }

        static string TimeSpanToHR(TimeSpan ts)
        {
            return string.Format("{0:00}", ts.Hours) + ":" + string.Format("{0:00}", ts.Minutes) + ":" + string.Format("{0:00}", ts.Seconds) + "." + string.Format("{0:000}", ts.Milliseconds);
        }

        static string TimeSpanToSRT(TimeSpan ts)
        {
            return string.Format("{0:00}", ts.Hours) + ":" + string.Format("{0:00}", ts.Minutes) + ":" + string.Format("{0:00}", ts.Seconds) + "," + string.Format("{0:000}", ts.Milliseconds);
        }

        static string IntToSRT(int i)
        {
            return TimeSpanToSRT(IntToTimeSpan(i));
        }

        static void readSubFile(string fileSpec)
        {
            var parser = new SubtitlesParser.Classes.Parsers.SrtParser();
            using (var fileStream = File.OpenRead(fileSpec))
            {
                subsOriginal = parser.ParseStream(fileStream, System.Text.Encoding.UTF8);
            }
        }

        static void readADMFile(undeafOptions opts, undeafStatus status)
        {
            using (StreamReader reader = File.OpenText(opts.Input.AvidemuxPy))
            {
                //locals
                TimeSpan programLength = TimeSpan.Zero;

                string videoFileName = string.Empty;
                string videoPath = string.Empty;
                string pyPath = System.IO.Path.GetDirectoryName(opts.Input.AvidemuxPy);
                string srtInFileName = string.Empty;
                string chapterOutFileSpec = string.Empty;
                string srtOutFileSpec = string.Empty;
                string srtModFileSpec = string.Empty;

                StreamWriter outFileChapter = null;

                int numChapters = 0;
                int totalSubs = 0;

                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length > cmd_adm_loadVideo.Length)
                    {
                        if (line.Substring(0, cmd_adm_loadVideo.Length) == cmd_adm_loadVideo)
                        {
                            //locals
                            string stripped = line.Replace(cmd_adm_loadVideo, string.Empty);
                            stripped = stripped.Substring(2, stripped.Length - 4);
                            videoFileName = System.IO.Path.GetFileNameWithoutExtension(stripped);
                            videoPath = System.IO.Path.GetDirectoryName(stripped);

                            //locals (filenames)
                            srtInFileName = videoPath + "\\" + videoFileName + ".srt";
                            if (!string.IsNullOrWhiteSpace(opts.Input.OverrideSrtFile))
                            {
                                srtInFileName = opts.Input.OverrideSrtFile;
                            }

                            string outputDir = videoPath;
                            if (!string.IsNullOrWhiteSpace(opts.Output.OverrideOutputDirectory))
                            {
                                outputDir = opts.Output.OverrideOutputDirectory;
                            }
                            chapterOutFileSpec = outputDir + "\\" + videoFileName + ".chapters.txt";
                            srtOutFileSpec = outputDir + "\\" + videoFileName + "." + opts.Language.UseLanguage + opts.Output.AlsoExportAllUnmodifiedName + ".srt";
                            srtModFileSpec = outputDir + "\\" + videoFileName + "." + opts.Language.UseLanguage + ".srt";

                            //io
                            if (opts.Output.GenerateChapterFile)
                            {
                                if (System.IO.File.Exists(chapterOutFileSpec))
                                {
                                    System.IO.File.Delete(chapterOutFileSpec);
                                }
                                outFileChapter = new StreamWriter(chapterOutFileSpec, true, System.Text.Encoding.UTF8);
                            }
                            readSubFile(srtInFileName);
                        }
                    }

                    //find adm.addSegment
                    if (line.Length > cmd_adm_addSegment.Length)
                    {
                        if (line.Substring(0, cmd_adm_addSegment.Length) == cmd_adm_addSegment)
                        {
                            string stripped = line.Replace(cmd_adm_addSegment, string.Empty);
                            stripped = stripped.Replace("(", "");
                            stripped = stripped.Replace(")", "");
                            stripped = stripped.Replace(" ", "");

                            string[] segParams = stripped.Split(',');
                            TimeSpan startTime = TimeSpan.Zero;
                            TimeSpan lengthTime = TimeSpan.Zero;
                            TimeSpan endTime = TimeSpan.Zero;
                            int startTimeInt = 0;
                            int endTimeInt = 0;
                            int numSubs = 0;

                            try
                            {
                                //locals
                                startTime = DFtoTimeSpan(segParams[1]);
                                lengthTime = DFtoTimeSpan(segParams[2]);
                                endTime = startTime + lengthTime;
                                startTimeInt = TimeSpanToInt(startTime);
                                endTimeInt = TimeSpanToInt(endTime);

                                numSubs = 0;
                                numChapters++;

                                //update chapter file
                                if (opts.Output.GenerateChapterFile)
                                {
                                    outFileChapter.WriteLine("CHAPTER" + string.Format("{0:00}", numChapters) + "=" + TimeSpanToHR(programLength));
                                    outFileChapter.WriteLine("CHAPTER" + string.Format("{0:00}", numChapters) + "NAME=" + string.Format(opts.Language.ChapterNameFormat, numChapters));
                                }

                                //recode subtitles
                                foreach (SubtitleItem sub in subsOriginal)
                                {
                                    if ((sub.StartTime >= startTimeInt + opts.Subtitles.Delay) && (sub.EndTime <= endTimeInt + opts.Subtitles.Delay))
                                    {
                                        //1:1 copy
                                        SubtitleItem subNew = new SubtitleItem();
                                        TimeSpan offset = -startTime + programLength;
                                        TimeSpan newStart = IntToTimeSpan(sub.StartTime) + offset;
                                        TimeSpan newEnd = IntToTimeSpan(sub.EndTime) + offset;
                                        subNew.StartTime = TimeSpanToInt(newStart) + opts.Subtitles.Delay;
                                        subNew.EndTime = TimeSpanToInt(newEnd) + opts.Subtitles.Delay;
                                        foreach (string dataLine in sub.Lines)
                                        {
                                            string s = dataLine;
                                            if (opts.Subtitles.ReplaceUnknownWithNote) { s = s.Replace("�", "♪"); }
                                            subNew.Lines.Add(s);
                                        }

                                        subsOutput.Add(subNew);
                                        numSubs++;

                                        string joinedLines = string.Join(" ", sub.Lines.ToArray());
                                        if (opts.Subtitles.RemoveUnicodeSymbols) { joinedLines = joinedLines.Replace("�", ""); }
                                        if (opts.Subtitles.ReplaceUnknownWithNote) { joinedLines = joinedLines.Replace("�", "♪"); }
                                        if (opts.Subtitles.RemoveBracketedText) { joinedLines = Regex.Replace(joinedLines, "(\\[.*\\])", string.Empty); }
                                        joinedLines = joinedLines.Trim(' ');


                                        //modified copy
                                        List<string> finalLines = new List<string>();
                                        foreach (string dataLine in sub.Lines)
                                        {
                                            string s = dataLine;
                                            if (opts.Subtitles.RemoveUnicodeSymbols) { s = s.Replace("�", ""); }
                                            if (opts.Subtitles.ReplaceUnknownWithNote) { s = s.Replace("�", "♪"); }
                                            if (opts.Subtitles.RemoveBracketedText) { s = Regex.Replace(s, "(\\[.*\\])", string.Empty); }

                                            s = s.Trim(' ');
                                            if (opts.Subtitles.SentenceCase)
                                            {
                                                string pre = string.Empty;
                                                if (s.Contains(":"))
                                                {

                                                    pre = s.Substring(0, s.IndexOf(":") + 1);
                                                    s = s.Substring(s.IndexOf(":") + 1);
                                                    if (s.Length > 0)
                                                    {
                                                        if (s.Substring(0, 1) == " ")
                                                        {
                                                            pre += " ";
                                                            s = s.Substring(1);
                                                        }
                                                    }
                                                }
                                                s = s.Transform(To.LowerCase);
                                                s = s.Transform(To.SentenceCase);
                                                s = pre + s;
                                            }


                                            string st = Regex.Replace(s, "(\\<.*\\>)", string.Empty);
                                            st = Regex.Replace(st, "[^a-zA-Z0-9]", string.Empty);

                                            if (opts.Subtitles.RemoveEmptyLines)
                                            {
                                                if (!string.IsNullOrWhiteSpace(st))
                                                {
                                                    finalLines.Add(s);
                                                }
                                            } else
                                            {
                                                finalLines.Add(s);
                                            }
                                        }
                                        if (finalLines.Count > 0)
                                        {
                                            SubtitleItem subMod = new SubtitleItem();
                                            offset = -startTime + programLength;
                                            newStart = IntToTimeSpan(sub.StartTime) + offset;
                                            newEnd = IntToTimeSpan(sub.EndTime) + offset;
                                            subMod.StartTime = TimeSpanToInt(newStart) + opts.Subtitles.Delay;
                                            subMod.EndTime = TimeSpanToInt(newEnd) + opts.Subtitles.Delay;
                                            subMod.Lines = finalLines;
                                            
                                            subsModTrim.Add(subMod);
                                        }
                                    }
                                }

                                //log
                                logger.Log(logger.GetByName("INFO"), "Moved " + string.Format("{0:000}", numSubs) + " subs from " + TimeSpanToHR(startTime) + " -> " + TimeSpanToHR(endTime) + " to " + TimeSpanToHR(programLength) + " -> " + TimeSpanToHR(programLength + lengthTime));
                            }
                            catch (Exception e)
                            {
                                status.HasError = true;
                                logger.Log(logger.GetByName("DEBUG"), e.ToString());
                            }
                            //locals
                            totalSubs += numSubs;
                            programLength += (endTime - startTime);

                        }
                    }
                }

                //io
                if (opts.Output.GenerateChapterFile) { outFileChapter.Close(); }

                if (System.IO.File.Exists(srtOutFileSpec))
                {
                    System.IO.File.Delete(srtOutFileSpec);
                }
                if (System.IO.File.Exists(srtModFileSpec))
                {
                    System.IO.File.Delete(srtModFileSpec);
                }

                //log
                logger.Log(logger.GetByName("INFO"), string.Format("{0:0} subs, {1:0} chapters, ", totalSubs, numChapters) + TimeSpanToHR(programLength) + " program length");

                //sort
                subsOutput.Sort(delegate (SubtitleItem s1, SubtitleItem s2)
                {
                    return s1.StartTime.CompareTo(s2.StartTime);
                });
                subsModTrim.Sort(delegate (SubtitleItem s1, SubtitleItem s2)
                {
                    return s1.StartTime.CompareTo(s2.StartTime);
                });


                //write subtitles
                int subCount = 0;
                var outFile = new StreamWriter(srtModFileSpec, true, System.Text.Encoding.UTF8);

                foreach (SubtitleItem i in subsModTrim)
                {
                    subCount++;
                    outFile.WriteLine(subCount.ToString());
                    outFile.WriteLine(IntToSRT(i.StartTime) + " --> " + IntToSRT(i.EndTime));
                    foreach (string l in i.Lines)
                    {
                        outFile.WriteLine(l);
                    }
                    outFile.WriteLine();
                }
                outFile.Close();

                if (opts.Output.AlsoExportAllUnmodified)
                {
                    subCount = 0;
                    outFile = new StreamWriter(srtOutFileSpec, true, System.Text.Encoding.UTF8);
                    foreach (SubtitleItem i in subsOutput)
                    {
                        subCount++;
                        outFile.WriteLine(subCount.ToString());
                        outFile.WriteLine(IntToSRT(i.StartTime) + " --> " + IntToSRT(i.EndTime));
                        foreach (string l in i.Lines)
                        {
                            outFile.WriteLine(l);
                        }
                        outFile.WriteLine();
                    }
                    outFile.Close();
                }
            }
        }
    }
}
