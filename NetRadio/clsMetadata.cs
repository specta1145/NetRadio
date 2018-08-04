using System;
using System.IO;
using System.Net;
using System.Text;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NetRadio
{
    class ClsMetadata
    {// The metadata for SHOUTcast/Icecast streams is not in the headers, but in the actual stream itself
        public static Dictionary<string, string> GetMetdata(string url)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string result = string.Empty;
            //string serverPath = "/";
            try
            {// https://gist.github.com/markheath/3301840
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 2000;
                request.KeepAlive = false;
                request.Headers.Clear();
                //request.Headers.Add("GET", "/ HTTP/1.0");
                request.UserAgent = "WinampMPEG/5.09";
                request.Headers.Add("Icy-MetaData", "1");
                using (WebResponse fifo = request.GetResponse())
                {
                    if (fifo != null)
                    {
                        result = fifo.Headers.ToString();
                        fifo.Close();
                    }
                }
            }
            catch (WebException ex)
            {//MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (ex.Status == WebExceptionStatus.Timeout)
                {// The operation has timed out.
                    dictionary.Add("error", "");
                }
                else
                {
                    dictionary.Add("error", ex.Message);
                }
                return dictionary;
            } //MessageBox.Show(result);
            dictionary.Add("description", Regex.Match(result, @"description: (.*)").Groups[1].Value);
            dictionary.Add("bitrate", Regex.Match(result, @"bitrate=(\d*)").Groups[1].Value);
            dictionary.Add("samplerate", Regex.Match(result, @"samplerate=(\d*)").Groups[1].Value);
            dictionary.Add("codec", Regex.Match(result, @"audio/(\w*)").Groups[1].Value);
            return dictionary;
        }

        public static string GetSongTilte(string url)
        {
            int metaInt = 0; // blocksize of mp3 data
            int metadataLength = 0; // length of metadata header
            string metadataHeader = ""; // metadata header that contains the actual songtitle
            string result = string.Empty;
            //string serverPath = "/";
            Stream socketStream = null;	// input stream on the web request
            byte[] buffer = new byte[512]; // receive buffer
            int count = 0; // byte counter
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request = (HttpWebRequest)WebRequest.Create(url);  // create web request
            request.Headers.Clear(); // clear old request header and build own header to receive ICY-metadata
            request.Timeout = 3000;
            request.KeepAlive = false;
            //request.Headers.Add("GET", serverPath + " HTTP/1.0");
            request.Headers.Add("Icy-MetaData", "1"); // needed to receive metadata informations
            request.UserAgent = "WinampMPEG/5.09";
            HttpWebResponse response = null; // web response
            try
            {
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    if (response != null)
                    {
                        if (!int.TryParse(Regex.Match(response.Headers.ToString(), @"icy-metaint: (.*)").Groups[1].Value, out metaInt))
                        {//  metaInt = Convert.ToInt32(response.GetResponseHeader("icy-metaint")); // 16000
                            response.Close();
                            return string.Empty;
                        }
                        using (socketStream = response.GetResponseStream())
                        {
                            while (true)
                            {
                                int bufLen = socketStream.Read(buffer, 0, buffer.Length);
                                for (int i = 0; i < bufLen; i++)
                                {
                                    if (metadataLength != 0)
                                    {
                                        metadataHeader += Convert.ToChar(buffer[i]);
                                        metadataLength--;
                                        if (metadataLength == 0) // all metadata informations were written to the 'metadataHeader' string
                                        {
                                            //MessageBox.Show("mdlength: " + metadataLength.ToString() + " | metaInt: " + metaInt + " | count: " + count+ Environment.NewLine + metadataHeader);
                                            result = Regex.Match(metadataHeader, @"StreamTitle='(.*?)';.*").Groups[1].Value.Trim();
                                            // (.*) ... to stop at the first possible character, follow them with a question mark
                                            response.Close();
                                            socketStream.Close();
                                            return result;
                                        }
                                    }
                                    else if (count++ == metaInt)
                                    {// get headerlength from lengthbyte and multiply by 16 to get correct headerlength
                                        metadataLength = Convert.ToInt32(buffer[i]) * 16;
                                        count = 0;
                                    }
                                }
                            } //while
                        }
                    }
                }
            }
            catch (WebException ex)
            {// MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation); // ist nicht modal
                return "_" + ex.Message;
            }
            finally
            {
                if (response != null)
                    response.Close();
                if (socketStream != null)
                    socketStream.Close();
            }
            return result;
        }

    }
}
