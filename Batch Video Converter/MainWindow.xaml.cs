using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace Batch_Video_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Boolean KillHB = false;
        public double xpos;
        public double ypos;
        private string registryPath = @"Software\JasonBean\BVC";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {

            string TempForBool;
            System.OperatingSystem osInfo = System.Environment.OSVersion;

            xpos = Convert.ToDouble(Registry.CurrentUser.GetValue(registryPath + "XPos", 100));
            ypos = Convert.ToDouble(Registry.CurrentUser.GetValue(registryPath + "YPos", 100));

            Point location = new Point(xpos, ypos);

            this.Top = location.Y;
            this.Left = location.X;


            //try to get settings from registry
            Functions.HBPath = (string)Registry.CurrentUser.GetValue(registryPath + "HBPath", string.Empty);
            Functions.mp4boxPath = (string)Registry.CurrentUser.GetValue(registryPath + "mp4boxPath", string.Empty);
            Functions.APPath = (string)Registry.CurrentUser.GetValue(registryPath + "APPath", string.Empty);
            Functions.OutPath = (string)Registry.CurrentUser.GetValue(registryPath + "OutPath", string.Empty);
            TempForBool = (string)Registry.CurrentUser.GetValue(registryPath + "RemoveAds", "False");
            if (TempForBool == "True")
            {
                Functions.RemoveAds = true;
            }
            else
            {
                Functions.RemoveAds = false;
            }
            TempForBool = (string)Registry.CurrentUser.GetValue(registryPath + "AutoCrop", "False");
            if (TempForBool == "True")
            {
                Functions.AutoCrop = true;
            }
            else
            {
                Functions.AutoCrop = false;
            }
            TempForBool = (string)Registry.CurrentUser.GetValue(registryPath + "AutoMeta", "False");
            if (TempForBool == "True")
            {
                Functions.AutoTVMeta = true;
            }
            else
            {
                Functions.AutoTVMeta = false;
            }
            TempForBool = (string)Registry.CurrentUser.GetValue(registryPath + "AutoMoviePoster", "False");
            if (TempForBool == "True")
            {
                Functions.AutoMoviePoster = true;
            }
            else
            {
                Functions.AutoMoviePoster = false;
            }
            TempForBool = (string)Registry.CurrentUser.GetValue(registryPath + "AutoImportiTunes", "False");
            if (TempForBool == "True")
            {
                Functions.AutoImportiTunes = true;
            }
            else
            {
                Functions.AutoImportiTunes = false;
            }
            Functions.mp4boxPath = (string)Registry.CurrentUser.GetValue(registryPath + "mp4boxPath", string.Empty);
            TempForBool = (string)Registry.CurrentUser.GetValue(registryPath + "AutoDeleteExport", "False");
            if (TempForBool == "True")
            {
                Functions.AutoDeleteExport = true;
            }
            else
            {
                Functions.AutoDeleteExport = false;
            }
            Functions.DefaultEncodeProfile = (int)Registry.CurrentUser.GetValue(registryPath + "EncodeProfile", 0);

            //get the major and minor version numbers of the OS
            Functions.OsMajor = osInfo.Version.Major;
            Functions.OsMinor = osInfo.Version.Minor;

            //set temp folder
            Functions.TempDir = System.IO.Path.GetTempPath();
        }

        private async void btnEncode_Click(object sender, RoutedEventArgs e)
        {
            int VideoCount = 0;
            string EncodeTo = "";
            string CropVideo = "";
            string Outfile = null;
            string OutExt = null;
            string AddedOptions = null;
            string InFile = null;
            string InPath = null;
            string edlFile = null;
            string Args = "";
            string outtemp = null;
            string[] output = null;
            float percentage = 0;
            float AllProgress = 0;
            bool DeletePoster = false;
            bool HBFail = false;

            //If on Windows 7 ore greater set the taskbar progress state to normal
            if (Functions.OsMajor >= 6 & Functions.OsMinor >= 1)
            {
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
            }

            //disable buttons and radio controls during encoding
            btnAdd.IsEnabled = false;
            btnRemove.IsEnabled = false;
            btnClear.IsEnabled = false;
            btnSettings.IsEnabled = false;
            btnEncode.IsEnabled = false;

            //get the number of videos to encode
            VideoCount = lstVideos.Items.Count;
            Functions.Posters = new string[VideoCount];
            Functions.AtomicArgs = new string[VideoCount];
            Functions.OutFile = new string[VideoCount];

            //check which profile to use and set EncodeTo string appropriately
            switch (Functions.DefaultEncodeProfile)
            {
                case 0:
                    EncodeTo = " -Z \"iPhone & iPod Touch\"";
                    OutExt = ".m4v";
                    break;
                case 1:
                    EncodeTo = " -Z \"iPhone 4\"";
                    OutExt = ".m4v";
                    break;
                case 2:
                    EncodeTo = " -Z \"iPad\"";
                    OutExt = ".m4v";
                    break;
                case 3:
                    EncodeTo = " -Z \"iPod\"";
                    OutExt = ".m4v";
                    break;
                case 4:
                    EncodeTo = " -Z \"Android Mid\"";
                    OutExt = ".mp4";
                    break;
                case 5:
                    //EncodeTo = "Android High";
                    OutExt = ".mp4";
                    AddedOptions = "-e x264  -q 22.0 -r 29.97 --pfr  -a 1 -E faac -B 128 -6 dpl2 -R Auto -D 0.0 -f mp4 -X 720 --keep-display-aspect -x weightp=0:cabac=0 ";
                    break;
                case 6:
                    //EncodeTo = "Android High";
                    OutExt = ".mp4";
                    AddedOptions = "-e x264  -q 22.0 -r 29.97 --pfr  -a 1 -E faac -B 128 -6 dpl2 -R Auto -D 0.0 -f mp4 -X 1280 --keep-display-aspect -x weightp=0:cabac=0 ";
                    break;
            }

            //check if autocrop is turned on and set string
            if (!Functions.AutoCrop)
            {
                CropVideo = "--crop 0:0:0:0";
            }




            for (int Video = 0; Video <= (VideoCount - 1); Video++)
            {
                if (Functions.AutoTVMeta | Functions.AutoMoviePoster)
                {
                    string[] FileNameSplit = null;
                    string[] MetaData = new string[6];
                    long SeriesID = 0;
                    int[] EpisodeData = new int[2];
                    int EpisodeIndex = 0;
                    string Poster = "";
                    bool TVFound = false;
                    bool MovieFound = false;
                    string Title = "";
                    string[] TMDbID = { "", string.Empty };

                    //Set Array Length for Args & Posters variables
                    // ERROR: Not supported in C#: ReDimStatement

                    // ERROR: Not supported in C#: ReDimStatement
                    Array.Clear(MetaData, MetaData.GetLowerBound(0), MetaData.Length);
                    Array.Clear(EpisodeData, EpisodeData.GetLowerBound(0), EpisodeData.Length);

                    Outfile = System.IO.Path.GetFileNameWithoutExtension(lstVideos.Items[Video].ToString());
                    Poster = "";
                    TVFound = false;
                    MovieFound = false;

                    //Get metadata from SageTV recording
                    MetaData = Functions.GetMetaData(lstVideos.Items[Video].ToString());

                    //Split Filename
                    FileNameSplit = Outfile.Split(new string[] { " - " }, StringSplitOptions.None);

                    // If the filename has three or more segments it means it could be a TV show
                    if (FileNameSplit.Length >= 3 & Functions.AutoTVMeta)
                    {

                        //Get Show title and episode name from filename
                        MetaData[0] = FileNameSplit[0];
                        MetaData[1] = FileNameSplit[2];

                        //Get season number and episode number from filename assuming format SxxExx or similar
                        EpisodeIndex = FileNameSplit[1].IndexOf('E');
                        EpisodeData[0] = Convert.ToInt32(FileNameSplit[1].Substring(1, EpisodeIndex - 1));
                        EpisodeData[1] = Convert.ToInt32(FileNameSplit[1].Substring(EpisodeIndex + 1));

                        //Get TVDB series ID using series title
                        SeriesID = Functions.GetSeriesID(MetaData[0]);
                        //Make sure a series ID has been found before getting poster
                        if (SeriesID > 0)
                        {
                            //Get poster using TVDB series ID
                            Poster = Functions.GetTVPoster(SeriesID, EpisodeData[0]);
                            //Indicate it has been found
                            Functions.OutFile[Video] = Outfile;
                            TVFound = true;
                        }
                        //ElseIf MetaData(0) <> String.Empty And MetaData(1) <> String.Empty And AutoTVMeta Then 'Otherwise if check to make sure the metadata exists
                        //Otherwise if check to make sure the metadata exists
                    }
                    else if (MetaData[0] != null & Functions.AutoTVMeta)
                    {
                        //Get series ID using the series title and ID
                        SeriesID = Functions.GetSeriesID(MetaData[0], MetaData[2]);
                        //Get the season number and episode number
                        //If a series ID was found continue
                        if (SeriesID > 0)
                        {
                            if (MetaData[1] != null)
                            {
                                EpisodeData = Functions.GetEpisodeData(MetaData[1], SeriesID);
                                if (EpisodeData[0] == 0 & EpisodeData[1] == 0)
                                {
                                    EpisodeData[0] = Convert.ToInt32(MetaData[3]);
                                    EpisodeData[1] = Convert.ToInt32(MetaData[4]);
                                }
                            }
                            else
                            {
                                EpisodeData[0] = Convert.ToInt32(MetaData[3]);
                                EpisodeData[1] = Convert.ToInt32(MetaData[4]);
                            }
                            //Use the file metadata if TVDB search failed
                            //Get poster from TVDB
                            Poster = Functions.GetTVPoster(SeriesID, EpisodeData[0]);
                        }
                        else
                        {
                            if (Convert.ToInt32(MetaData[3]) > 0 & Convert.ToInt32(MetaData[4]) > 0)
                            {
                                EpisodeData[0] = Convert.ToInt32(MetaData[3]);
                                EpisodeData[1] = Convert.ToInt32(MetaData[4]);
                            }
                            else
                            {
                                for (int x = 0; x <= 5; x++)
                                {
                                    MetaData[x] = string.Empty;
                                }
                                //break; // TODO: might not be correct. Was : Exit For
                            }
                            //Indicate something was found
                        }
                        if (EpisodeData[0] != 0 & EpisodeData[1] != 0)
                        {
                            Outfile = MetaData[0] + " - " + "S" + string.Format("{0:00}", EpisodeData[0]) + "E" + string.Format("{0:00}", EpisodeData[1]) + " - " + MetaData[1];
                            Outfile = Regex.Replace(Outfile, "[\\/:*?\"<>|]", "");
                        }
                        Functions.OutFile[Video] = Outfile;
                        TVFound = true;
                        //If movie poster capture is enabled
                    }
                    else if (Functions.AutoMoviePoster)
                    {
                        //If a file metadata ID was captured continue
                        if (MetaData[2] != null)
                        {
                            //If the metadata ID contains "MV" use the title and year from the metadata
                            if (MetaData[2].Substring(0, 2) == "MV")
                            {
                                Title = MetaData[0];
                                TMDbID = Functions.GetMovieID(MetaData[0], MetaData[5]);
                            }
                            //Otherwise use the filename for the movie title
                        }
                        else
                        {
                            Title = Outfile;
                            TMDbID = Functions.GetMovieID(Outfile);
                            if (TMDbID[1] != string.Empty)
                            {
                                Title = TMDbID[1];
                            }
                        }

                        //If a TMDB ID was captured get a poster and set it to found
                        if (!string.IsNullOrEmpty(TMDbID[0]))
                        {
                            Poster = Functions.GetMoviePoster(TMDbID[0]);
                            Functions.OutFile[Video] = Outfile;
                            MovieFound = true;
                        }
                    }

                    if (MetaData[1] != null)
                    {
                        if (MetaData[1].IndexOf("\\") > 0)
                        {
                            MetaData[1] = MetaData[1].Remove(MetaData[1].IndexOf("\\"), 1);
                        }
                    }

                    //Continue if something was found
                    if (TVFound)
                    {
                        //If no poster was found create atomicparsley command line without artwork
                        if (string.IsNullOrEmpty(Poster))
                        {
                            Functions.Posters[Video] = "";
                            if (MetaData[1] != string.Empty)
                            {
                                Functions.AtomicArgs[Video] = "\"" + Functions.OutPath + "\\" + Outfile + OutExt + "\" --TVShowName \"" + MetaData[0] + "\" --TVEpisode \"" + MetaData[1] + "\" --TVSeasonNum " + EpisodeData[0].ToString() + " --TVEpisodeNum " + EpisodeData[1].ToString() + " --stik \"TV Show\" --title \"" + MetaData[1] + "\" --overWrite";
                            }
                            else
                            {
                                Functions.AtomicArgs[Video] = "\"" + Functions.OutPath + "\\" + Outfile + OutExt + "\" --TVShowName \"" + MetaData[0] + "\" --TVSeasonNum " + EpisodeData[0].ToString() + " --TVEpisodeNum " + EpisodeData[1].ToString() + " --stik \"TV Show\" --overWrite";
                            }
                            //Otherwise include artwork
                        }
                        else
                        {
                            Functions.Posters[Video] = Poster;
                            if (MetaData[1] != string.Empty)
                            {
                                Functions.AtomicArgs[Video] = "\"" + Functions.OutPath + "\\" + Outfile + OutExt + "\" --TVShowName \"" + MetaData[0] + "\" --TVEpisode \"" + MetaData[1] + "\" --TVSeasonNum " + EpisodeData[0].ToString() + " --TVEpisodeNum " + EpisodeData[1].ToString() + " --stik \"TV Show\" --title \"" + MetaData[1] + "\" --overWrite --artwork \"" + Poster + "\"";
                            }
                            else
                            {
                                Functions.AtomicArgs[Video] = "\"" + Functions.OutPath + "\\" + Outfile + OutExt + "\" --TVShowName \"" + MetaData[0] + "\" --TVSeasonNum " + EpisodeData[0].ToString() + " --TVEpisodeNum " + EpisodeData[1].ToString() + " --stik \"TV Show\" --overWrite --artwork \"" + Poster + "\"";
                            }
                        }
                        //If a movie poster was found create command line
                    }
                    else if (MovieFound & !string.IsNullOrEmpty(Poster))
                    {
                        Functions.Posters[Video] = Poster;
                        Functions.AtomicArgs[Video] = "\"" + Functions.OutPath + "\\" + Outfile + OutExt + "\" --stik \"Movie\" --title \"" + Title + "\" --overWrite --artwork \"" + Poster + "\"";
                    }
                    else
                    {
                        Functions.OutFile[Video] = System.IO.Path.GetFileNameWithoutExtension(lstVideos.Items[Video].ToString());
                    }

                    for (int x = 0; x <= 5; x++)
                    {
                        MetaData[x] = string.Empty;
                    }

                    TVFound = false;
                    MovieFound = false;
                }
                else
                {
                    Functions.OutFile[Video] = System.IO.Path.GetFileNameWithoutExtension(lstVideos.Items[Video].ToString());
                }

            }

            //start looping through videos in list box
            for (int Video = 1; Video <= VideoCount; Video++)
            {
                //get the filename of the input file without extension
                InFile = System.IO.Path.GetFileNameWithoutExtension(lstVideos.Items[0].ToString());
                //get the path to the input file to detect for edl file
                InPath = System.IO.Path.GetDirectoryName(lstVideos.Items[0].ToString());
                //create file path for input video's edl file

                if (InPath.Substring((InPath.Length - 1), 1) == "\\")
                {
                    edlFile = InPath + InFile + ".edl";
                }
                else
                {
                    edlFile = InPath + "\\" + InFile + ".edl";
                }

                //check if an edl file exists for the input video
                //use edl file if it exists and RemoveAds is selected
                if (File.Exists(edlFile) & Functions.RemoveAds)
                {
                    StreamReader edlFileRead = default(StreamReader);
                    int numlines = 0;
                    float currTimeIndex = 0;
                    bool processPart = false;
                    bool noStartAt = false;
                    currTimeIndex = 0;

                    edlFileRead = File.OpenText(edlFile);

                    //run through edl file to determine the number of lines
                    while (edlFileRead.Peek() != -1)
                    {
                        edlFileRead.ReadLine();
                        numlines += 1;
                    }

                    edlFileRead.Close();

                    float[,] edldata = new float[numlines + 1, 2];
                    //Dim edldata(numlines - 1, 1) As Single
                    int parts = 0;
                    string[] edlLine = null;
                    float progress = 0;
                    float currprog = 0;
                    float progmod = 0;

                    edlFileRead = File.OpenText(edlFile);

                    //read lines from edl file and put values into edldata array
                    for (int x = 0; x <= (numlines - 1); x++)
                    {
                        edlLine = edlFileRead.ReadLine().Split('\t');
                        if (x == 0 & Convert.ToSingle(edlLine[0]) < 1)
                        {
                            edlLine[0] = "0";
                        }
                        if (edlLine[0] == edlLine[1])
                        {
                            numlines -= 1;
                        }
                        else
                        {
                            for (int y = 0; y <= 1; y++)
                            {
                                edldata[x, y] = Convert.ToSingle(edlLine[y]);
                            }
                        }
                    }

                    edlFileRead.Close();

                    //if the very first ad break begins at the beginning of the video decrement numlines by 1
                    if (!(edldata[0, 0] == 0))
                    {
                        progmod = 1;
                        //numlines -= 1
                        //Else
                        //numlines += 1
                    }

                    //make progress bar, progress label, and cancel button visible
                    this.Title = "Batch Video Converter - 0.0%";
                    lblPerComp.Content = "Video: " + Video.ToString() + "/" + VideoCount.ToString() + " - 0.0%";
                    encodeProgress.Value = 0;
                    encodeProgress.Visibility = Visibility.Visible;
                    lblPerComp.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;

                    //begin processing and encoding each section of "show" video sections
                    //For arrayLine As Integer = 0 To (numlines - 1)
                    //    'increment the number of video parts by 1
                    //    parts += 1
                    //    'determine which arguments to use based on edl data
                    //    If Not edldata(0, 0) = 0 And arrayLine = 0 Then
                    //        'if the first edl value is not 0 start encoding from the beginning and stop at the first value
                    //        Args = CropVideo & "-Z """ & EncodeTo & """ -i """ & lstVideos.Items(0).ToString & """ -o """ & OutPath & "\" & Outfile & "." & (arrayLine + 1).ToString & ".m4v"" --stop-at duration:" & edldata(0, 0).ToString

                    //    ElseIf (edldata(0, 0) = 0 And edldata((arrayLine + 1), 0) = edldata((arrayLine + 1), 1)) Or (edldata(0, 0) = 0 And (arrayLine = (numlines - 1))) Then
                    //        'if the very first edl value is 0 and the values on the next line are equal
                    //        'use the current second value as a start and encode to the end
                    //        Args = CropVideo & "-Z """ & EncodeTo & """ -i """ & lstVideos.Items(0).ToString & """ -o """ & OutPath & "\" & Outfile & "." & (arrayLine + 1).ToString & ".m4v"" --start-at duration:" & edldata(arrayLine, 1).ToString
                    //        arrayLine = numlines
                    //    ElseIf (edldata(arrayLine, 0) = edldata(arrayLine, 1)) Or (arrayLine = (numlines - 1)) Then
                    //        'if the two edl values are equal this indicates the end of the video
                    //        'encoding should start at the second value of the previous edl line and go to the end of the video
                    //        'If edldata(arrayLine, 0) = edldata(arrayLine, 1) Then
                    //        Args = CropVideo & "-Z """ & EncodeTo & """ -i """ & lstVideos.Items(0).ToString & """ -o """ & OutPath & "\" & Outfile & "." & (arrayLine + 1).ToString & ".m4v"" --start-at duration:" & edldata((arrayLine - 1), 1).ToString
                    //        'ElseIf arrayLine = (numlines - 1) Then
                    //        'Args = CropVideo & "-Z """ & EncodeTo & """ -i """ & lstVideos.Items(0).ToString & """ -o """ & OutPath & "\" & Outfile & "." & (arrayLine + 1).ToString & ".m4v"" --start-at duration:" & edldata((arrayLine - 1), 1).ToString
                    //        'End If
                    //        'set arrayLine to equal numlines so that loop will not go again
                    //        arrayLine = numlines
                    //    Else
                    //        'otherwise the section of video is in the middle and needs a duration
                    //        If edldata(0, 0) = 0 Then
                    //            'first edl value is 0, use duration from first edl value on next line to the second edl value on current line
                    //            'start at second edl value on current line
                    //            Dim duration As Single = edldata(arrayLine + 1, 0) - edldata((arrayLine), 1)
                    //            Args = CropVideo & "-Z """ & EncodeTo & """ -i """ & lstVideos.Items(0).ToString & """ -o """ & OutPath & "\" & Outfile & "." & (arrayLine + 1).ToString & ".m4v"" --start-at duration:" & edldata(arrayLine, 1).ToString & " --stop-at duration:" & duration.ToString
                    //        Else
                    //            'otherwise, use duration from first edl value on current line to the second edl value on previous line
                    //            'start at second edl value on previous line
                    //            Dim duration As Single = edldata(arrayLine, 0) - edldata((arrayLine - 1), 1)
                    //            Args = CropVideo & "-Z """ & EncodeTo & """ -i """ & lstVideos.Items(0).ToString & """ -o """ & OutPath & "\" & Outfile & "." & (arrayLine + 1).ToString & ".m4v"" --start-at duration:" & edldata((arrayLine - 1), 1).ToString & " --stop-at duration:" & duration.ToString
                    //        End If
                    //    End If

                    // NOTE THAT WE ARE PROCESSING ONE ITEM PAST THE NUMBER OF LINES
                    for (int arrayLine = 0; arrayLine <= numlines; arrayLine++)
                    {


                        processPart = false;
                        noStartAt = false;

                        // Is this the last one?
                        if (arrayLine == numlines)
                        {
                            // If it is, just continue processing from the current time marker right to the end. 
                            Args = " --start-at duration:" + currTimeIndex;
                            //increment the number of video parts by 1
                            parts += 1;
                            processPart = true;
                        }
                        else
                        {
                            // This is not the last block.
                            // Process the time from the current time index to the start of this block.
                            // But if this block starts at the same time as our time index, then there's nothing to process, and we can skip this block.
                            if (edldata[arrayLine, 0] == currTimeIndex)
                            {
                                // Nothing to process. Skip this block.
                                // Set the current index to the end of this block, when the commercials are over.
                                currTimeIndex = edldata[arrayLine, 1];
                            }
                            else
                            {
                                // Process this block.
                                // If the start time is greater than zero, it means we must specify the start time.
                                if (currTimeIndex > 0)
                                {
                                    // The duration is, of course, the end time minus the start time.
                                    Args = " --start-at duration:" + currTimeIndex.ToString() + " --stop-at duration:" + (edldata[arrayLine, 0] - currTimeIndex).ToString();
                                }
                                else
                                {
                                    Args = " --stop-at duration:" + edldata[arrayLine, 0].ToString();
                                    noStartAt = true;
                                }

                                // Set the current time index to after the commercial.
                                currTimeIndex = edldata[arrayLine, 1];

                                processPart = true;
                                parts += 1;
                            }
                        }


                        if (processPart)
                        {
                            //Args = "--gain 10.0 " + CropVideo + " -Z \"" + EncodeTo + "\" -i \"" + lstVideos.Items[0].ToString() + "\" -o \"" + Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + "." + parts.ToString() + OutExt + "\"" + Args;
                            Args = AddedOptions + CropVideo + EncodeTo + " -i \"" + lstVideos.Items[0].ToString() + "\" -o \"" + Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + "." + parts.ToString() + OutExt + "\"" + Args;

                            //set process properties and start HandBrakeCLI
                            using (Process handbrake = new Process())
                            {
                                handbrake.StartInfo.UseShellExecute = false;
                                handbrake.StartInfo.CreateNoWindow = true;
                                handbrake.StartInfo.RedirectStandardOutput = true;
                                handbrake.StartInfo.FileName = Functions.HBPath;
                                handbrake.StartInfo.Arguments = Args;
                                handbrake.Start();
                                //start capturing the stdout from HandBrakeCLI
                                using (StreamReader stdout = handbrake.StandardOutput)
                                {

                                    //HandBrakeCLI has two progress percentages when --start-at is used
                                    //one for seek and once it finds the start point begins encoding

                                    //start processing stdout
                                    //if the first edl value is not 0 we only need to process encoding progress
                                    //If Not edldata(0, 0) = 0 And arrayLine = 0 Then
                                    if (noStartAt)
                                    {
                                        //wait for "Encoding:" to appear at the beginning of the standard output
                                        do
                                        {
                                            if (handbrake.HasExited)
                                            {
                                                HBFail = true;
                                                MessageBox.Show("HandBrake Error Has Occurred", "HandBrake Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                                break; // TODO: might not be correct. Was : Exit For
                                            }
                                            try
                                            {
                                                outtemp = await stdout.ReadLineAsync();
                                                output = outtemp.Split(' ');
                                            }
                                            catch
                                            {
                                            }
                                        } while (!(output[0] == "Encoding:"));

                                        //capture stdout until encoding is finished
                                        do
                                        {
                                            //split stdout line
                                            try
                                            {
                                                outtemp = await stdout.ReadLineAsync();
                                                output = outtemp.Split(' ');
                                            }
                                            catch
                                            {

                                            }

                                            //make sure the number of values on line is valid before processing
                                            if (output.Length >= 6)
                                            {
                                                if (float.TryParse(output[5], out percentage))
                                                {
                                                    //capture progress from stdout and calculate overall current progress
                                                    currprog = percentage / (numlines + progmod);
                                                    //set progress bar value and progress label
                                                    encodeProgress.Value = Convert.ToInt32(currprog);
                                                    lblPerComp.Content = "Video: " + Video.ToString() + "/" + VideoCount.ToString() + " - " + currprog.ToString("##0.0") + "%";
                                                    //calculate total progress for all videos and set window title
                                                    //set value on taskbar progress if running on Windows 7 or higher
                                                    AllProgress = (100 * (Video - 1) + currprog) / VideoCount;
                                                    this.Title = "Batch Video Converter - " + AllProgress.ToString("##0.0") + "%";
                                                    if (Functions.OsMajor >= 6 & Functions.OsMinor >= 1)
                                                    {
                                                        TaskbarManager.Instance.SetProgressValue(Convert.ToInt32(AllProgress), 100);
                                                    }
                                                }
                                            }

                                            //if the cancel button was pressed kill HandBrakeCLI, wait for it to exit and delete all parts of the output video
                                            //stop processing videos
                                            if (KillHB)
                                            {
                                                handbrake.Kill();
                                                handbrake.WaitForExit();
                                                for (int prt = 1; prt <= (arrayLine + 1); prt++)
                                                {
                                                    System.IO.File.Delete(Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + "." + prt.ToString() + OutExt);
                                                }
                                                for (int Poster = 0; Poster <= (Functions.Posters.Length - 1); Poster++)
                                                {
                                                    if (!string.IsNullOrEmpty(Functions.Posters[Poster]))
                                                    {
                                                        File.Delete(Functions.Posters[Poster]);
                                                    }
                                                }
                                                break; // TODO: might not be correct. Was : Exit For
                                            }
                                            //Loop Until HandBrakeCLI has finished and exited
                                        } while (!(handbrake.HasExited));

                                        //update the progress based on what it should be, captured stdout usually doesn't get to 100.0
                                        progress = 100 / (numlines + progmod);
                                        encodeProgress.Value = Convert.ToInt32(progress);
                                        lblPerComp.Content = "Video: " + Video.ToString() + "/" + VideoCount.ToString() + " - " + progress.ToString("##0.0") + "%";
                                        AllProgress = (100 * (Video - 1) + progress) / VideoCount;
                                        this.Title = "Batch Video Converter - " + AllProgress.ToString("##0.0") + "%";
                                        //if on Windows 7 set taskbar progress
                                        if (Functions.OsMajor >= 6 & Functions.OsMinor >= 1)
                                        {
                                            TaskbarManager.Instance.SetProgressValue(Convert.ToInt32(AllProgress), 100);
                                        }
                                    }
                                    else
                                    {
                                        //section is in the middle or end of video

                                        //wait for "Encoding:" to appear at the beginning of the standard output
                                        if (arrayLine != 0 && edldata[0, 0] != 0)
                                        {
                                            do
                                            {
                                                if (handbrake.HasExited)
                                                {
                                                    HBFail = true;
                                                    MessageBox.Show("HandBrake Error Has Occurred", "HandBrake Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                                    break; // TODO: might not be correct. Was : Exit For
                                                }
                                                outtemp = await stdout.ReadLineAsync();
                                                output = outtemp.Split(' ');
                                            } while (!(output[0] == "Encoding:"));

                                            //capture stdout until seeking is finished
                                            do
                                            {
                                                if (handbrake.HasExited)
                                                {
                                                    HBFail = true;
                                                    MessageBox.Show("HandBrake Error Has Occurred", "HandBrake Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                                    break; // TODO: might not be correct. Was : Exit For
                                                }
                                                //split stdout line
                                                outtemp = await stdout.ReadLineAsync();
                                                output = outtemp.Split(' ');
                                                //make sure the number of values on line is valid before processing
                                                if (output.Length >= 10)
                                                {
                                                    if (float.TryParse(output[9], out percentage) && percentage > 0)
                                                    {
                                                        //capture progress from stdout and calculate overall current progress
                                                        currprog = progress + (percentage / (numlines + progmod) / 2);
                                                        //set progress bar value and progress label
                                                        encodeProgress.Value = Convert.ToInt32(currprog);
                                                        lblPerComp.Content = "Video: " + Video.ToString() + "/" + VideoCount.ToString() + " - " + currprog.ToString("##0.0") + "%";
                                                        //calculate total progress for all videos and set window title
                                                        //set value on taskbar progress if running on Windows 7 or higher
                                                        AllProgress = (100 * (Video - 1) + currprog) / VideoCount;
                                                        this.Title = "Batch Video Converter - " + AllProgress.ToString("##0.0") + "%";
                                                        if (Functions.OsMajor >= 6 & Functions.OsMinor >= 1)
                                                        {
                                                            TaskbarManager.Instance.SetProgressValue(Convert.ToInt32(AllProgress), 100);
                                                        }
                                                    }
                                                }

                                                //if the cancel button was pressed kill HandBrakeCLI, wait for it to exit and delete all parts of the output video
                                                //stop processing videos
                                                if (KillHB)
                                                {
                                                    handbrake.Kill();
                                                    handbrake.WaitForExit();
                                                    for (int prt = 1; prt <= (arrayLine + 1); prt++)
                                                    {
                                                        System.IO.File.Delete(Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + "." + prt.ToString() + OutExt);
                                                    }
                                                    break; // TODO: might not be correct. Was : Exit Do
                                                }
                                                //loop until the captured percentage rounds up to 100 to prevent an infinite loop
                                            } while (!(Convert.ToInt32(percentage) > 90));
                                        }
                                        //update progress and total progress to make up for current progress not reaching 100%
                                        progress += 100 / (numlines + progmod) / 2;
                                        encodeProgress.Value = Convert.ToInt32(progress);
                                        lblPerComp.Content = "Video: " + Video.ToString() + "/" + VideoCount.ToString() + " - " + progress.ToString("##0.0") + "%";
                                        AllProgress = (100 * (Video - 1) + progress) / VideoCount;
                                        this.Title = "Batch Video Converter - " + AllProgress.ToString("##0.0") + "%";
                                        if (Functions.OsMajor >= 6 & Functions.OsMinor >= 1)
                                        {
                                            TaskbarManager.Instance.SetProgressValue(Convert.ToInt32(AllProgress), 100);
                                        }
                                        //exit for loop if cancel button is pressed and clean up handbrake process
                                        if (KillHB)
                                        {
                                            handbrake.WaitForExit();
                                            for (int Poster = 0; Poster <= (Functions.Posters.Length - 1); Poster++)
                                            {
                                                if (!string.IsNullOrEmpty(Functions.Posters[Poster]))
                                                {
                                                    File.Delete(Functions.Posters[Poster]);
                                                }
                                            }
                                            break; // TODO: might not be correct. Was : Exit For
                                        }

                                        //wait for encoding to start
                                        do
                                        {
                                            if (handbrake.HasExited)
                                            {
                                                HBFail = true;
                                                MessageBox.Show("HandBrake Error Has Occurred", "HandBrake Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                                break; // TODO: might not be correct. Was : Exit For
                                            }
                                            outtemp = await stdout.ReadLineAsync();
                                            output = outtemp.Split(' ');
                                        } while (!(output[0] == "Encoding:"));
                                        //capture stdout until encoding is finished
                                        do
                                        {
                                            //split stdout line
                                            try
                                            {
                                                outtemp = await stdout.ReadLineAsync();
                                                output = outtemp.Split(' ');
                                            }
                                            catch
                                            {
                                            }
                                            //make sure the number of values on line is valid before processing
                                            if (output.Length >= 6)
                                            {
                                                if (float.TryParse(output[5], out percentage) && percentage > 0)
                                                {
                                                    //capture progress from stdout and calculate overall current progress
                                                    currprog = progress + (percentage / (numlines + progmod) / 2);
                                                    //set progress bar value and progress label
                                                    encodeProgress.Value = Convert.ToInt32(currprog);
                                                    lblPerComp.Content = "Video: " + Video.ToString() + "/" + VideoCount.ToString() + " - " + currprog.ToString("##0.0") + "%";
                                                    //calculate total progress for all videos and set window title
                                                    //set value on taskbar progress if running on Windows 7 or higher
                                                    AllProgress = (100 * (Video - 1) + currprog) / VideoCount;
                                                    this.Title = "Batch Video Converter - " + AllProgress.ToString("##0.0") + "%";
                                                    if (Functions.OsMajor >= 6 & Functions.OsMinor >= 1)
                                                    {
                                                        TaskbarManager.Instance.SetProgressValue(Convert.ToInt32(AllProgress), 100);
                                                    }
                                                }
                                            }

                                            //if the cancel button was pressed kill HandBrakeCLI, wait for it to exit and delete all parts of the output video
                                            //stop processing videos
                                            if (KillHB)
                                            {
                                                handbrake.Kill();
                                                handbrake.WaitForExit();
                                                for (int prt = 1; prt <= (arrayLine + 1); prt++)
                                                {
                                                    System.IO.File.Delete(Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + "." + prt.ToString() + OutExt);
                                                }
                                                break; // TODO: might not be correct. Was : Exit Do
                                            }
                                            //loop until HandBrakeCLI has exited
                                        } while (!(handbrake.HasExited));
                                        //update progress and total progress to make up for current progress not reaching 100%
                                        if (!KillHB)
                                        {
                                            progress += 100 / (numlines + progmod) / 2;
                                            encodeProgress.Value = Convert.ToInt32(progress);
                                            lblPerComp.Content = "Video: " + Video.ToString() + "/" + VideoCount.ToString() + " - " + progress.ToString("##0.0") + "%";
                                            AllProgress = (100 * (Video - 1) + progress) / VideoCount;
                                            this.Title = "Batch Video Converter - " + AllProgress.ToString("##0.0") + "%";
                                            if (Functions.OsMajor >= 6 & Functions.OsMinor >= 1)
                                            {
                                                TaskbarManager.Instance.SetProgressValue(Convert.ToInt32(AllProgress), 100);
                                            }
                                        }
                                    }

                                    //clean up handbrake process
                                    handbrake.WaitForExit();
                                }
                            }

                            //if cancel was pressed exit for loop
                            if (KillHB)
                            {
                                for (int Poster = 0; Poster <= (Functions.Posters.Length - 1); Poster++)
                                {
                                    if (!string.IsNullOrEmpty(Functions.Posters[Poster]))
                                    {
                                        File.Delete(Functions.Posters[Poster]);
                                    }
                                }
                                break; // TODO: might not be correct. Was : Exit For
                            }
                        }
                    }

                    //make progress bar, label, and cancel button invisible
                    lblPerComp.Visibility = Visibility.Hidden;
                    encodeProgress.Visibility = Visibility.Hidden;
                    btnCancel.Visibility = Visibility.Hidden;

                    //use mp4box to combine video parts if cancel button not pressed
                    if (!KillHB & !HBFail)
                    {
                        Args = "";
                        //create arguments for mp4box to combine video files
                        for (int part = 1; part <= parts; part++)
                        {
                            if (part == 1)
                            {
                                // First one must be done with 'add' otherwise a busted file happens
                                Args += "-add \"" + Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + "." + part.ToString() + OutExt + "\" ";
                            }
                            else
                            {
                                Args += "-cat \"" + Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + "." + part.ToString() + OutExt + "\" ";
                            }
                        }
                        Args += "\"" + Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + OutExt + "\"";
                        this.Title = "Batch Video Converter - Running MP4Box";
                        //set mp4box process properties
                        using (Process mp4box = new Process())
                        {
                            mp4box.StartInfo.UseShellExecute = false;
                            mp4box.StartInfo.CreateNoWindow = true;
                            mp4box.StartInfo.RedirectStandardOutput = false;
                            mp4box.StartInfo.FileName = Functions.mp4boxPath;
                            mp4box.StartInfo.Arguments = Args;
                            //start mp4box and wait for it to close
                            mp4box.Start();
                            mp4box.WaitForExit();
                        }
                        this.Title = "Batch Video Converter";
                        //delete parts of video
                        for (int part = 1; part <= parts; part++)
                        {
                            System.IO.File.Delete(Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + "." + part.ToString() + OutExt);
                        }
                    }
                }
                else
                {
                    //no EDL file exists or if commercial removal disabled 
                    float progress = 0;

                    //set HandBrakeCLI arguments
                    //Args = "--gain 10.0 " + CropVideo + " -Z \"" + EncodeTo + "\" -i \"" + lstVideos.Items[0].ToString() + "\" -o \"" + Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + OutExt + "\"";
                    Args = AddedOptions + CropVideo + EncodeTo + " -i \"" + lstVideos.Items[0].ToString() + "\" -o \"" + Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + OutExt + "\"";

                    //make progress bar, label, and cancel button visible
                    lblPerComp.Visibility = Visibility.Visible;
                    encodeProgress.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;

                    //HandbrakeInterop Test
                    //Dim hbio As New HandBrakeInstance
                    //Dim HBEncode As New EncodeJob
                    //Dim HBEncodeProfile As New Encoding.EncodingProfile
                    //Dim HBAudio As New AudioEncoding
                    //Dim HBAudioList As New List(Of AudioEncoding)
                    //Dim HBAudioTracks As New List(Of Integer)

                    //Using HBInst As New HandBrakeInstance
                    //    HBInst.Initialize(0)

                    //    HBInst.StartScan(lstVideos.Items(0).ToString, 1, 0)

                    //    'Do
                    //    '    Application.DoEvents()
                    //    'Loop Until HBInst.ScanCompleted(Me, e)

                    //    HBAudio.Encoder = 1
                    //    HBAudio.EncodeRateType = AudioEncodeRateType.Bitrate
                    //    HBAudio.Bitrate = 128
                    //    HBAudio.SampleRateRaw = 48000
                    //    HBAudio.Mixdown = "Dolby Pro Logic II"
                    //    HBAudio.InputNumber = 1

                    //    HBAudioList.Add(HBAudio)

                    //    HBEncodeProfile.OutputFormat = Encoding.Container.Mp4
                    //    HBEncodeProfile.AudioEncodings = HBAudioList
                    //    HBEncodeProfile.Anamorphic = Encoding.Anamorphic.None
                    //    HBEncodeProfile.Width = 480
                    //    HBEncodeProfile.MaxHeight = 320
                    //    HBEncodeProfile.Modulus = 16
                    //    HBEncodeProfile.VideoEncodeRateType = VideoEncodeRateType.ConstantQuality
                    //    HBEncodeProfile.Quality = 20
                    //    HBEncodeProfile.VideoEncoder = "x264"
                    //    HBEncodeProfile.X264Options = "cabac=0:ref=2:me=umh:bframes=0:weightp=0:subq=6:8x8dct=0:trellis=0"
                    //    HBEncodeProfile.IncludeChapterMarkers = True

                    //    HBEncode.EncodingProfile = HBEncodeProfile
                    //    HBEncode.SourceType = SourceType.File
                    //    HBEncode.SourcePath = lstVideos.Items(0).ToString
                    //    HBEncode.Title = 1
                    //    HBEncode.Angle = 0
                    //    HBEncode.OutputPath = OutPath & "\" & Outfile & ".m4v"
                    //    HBAudioTracks.Add(1)
                    //    HBEncode.ChosenAudioTracks = HBAudioTracks

                    //    HBInst.StartEncode(HBEncode)
                    //End Using



                    //set handbrake process properties
                    using (Process handbrake = new Process())
                    {
                        handbrake.StartInfo.UseShellExecute = false;
                        handbrake.StartInfo.CreateNoWindow = true;
                        handbrake.StartInfo.RedirectStandardOutput = true;
                        handbrake.StartInfo.FileName = Functions.HBPath;
                        handbrake.StartInfo.Arguments = Args;

                        //start HandbrakeCLI and start capturing stdout
                        handbrake.Start();
                        using (StreamReader stdout = handbrake.StandardOutput)
                        {

                            //start processing stdout
                            //wait for encoding to appear on stdout line
                            do
                            {
                                outtemp = await stdout.ReadLineAsync();
                                output = outtemp.Split(' ');
                            } while (!(output[0] == "Encoding:"));

                            //capture stdout until encoding is finished
                            do
                            {
                                //split stdout line
                                try
                                {
                                    outtemp = await stdout.ReadLineAsync();
                                    output = outtemp.Split(' ');
                                }
                                catch
                                {
                                }
                                //make sure the number of values on line is valid before processing
                                if (output.Length >= 6)
                                {
                                    if (float.TryParse(output[5], out percentage))
                                    {
                                        //capture progress from stdout and calculate progress                            
                                        progress = Convert.ToSingle(percentage);
                                        //set progress bar value and progress label
                                        encodeProgress.Value = Convert.ToInt32(progress);
                                        lblPerComp.Content = "Video: " + Video.ToString() + "/" + VideoCount.ToString() + " - " + progress.ToString("##0.0") + "%";
                                        //calculate total progress for all videos and set window title
                                        //set value on taskbar progress if running on Windows 7 or higher
                                        AllProgress = (100 * (Video - 1) + progress) / VideoCount;
                                        this.Title = "Batch Video Converter - " + AllProgress.ToString("##0.0") + "%";
                                        if (Functions.OsMajor >= 6 & Functions.OsMinor >= 1)
                                        {

                                            TaskbarManager.Instance.SetProgressValue(Convert.ToInt32(AllProgress), 100);
                                        }
                                    }
                                }

                                //if the cancel button was pressed kill HandBrakeCLI, wait for it to exit
                                //delete all parts of the output video
                                //stop processing videos
                                if (KillHB)
                                {
                                    handbrake.Kill();
                                    handbrake.WaitForExit();
                                    File.Delete(Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + OutExt);
                                    break; // TODO: might not be correct. Was : Exit Do
                                }
                            } while (!(handbrake.HasExited));
                        }
                    }

                    //make progress bar, label, and cancel button invisible
                    lblPerComp.Visibility = Visibility.Hidden;
                    encodeProgress.Visibility = Visibility.Hidden;
                    btnCancel.Visibility = Visibility.Hidden;
                }

                //if cancel button was pressed set kill variable back to false and exit for loop
                if (KillHB)
                {
                    KillHB = false;
                    if (Functions.AutoTVMeta | Functions.AutoMoviePoster)
                    {
                        string[] AlreadyDeleted = new string[Functions.Posters.Length];
                        bool Deleted = false;
                        int DeletedPosters = 0;
                        for (int Poster = Video - 1; Poster <= (Functions.Posters.Length - 1); Poster++)
                        {
                            Deleted = false;
                            for (int x = 0; x <= AlreadyDeleted.Length - 1; x++)
                            {
                                if (AlreadyDeleted[x] == Functions.Posters[Poster])
                                {
                                    Deleted = true;
                                    break; // TODO: might not be correct. Was : Exit For
                                }
                            }
                            if (!string.IsNullOrEmpty(Functions.Posters[Poster]) & Deleted == false)
                            {
                                File.Delete(Functions.Posters[Poster]);
                                AlreadyDeleted[DeletedPosters] = Functions.Posters[Poster];
                                DeletedPosters += 1;
                            }
                        }
                    }
                    break; // TODO: might not be correct. Was : Exit For
                }

                if (!HBFail)
                {

                    if (Functions.AutoTVMeta | Functions.AutoMoviePoster)
                    {
                        this.Title = "Batch Video Converter - Running Atomic Parsley";

                        // If arguments were generated run atomicparsley
                        if (Functions.AtomicArgs[Video - 1] != string.Empty)
                        {
                            using (Process atomic = new Process())
                            {
                                atomic.StartInfo.UseShellExecute = false;
                                atomic.StartInfo.CreateNoWindow = true;
                                atomic.StartInfo.RedirectStandardOutput = false;
                                atomic.StartInfo.FileName = Functions.APPath;
                                atomic.StartInfo.Arguments = Functions.AtomicArgs[Video - 1];
                                atomic.Start();
                                atomic.WaitForExit();
                            }
                        }

                        this.Title = "Batch Video Converter";

                        //Delete poster if it exists
                        if (!string.IsNullOrEmpty(Functions.Posters[Video - 1]))
                        {
                            if (Video == Functions.Posters.Length)
                            {
                                File.Delete(Functions.Posters[Video - 1]);
                            }
                            else
                            {
                                DeletePoster = true;
                                for (int x = Video; x <= Functions.Posters.Length - 1; x++)
                                {
                                    if (Functions.Posters[x] == Functions.Posters[Video - 1])
                                    {
                                        DeletePoster = false;
                                        break; // TODO: might not be correct. Was : Exit For
                                    }
                                }
                                if (DeletePoster)
                                {
                                    File.Delete(Functions.Posters[Video - 1]);
                                }
                            }
                        }

                    }

                    if (Functions.AutoImportiTunes)
                    {
                        this.Title = "Batch Video Converter - Importing Into iTunes";
                        Functions.iTunesExport(Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + OutExt);
                        if (Functions.AutoDeleteExport)
                        {
                            File.Delete(Functions.OutPath + "\\" + Functions.OutFile[Video - 1] + OutExt);
                        }
                    }
                }
                else
                {
                    if (Functions.Posters[Video - 1] != null)
                    {
                        File.Delete(Functions.Posters[Video - 1]);
                    }
                    foreach (string FileFound in Directory.GetFiles(Functions.OutPath, Functions.OutFile[Video - 1] + ".*." + OutExt))
                    {
                        File.Delete(FileFound);
                    }
                }

                //remove first item in list box
                lstVideos.Items.RemoveAt(0);
            }
            //if on Windows 7 or higher disable taskbar progress
            if (Functions.OsMajor >= 6 & Functions.OsMinor >= 1)
            {
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            }
            //set window name back to normal and enable all buttons and radio controls
            this.Title = "Batch Video Converter";
            btnAdd.IsEnabled = true;
            btnRemove.IsEnabled = true;
            btnClear.IsEnabled = true;
            btnSettings.IsEnabled = true;
            btnEncode.IsEnabled = true;
        }

        private void btnAdd_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFD = new OpenFileDialog();
            //set open file dialog properties

            openFD.Title = "Open a Video File";
            openFD.Filter = "Video Files (*.AVI;*.M2TS;*.M4V;*.MP4;*.MPG;*.MKV;*.TS)|*.AVI;*.M2TS;*.M4V;*.MP4;*.MPG;*.MKV;*.TS|All Files (*.*)|*.*";
            openFD.Multiselect = true;

            //if set use lastsource registry value to set the initial directory
            /*if ((Application.UserAppDataRegistry.GetValue("LastSource") != null))
            {
                openFD.InitialDirectory = Application.UserAppDataRegistry.GetValue("LastSource").ToString();
            }*/
            openFD.FilterIndex = 1;
            //show open file dialog and add files to list if ok is pressed
            Nullable<bool> result = openFD.ShowDialog();
            if (result == true)
            {
                foreach (string strFile in openFD.FileNames)
                {
                    lstVideos.Items.Add(strFile);
                }
                //add the last directory to the lastsource registry entry
                //Application.UserAppDataRegistry.SetValue("LastSource", Path.GetDirectoryName(openFD.FileName));
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            //remove selected item from list box

            if (lstVideos.SelectedIndex > -1)
            {
                lstVideos.Items.RemoveAt(lstVideos.SelectedIndex);
            }
            else
            {
                MessageBox.Show("Please select a file to remove!");
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            //clear the list box
            lstVideos.Items.Clear();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //set killhb to true if the cancel button is pressed
            KillHB = true;
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            //make sure an item is selected and not the first
            if (lstVideos.SelectedIndex > 0)
            {
                //get selected item and its index
                object objSelectedItem = lstVideos.SelectedItem;
                int intIndex = lstVideos.SelectedIndex;

                //remove the item that you want to move
                lstVideos.Items.RemoveAt(intIndex);

                //decrement index
                intIndex -= 1;

                //insert item at new index
                lstVideos.Items.Insert(intIndex, objSelectedItem);

                //select item
                lstVideos.SelectedIndex = intIndex;
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            //make sure an item is selected and not the last
            if (lstVideos.SelectedIndex > -1 & lstVideos.SelectedIndex < (lstVideos.Items.Count - 1))
            {
                //get selected item and its index
                object objSelectedItem = lstVideos.SelectedItem;
                int intIndex = lstVideos.SelectedIndex;

                //remove the item that you want to move
                lstVideos.Items.RemoveAt(intIndex);

                //increment index
                intIndex += 1;

                //insert item at new index
                lstVideos.Items.Insert(intIndex, objSelectedItem);

                //select item
                lstVideos.SelectedIndex = intIndex;
            }
        }
        
        private void OnClosed(object sender, EventArgs e)
        {
            Point position = new Point(this.Left,this.Top);

            if (position.X != xpos | position.Y != ypos)
            {
                Registry.SetValue(registryPath, "XPos", Convert.ToString(position.X));
                Registry.SetValue(registryPath, "YPos", Convert.ToString(position.Y));
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            frmSettings Settings = new frmSettings();

            Settings.ShowDialog();
        }
    }
}
