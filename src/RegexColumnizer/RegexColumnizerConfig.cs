using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace RegexColumnizer
{
    [Serializable]
    public class RegexColumnizerConfig
    {
        [DefaultValue("true")]
        public bool LocalTimestamps
        {
            get;
            set;
        }

        public Regex Regex
        {
            get;
            set;
        }
 

        [DefaultValue("")]
        public string TimestampField
        {
            get;
            set;
        }

        [DefaultValue("Test")]
        public string TimestampFormat
        {
            get;
            set;
        }

        public string[] SelectedFields
        {
            get; 
            set;
        }

        public RegexColumnizerConfig()
        {
            Regex = new Regex("(?<one>.*)", RegexOptions.IgnoreCase);
            SelectedFields = new string[] {"one"};
            TimestampField = "";
            TimestampFormat = "";

        }

        internal bool IsValid()
        {
            return
                Regex != null
                && SelectedFields != null
                && TimestampField != null
                && TimestampFormat != null;
        }
    }
}
