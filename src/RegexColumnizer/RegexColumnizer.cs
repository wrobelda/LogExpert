using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace RegexColumnizer
{

    public class RegexColumnizer : IColumnizerConfigurator, ILogLineColumnizer, IInitColumnizer
    {
        ///// <summary>
        ///// Implement this property to let LogExpert display the name of the Columnizer
        ///// in its Colummnizer selection dialog.
        ///// </summary>
        public string Text
        {
            get { return GetName(); }
        }

        private RegexColumnizerConfig config = new RegexColumnizerConfig();
        protected int timeOffset = 0;
        private string name="";
        protected string configDir;


        #region IColumnizerConfigurator Members

        public void Configure(ILogLineColumnizerCallback callback, string configDir)
        {
            string configPath = configDir + @"\Regexcolumnizer-" + name + "." + ".dat";
            RegexColumnizerConfigDlg dlg = new RegexColumnizerConfigDlg(this.config);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                dlg.Apply(config);
                BinaryFormatter formatter = new BinaryFormatter();
                Stream fs = new FileStream(configPath, FileMode.Create, FileAccess.Write);
                formatter.Serialize(fs, this.config);
                fs.Close();
            }
        }


        public void LoadConfig(string configDir)
        {
            this.configDir = configDir;
        }

        #endregion

        #region ILogLineColumnizer Members

        public int GetColumnCount()
        {
            return config.SelectedFields.Length;
        }

        public string[] GetColumnNames()
        {
            return config.SelectedFields;
        }

        public string GetDescription()
        {
            return "Regex Columnizer";
        }

        public string GetName()
        {
            return "RegexColumnizer";
        }

        public int GetTimeOffset()
        {
            return this.timeOffset;
        }

        public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
        {
      
            Match m = config.Regex.Match(line);

            DateTime timestamp;
            if (config.TimestampField.Length == 0 || config.TimestampFormat.Length == 0 || !m.Success || !m.Groups[config.TimestampField].Success || !DateTime.TryParseExact(
                m.Groups[config.TimestampField].Value, config.TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
            {
                timestamp = DateTime.MinValue;
            }
            else
            {
                if (config.LocalTimestamps)
                    timestamp = timestamp.ToLocalTime();
                timestamp = timestamp.AddMilliseconds(timeOffset);
            }

            return timestamp;
        }

        public bool IsTimeshiftImplemented()
        {
            return true;
        }

        public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
        {
            if (column >= 0 && column < GetColumnCount() && GetColumnNames()[column].Equals(config.TimestampField))
            {
                try
                {
                    DateTime newDateTime = DateTime.ParseExact(value, config.TimestampFormat, CultureInfo.InvariantCulture);
                    DateTime oldDateTime = DateTime.ParseExact(oldValue, config.TimestampFormat, CultureInfo.InvariantCulture);
                    long mSecsOld = oldDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                    long mSecsNew = newDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                    this.timeOffset = (int)(mSecsNew - mSecsOld);
                }
                catch (FormatException)
                { }
            }
        }

        public void SetTimeOffset(int msecOffset)
        {
            this.timeOffset = msecOffset;
        }

        public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
        {
            
            string[] col = new string[GetColumnCount()];
            
            Match m = config.Regex.Match(line);
            if(m.Success)
            {
                int i = 0;
                foreach (string column in GetColumnNames())
                {
                    col[i++] = m.Groups[column].Success ? m.Groups[column].Value : "";
                }
                DateTime timeStamp=GetTimestamp(callback, line);
                if (timeStamp != DateTime.MinValue)
                {
                    for(i=0;i<config.SelectedFields.Length;i++)
                    {
                        if(config.TimestampField.Equals(config.SelectedFields[i]))
                        {
                            col[i] = timeStamp.ToString(config.TimestampFormat);
                            break;
                        }
                    } 
                }
            }
            else
            {
                for(int i=0; i<col.Length; i++)
                    col[i]="";
            }
            return col;
        }

        #endregion

        #region IInitColumnizer Members

        public void DeSelected(ILogLineColumnizerCallback callback)
        {
            //throw new NotImplementedException();
        }

        public void Selected(ILogLineColumnizerCallback callback)
        {
            FileInfo fi = new FileInfo(callback.GetFileName());

            name = BitConverter.ToString(new MD5CryptoServiceProvider().
                ComputeHash(Encoding.Unicode.GetBytes(fi.FullName))).Replace("-", "").ToLower();


            string configPath = configDir + @"\Regexcolumnizer-" + name + "." + ".dat";

            if (!File.Exists(configPath))
            {
                this.config = new RegexColumnizerConfig();
            }
            else
            {
                Stream fs = File.OpenRead(configPath);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    
                    
                    RegexColumnizerConfig config = (RegexColumnizerConfig)formatter.Deserialize(fs);
                    if (config.IsValid())
                    {
                        this.config = config;
                    }
                }
                catch (SerializationException e)
                {
                    MessageBox.Show(e.Message, "Deserialize");
                    this.config = new RegexColumnizerConfig();
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        public IColumnizedLogLine SplitLine(ILogLineColumnizerCallback callback, ILogLine line)
        {
            throw new NotImplementedException();
        }

        public DateTime GetTimestamp(ILogLineColumnizerCallback callback, ILogLine line)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
