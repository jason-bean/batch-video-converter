using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using iTunesLib;

namespace Batch_Video_Converter
{
       
    public static class Functions
    {
        
        private static string _HBPath;
        private static string _mp4boxPath;
        private static string _APPath;
        private static string _OutPath;
        private static bool _RemoveAds;
        private static bool _AutoCrop;
        private static bool _AutoTVMeta;
        private static bool _AutoMoviePoster;
        private static bool _AutoImportiTunes;
        private static bool _AutoDeleteExport;
        private static int _OsMajor;
        private static int _OsMinor;
        private static string _TempDir;
        private static string[] _AtomicArgs;
        private static string[] _OutFile;
        private static string[] _Posters;
        private static string _SearchedTitle;
        private static string[,] _Selections;
        private static string[] _SelectedID = new string[2];
        private static string _tvdbAPIID = "6111D5FCB5CFCAEF";
        private static string _tmdbAPIKey = "765da88acb6888be871b5ffd1c4a80fa";
        private static string[] _EncodeProfiles = { "iPhone/iPod Touch (480x320)", "iPhone 4/AppleTV (960x640) Anamorphic", "iPad/iPad 2/AppleTV 2 (1280x720) Anamorphic", "iPod (320x240)", "Android Mid (480x320)", "Android High (720x405)", "Android High (1280x720)" };
        private static int _DefaultEncodeProfile;

        public static string HBPath
        {
            get { return _HBPath; }
            set { _HBPath = value; }
        }
        public static string mp4boxPath
        {
            get { return _mp4boxPath; }
            set { _mp4boxPath = value; }
        }
        public static string APPath
        {
            get { return _APPath; }
            set { _APPath = value; }
        }
        public static string OutPath
        {
            get { return _OutPath; }
            set { _OutPath = value; }
        }
        public static bool RemoveAds
        {
            get { return _RemoveAds; }
            set { _RemoveAds = value; }
        }
        public static bool AutoCrop
        {
            get { return _AutoCrop; }
            set { _AutoCrop = value; }
        }
        public static bool AutoTVMeta
        {
            get { return _AutoTVMeta; }
            set { _AutoTVMeta = value; }
        }
        public static bool AutoMoviePoster
        {
            get { return _AutoMoviePoster; }
            set { _AutoMoviePoster = value; }
        }
        public static bool AutoImportiTunes
        {
            get { return _AutoImportiTunes; }
            set { _AutoImportiTunes = value; }
        }
        public static bool AutoDeleteExport
        {
            get { return _AutoDeleteExport; }
            set { _AutoDeleteExport = value; }
        }
        public static int OsMajor
        {
            get { return _OsMajor; }
            set { _OsMajor = value; }
        }
        public static int OsMinor
        {
            get { return _OsMinor; }
            set { _OsMinor = value; }
        }
        public static string TempDir
        {
            get { return _TempDir; }
            set { _TempDir = value; }
        }
        public static string[] AtomicArgs
        {
            get { return _AtomicArgs; }
            set { _AtomicArgs = value; }
        }
        public static string[] OutFile
        {
            get { return _OutFile; }
            set { _OutFile = value; }
        }
        public static string[] Posters
        {
            get { return _Posters; }
            set { _Posters = value; }
        }
        public static string SearchedTitle
        {
            get { return _SearchedTitle; }
            set { _SearchedTitle = value; }
        }
        public static string[,] Selections
        {
            get { return _Selections; }
            set { _Selections = value; }
        }
        public static string[] SelectedID
        {
            get { return _SelectedID; }
            set { _SelectedID = value; }
        }
        public static string tvdbAPIID
        {
            get { return _tvdbAPIID; }
            set { _tvdbAPIID = value; }
        }
        public static string tmdbAPIKey
        {
            get { return _tmdbAPIKey; }
            set { _tmdbAPIKey = value; }
        }
        public static string[] EncodeProfiles
        {
            get { return _EncodeProfiles; }
        }
        public static int DefaultEncodeProfile
        {
            get { return _DefaultEncodeProfile; }
            set { _DefaultEncodeProfile = value; }
        }

        public static byte[] GetWebData(string URL)
        {
            WebRequest Req = default(WebRequest);
            System.IO.Stream SourceStream = default(System.IO.Stream);
            WebResponse Response = default(WebResponse);
            MemoryStream TempStream = new MemoryStream();

            try
            {
                //create a web request to the URL  
                Req = HttpWebRequest.Create(URL);

                //get a response from web site  
                Response = Req.GetResponse();

                //Source stream with requested document  
                SourceStream = Response.GetResponseStream();

                //SourceStream has no ReadAll, so we must read data block-by-block  
                //Temporary Buffer and block size  
                byte[] Buffer = new byte[4097];
                int BlockSize = 0;

                //Memory stream to store data  
                
                do
                {
                    BlockSize = SourceStream.Read(Buffer, 0, 4096);
                    if (BlockSize > 0)
                        TempStream.Write(Buffer, 0, BlockSize);
                } while (BlockSize > 0);

                //return the document binary data  
                return TempStream.ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " - " + URL, "GetDataBin");
                return TempStream.ToArray();
                //errdw("GetURLDataBin", ex.Message, URL)
            }
            finally
            {
                //grrr... Using is great, but the command is not in VB.Net  
                SourceStream.Close();
                Response.Close();
            }
        }

        public static string[] GetMetaData(string FileName)
        {
            FileStream readfile = new FileStream(FileName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
            int Character = 0;
            string findtemp = "";
            long Length = 0;
            long Position = 0;
            string find = "";
            string[] MetaData = null;
            string[] DataLine = null;
            string[] FileData = new string[6];
            //byte[] findbyte = null;

            //Get length of video file and read the last 8kb to capture SageTV metadata
            //readfile = File.OpenRead(FileName);
            Length = readfile.Length;
            Position = Length - 8000;
            //readfile.Position = (Position);
            readfile.Seek(Position, SeekOrigin.Begin);

            //Read last 8kb of file byte by byte and convert it to ASCII into a string
            for (long x = Position; x <= (Length - 1); x++)
            {
                //findbyte[0] = readfile.ReadByte();
                find += (char)readfile.ReadByte();
            }

            //Check to make sure the captured data contains metadata
            if (find.Contains("META"))
            {
                //Strip occurances of "Gÿ" in the string
                find = find.Replace("G\u001fÿ", "");
                find = find.Replace("\\;", ",");
                //cut beginning and end of string to include only the metadata
                find = find.Substring((find.IndexOf("META") + 4), (find.Length - find.IndexOf("META") - 4));
                find = find.Substring(0, (find.LastIndexOf(';') + 1));
                //strip out any unusual characters from string
                for (int x = 0; x <= (find.Length - 1); x++)
                {
                    Character = Convert.ToInt32(find.Substring(x, 1).ToCharArray()[0]);
                    if ((Character > 31 & Character < 127) | (Character > 127 & Character < 166))
                    {
                        findtemp += (char)Character;
                    }
                }
                find = findtemp;
                //Split captured data by the end of line character ";"
                MetaData = find.Split(';');
                for (int x = 0; x <= (MetaData.Length - 1); x++)
                {
                    //Split data even further by "=" and then capture the Title and ID
                    DataLine = MetaData[x].Split('=');
                    if (DataLine[0] == "Title")
                    {
                        FileData[0] = DataLine[1];
                    }
                    else if (DataLine[0] == "ExternalID")
                    {
                        FileData[2] = DataLine[1];
                    }
                }
                //If the ID contains "EP" it is a TV show, capture episode name, season number and episode number
                if (FileData[2].Contains("EP"))
                {
                    for (int x = 0; x <= (MetaData.Length - 1); x++)
                    {
                        DataLine = MetaData[x].Split('=');
                        if (DataLine[0] == "EpisodeName")
                        {
                            FileData[1] = DataLine[1];
                        }
                        if (DataLine[0] == "SeasonNumber")
                        {
                            FileData[3] = DataLine[1];
                        }
                        if (DataLine[0] == "EpisodeNumber")
                        {
                            FileData[4] = DataLine[1];
                        }
                    }
                    //Convert ID format
                    if (FileData[2].IndexOf("EP") != 0)
                    {
                        //FileData[2] = Strings.Right(FileData[2], (FileData[2].Length - (FileData[2].IndexOf("EP"))));
                        FileData[2] = FileData[2].Substring(FileData[2].IndexOf("EP"));
                    }
                    //If ID starts with MV capture the year of the movie
                }
                else if (FileData[2].Contains("MV"))
                {
                    for (int x = 0; x <= (MetaData.Length - 1); x++)
                    {
                        DataLine = MetaData[x].Split('=');
                        if (DataLine[0] == "Year")
                        {
                            FileData[5] = DataLine[1];
                        }
                    }
                }
            }

            //Close the file and return file metadata
            readfile.Close();
            return FileData;
        }

        public static long GetSeriesID(string SeriesName, string RecEpID = "0")
        {
            byte[] bytearray = null;
            FileStream xmldata = default(FileStream);
            string url = null;
            string Title = null;
            string SeriesID = "0";
            long RecZapID = 0;
            //string zapID = "";
            XPathDocument xpathDoc;
            XPathNavigator xmlNav;
            XPathNodeIterator xmlNI;
            frmTitleSelect TitleSelect = new frmTitleSelect();

            //Replace spaces in title with +'s, generate URL, and get XML data
            Title = SeriesName.Replace(" ", "+");
            url = "http://www.thetvdb.com/api/GetSeries.php?seriesname=" + Title + "&language=en";
            bytearray = GetWebData(url);

            //Write XML data to file
            xmldata = new FileStream(TempDir + "temp.xml", FileMode.Create);
            xmldata.Write(bytearray, 0, bytearray.Length);
            xmldata.Close();

            //Open file with XPath
            xpathDoc = new XPathDocument(@TempDir + "temp.xml");
            xmlNav = xpathDoc.CreateNavigator();

            //If there is no ID from file metadata exactly match title to get TVDB Series ID
            if (RecEpID == "0")
            {
                //xmlNI = xmlNav.Select("//SeriesName[. = '" & SeriesName & "']/parent::node()/seriesid")
                xmlNI = xmlNav.Select("/Data/Series[contains(SeriesName,\"" + SeriesName + "\")]/seriesid");

                if (xmlNI.Count > 0)
                {
                    if (xmlNI.Count == 1)
                    {
                        xmlNI.MoveNext();
                        SeriesID = xmlNI.Current.Value;
                    }
                    else
                    {
                        SelectedID[0] = string.Empty;
                        xmlNI = xmlNav.Select("/Data/Series/SeriesName");
                        Selections = new string[xmlNI.Count, 3];

                        int x = 0;
                        while (xmlNI.MoveNext())
                        {
                            Selections[x, 0] = xmlNI.Current.Value;
                            x += 1;
                        }
                        xmlNI = xmlNav.Select("/Data/Series/FirstAired");
                        x = 0;
                        while (xmlNI.MoveNext())
                        {
                            Selections[x, 1] = xmlNI.Current.Value.Substring(0, 4);
                            x += 1;
                        }
                        xmlNI = xmlNav.Select("/Data/Series/seriesid");
                        x = 0;
                        while (xmlNI.MoveNext())
                        {
                            Selections[x, 2] = xmlNI.Current.Value;
                            x += 1;
                        }
                        SearchedTitle = "File Title: " + SeriesName;
                        TitleSelect.ShowDialog();
                        if (SelectedID[0] != string.Empty)
                        {
                            SeriesID = SelectedID[0];
                        }
                    }
                }

                //Otherwise try and match the zap2it ID to the recording's ID to get the correct TVDB Series ID
            }
            else
            {
                RecZapID = Convert.ToInt64(RecEpID.Substring(2, (RecEpID.Length - 2)).Substring(0, (RecEpID.Length - 2 - 4)));

                xmlNI = xmlNav.Select("/Data/Series[contains(zap2it_id,'" + RecZapID.ToString() + "')]/seriesid");

                if (xmlNI.Count > 1)
                {
                    //xmlNI = xmlNav.Select("//SeriesName[. = '" & SeriesName & "']/parent::node()/seriesid")
                    xmlNI = xmlNav.Select("/Data/Series[contains(SeriesName,\"" + SeriesName + "\")]/seriesid");
                    if (xmlNI.Count == 1)
                    {
                        xmlNI.MoveNext();
                        SeriesID = xmlNI.Current.Value;
                    }
                    else
                    {
                        SelectedID[0] = string.Empty;
                        xmlNI = xmlNav.Select("/Data/Series[contains(zap2it_id,'" + RecZapID.ToString() + "')]/SeriesName");
                        Selections = new string[xmlNI.Count, 3];

                        int x = 0;
                        while (xmlNI.MoveNext())
                        {
                            Selections[x, 0] = xmlNI.Current.Value;
                            x += 1;
                        }
                        xmlNI = xmlNav.Select("/Data/Series[contains(zap2it_id,'" + RecZapID.ToString() + "')]/FirstAired");
                        x = 0;
                        while (xmlNI.MoveNext())
                        {
                            Selections[x, 1] = xmlNI.Current.Value.Substring(0, 4);
                            x += 1;
                        }
                        xmlNI = xmlNav.Select("/Data/Series[contains(zap2it_id,'" + RecZapID.ToString() + "')]/seriesid");
                        x = 0;
                        while (xmlNI.MoveNext())
                        {
                            Selections[x, 2] = xmlNI.Current.Value;
                            x += 1;
                        }
                        SearchedTitle = "File Title: " + SeriesName;
                        TitleSelect.ShowDialog();
                        if (SelectedID[0] != string.Empty)
                        {
                            SeriesID = SelectedID[0];
                        }
                    }
                }
                else if (xmlNI.Count == 1)
                {
                    xmlNI.MoveNext();
                    SeriesID = xmlNI.Current.Value;
                }
                else if (xmlNI.Count == 0)
                {
                    SeriesName = SeriesName.ToLower();
                    xmlNI = xmlNav.Select("/Data/Series[translate(SeriesName,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') = \"" + SeriesName + "\"]/seriesid");
                    if (xmlNI.Count > 0)
                    {
                        xmlNI.MoveNext();
                        SeriesID = xmlNI.Current.Value;
                    }
                }
            }

            if (string.IsNullOrEmpty(SeriesID))
            {
                SeriesID = "0";
            }

            //Delete temporary XML file and return the TVDB series ID
            File.Delete(TempDir + "temp.xml");
            return Convert.ToInt64(SeriesID);
        }

        public static int[] GetEpisodeData(string EpisodeName, long SeriesID)
        {
            string url = null;
            byte[] SeriesData = null;
            FileStream SeriesXML = default(FileStream);
            int[] EpisodeData = new int[2];
            XPathDocument xpathDoc;
            XPathNavigator xmlNav;
            XPathNodeIterator xmlNI;

            //Use TVDB Series ID to get all episode data for the series
            url = "http://www.thetvdb.com/api/" + tvdbAPIID + "/series/" + SeriesID.ToString() + "/all/en.xml";
            SeriesData = GetWebData(url);

            //Write XML to file
            SeriesXML = new FileStream(TempDir + "temp.xml", FileMode.Create);
            SeriesXML.Write(SeriesData, 0, SeriesData.Length);
            SeriesXML.Close();

            //Search XML for correct season number and episode number using episode name
            xpathDoc = new XPathDocument(TempDir + "temp.xml");
            xmlNav = xpathDoc.CreateNavigator();
            xmlNI = xmlNav.Select("/Data/Episode[contains(EpisodeName,\"" + EpisodeName + "\")]/SeasonNumber");
            while (xmlNI.MoveNext())
            {
                if (Convert.ToInt32(xmlNI.Current.Value) != 0)
                {
                    EpisodeData[0] = Convert.ToInt32(xmlNI.Current.Value);
                }
            }
            xmlNI = xmlNav.Select("/Data/Episode[contains(EpisodeName,\"" + EpisodeName + "\")]/EpisodeNumber");
            while (xmlNI.MoveNext())
            {
                if (Convert.ToInt32(xmlNI.Current.Value) != 0)
                {
                    EpisodeData[1] = Convert.ToInt32(xmlNI.Current.Value);
                }
            }

            //Delete xml file and return season number & episode number
            File.Delete(TempDir + "temp.xml");

            return EpisodeData;
        }

        public static string GetTVPoster(long SeriesID, int Season)
        {
            byte[] BannerData = null;
            FileStream BannerXML = default(FileStream);
            string BannerPath = null;
            string url = null;
            byte[] PosterData = null;
            string[] BannerSplit = null;
            string FileName = "";
            FileStream PosterWrite = default(FileStream);
            XPathDocument xpathDoc = default(XPathDocument);
            XPathNavigator xmlNav = default(XPathNavigator);
            XPathNodeIterator xmlNI = default(XPathNodeIterator);

            //Use TVDB series ID to get the banners.xml file
            url = "http://www.thetvdb.com/api/" + tvdbAPIID + "/series/" + SeriesID.ToString() + "/banners.xml";
            BannerData = GetWebData(url);

            //Write XML to file
            BannerXML = new FileStream(TempDir + "temp.xml", FileMode.Create);
            BannerXML.Write(BannerData, 0, BannerData.Length);
            BannerXML.Close();

            //Open XML file and get the path to all files with the banner type "poster"
            xpathDoc = new XPathDocument(TempDir + "temp.xml");
            xmlNav = xpathDoc.CreateNavigator();
            //xmlNI = xmlNav.Select("/Banners/Banner[BannerType = 'poster']/BannerPath")
            xmlNI = xmlNav.Select("/Banners/Banner[BannerType = 'season' and BannerType2 = 'season' and Season = '" + Season + "']/BannerPath");

            //Download poster and write file to temp folder
            if (xmlNI.Count > 0)
            {
                //Get only the path to the first poster
                xmlNI.MoveNext();
                BannerPath = xmlNI.Current.Value;
                url = "http://www.thetvdb.com/banners/" + BannerPath;
                BannerSplit = BannerPath.Split('/');
                FileName = TempDir + BannerSplit[1];
                if (!File.Exists(FileName))
                {
                    PosterData = GetWebData(url);
                    PosterWrite = new FileStream(FileName, FileMode.Create);
                    PosterWrite.Write(PosterData, 0, PosterData.Length);
                    PosterWrite.Close();
                }
            }
            else
            {
                xmlNI = xmlNav.Select("/Banners/Banner[BannerType = 'poster']/BannerPath");

                if (xmlNI.Count > 0)
                {
                    //Get only the path to the first poster
                    xmlNI.MoveNext();
                    BannerPath = xmlNI.Current.Value;
                    url = "http://www.thetvdb.com/banners/" + BannerPath;
                    BannerSplit = BannerPath.Split('/');
                    FileName = TempDir + BannerSplit[1];
                    if (!File.Exists(FileName))
                    {
                        PosterData = GetWebData(url);
                        PosterWrite = new FileStream(FileName, FileMode.Create);
                        PosterWrite.Write(PosterData, 0, PosterData.Length);
                        PosterWrite.Close();
                    }
                }
            }

            //Delete XML temp file and return the poster filename
            File.Delete(TempDir + "temp.xml");
            return FileName;
        }

        public static string[] GetMovieID(string Title, string Year = "")
        {
            string URLTitle = null;
            string url = null;
            byte[] MovieData = null;
            FileStream FileWrite = default(FileStream);
            XPathDocument xpathDoc = default(XPathDocument);
            XPathNavigator xmlNav = default(XPathNavigator);
            XmlNamespaceManager nsMgr = default(XmlNamespaceManager);
            XPathNodeIterator xmlNI = default(XPathNodeIterator);
            int Result = 0;
            string[] TMDbID = {"",string.Empty};
            frmTitleSelect TitleSelect = new frmTitleSelect();

            //Replace spaces in movie title with +'s and, create URL, and get data
            URLTitle = Title.Replace(" ", "+");
            url = "http://api.themoviedb.org/2.1/Movie.search/en/xml/" + tmdbAPIKey + "/" + URLTitle;
            MovieData = GetWebData(url);

            //Write data to temporary XML file
            FileWrite = new FileStream(TempDir + "temp.xml", FileMode.Create);
            FileWrite.Write(MovieData, 0, MovieData.Length);
            FileWrite.Close();

            //Open XML file
            xpathDoc = new XPathDocument(TempDir + "temp.xml");
            xmlNav = xpathDoc.CreateNavigator();
            nsMgr = new XmlNamespaceManager(xmlNav.NameTable);
            nsMgr.AddNamespace("opensearch", "http://a9.com/-/spec/opensearch/1.1/");

            //If the year of the movie was captured use the Title and Year to search XML
            if (!string.IsNullOrEmpty(Year))
            {
                //Check to make sure there were results first
                xmlNI = xmlNav.Select("/OpenSearchDescription/opensearch:totalResults", nsMgr);
                xmlNI.MoveNext();
                Result = Convert.ToInt32(xmlNI.Current.Value);
                //If Result <> "Nothing found." Then
                if (Result > 0)
                {
                    xmlNI = xmlNav.Select("/OpenSearchDescription/movies/movie[contains(name,\"" + Title + "\") and contains(released,'" + Year + "')]/id");
                }
                else
                {
                    return TMDbID;                    
                }
                //Otherwise get all TMDB ID's
            }
            else
            {
                //Check to make sure there were results first
                xmlNI = xmlNav.Select("/OpenSearchDescription/opensearch:totalResults", nsMgr);
                xmlNI.MoveNext();
                Result = Convert.ToInt32(xmlNI.Current.Value);
                //If Result <> "Nothing found." Then
                if (Result > 0)
                {
                    //xmlNI = xmlNav.Select("/OpenSearchDescription/movies/movie/name[. = """ & Title & """]/parent::node()/id")
                    xmlNI = xmlNav.Select("/OpenSearchDescription/movies/movie[name = \"" + Title + "\"]/id");
                }
                else
                {
                    return TMDbID;
                }
            }

            //Only get the TMDB ID if there is one result, otherwise we don't want to get the wrong ID
            if (xmlNI.Count == 1)
            {
                while (xmlNI.MoveNext())
                {
                    TMDbID[0] = xmlNI.Current.Value;
                }
            }
            else
            {
                SelectedID[0] = string.Empty;
                
                xmlNI = xmlNav.Select("/OpenSearchDescription/movies/movie/name");
                Selections = new string[xmlNI.Count, 3];

                int x = 0;
                while (xmlNI.MoveNext())
                {
                    Selections[x, 0] = xmlNI.Current.Value;
                    x += 1;
                }
                xmlNI = xmlNav.Select("/OpenSearchDescription/movies/movie/released");
                x = 0;
                while (xmlNI.MoveNext())
                {
                    Selections[x, 1] = xmlNI.Current.Value;
                    x += 1;
                }
                xmlNI = xmlNav.Select("/OpenSearchDescription/movies/movie/id");
                x = 0;
                while (xmlNI.MoveNext())
                {
                    Selections[x, 2] = xmlNI.Current.Value;
                    x += 1;
                }
                SearchedTitle = "File Title: " + Title;
                TitleSelect.ShowDialog();
                if (SelectedID[0] != string.Empty)
                {
                    TMDbID = SelectedID;
                }
            }
            //Delete temp file
            File.Delete(TempDir + "temp.xml");
            //Return the TMDB ID
            return TMDbID;
        }

        public static string GetMoviePoster(string TMDbID)
        {
            string url = null;
            byte[] PosterData = null;
            FileStream FileWrite = default(FileStream);
            XPathDocument xpathDoc = default(XPathDocument);
            XPathNavigator xmlNav = default(XPathNavigator);
            XPathNodeIterator xmlNI = default(XPathNodeIterator);
            string PosterURL = null;
            string[] PosterSplit = null;
            string FileName = null;
            FileStream PosterWrite = default(FileStream);

            //Use the TMDB ID to get the list of images for the movie
            url = "http://api.themoviedb.org/2.1/Movie.getImages/en/xml/" + tmdbAPIKey + "/" + TMDbID;
            PosterData = GetWebData(url);

            //Write XML to file
            FileWrite = new FileStream(TempDir + "temp.xml", FileMode.Create);
            FileWrite.Write(PosterData, 0, PosterData.Length);
            FileWrite.Close();

            //Open XML file and get the URL's for all poster images
            xpathDoc = new XPathDocument(TempDir + "temp.xml");
            xmlNav = xpathDoc.CreateNavigator();
            xmlNI = xmlNav.Select("/OpenSearchDescription/movies/movie/images/poster/image[@size='mid']/@url");

            if (xmlNI.Count > 0)
            {
                //Get the URL for the first poster image
                xmlNI.MoveNext();
                PosterURL = xmlNI.Current.Value;

                //Delete Temp file
                File.Delete(TempDir + "temp.xml");

                //Get poster file
                PosterData = GetWebData(PosterURL);

                //Get only filename using the poster URL
                PosterSplit = PosterURL.Split('/');
                FileName = TempDir + PosterSplit[PosterSplit.Length - 1];

                //Write downloaded file to temp folder
                PosterWrite = new FileStream(FileName, FileMode.Create);
                PosterWrite.Write(PosterData, 0, PosterData.Length);
                PosterWrite.Close();
            }
            else
            {
                FileName = "";
            }

            //Return name of poster file
            return FileName;
        }

        public static void iTunesExport(string FilePath)
        {
            iTunesAppClass iTunes = new iTunesAppClass();
            
            //iTunesApp iTunes = new iTunesApp();
            IITLibraryPlaylist Tracks = iTunes.LibraryPlaylist;
            var AddVideo = Tracks.AddFile(FilePath);
            do
            {
            } while (AddVideo.InProgress == true);
            Marshal.FinalReleaseComObject(iTunes);
        }

    }
}
