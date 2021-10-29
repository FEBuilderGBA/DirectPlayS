using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DirectPlayS
{
    class Config : Dictionary<string, string>
    {
        string ConfigFilename;
        public void Save()
        {
            Save(this.ConfigFilename);
        }

        public void Save(string fullfilename)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in this)
            {
                sb.Append(pair.Key);
                sb.Append("\t");
                sb.AppendLine(pair.Value);
            }

            try
            {
                File.WriteAllText(fullfilename, sb.ToString());
            }
            catch(Exception)
            {
            }
        }
        public void Load()
        {
            string conf = Path.Combine(Program.BaseDirectory, "config.txt");
            Load(conf);
        }

        public void Load(string fullfilename)
        {
            this.ConfigFilename = fullfilename;
            if (!System.IO.File.Exists(fullfilename))
            {
                return;
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(fullfilename);
            }
            catch(Exception)
            {
                return;
            }

            foreach (string line in lines)
            {
                string[] sp = line.Split(new char[] { '\t' });
                if (sp.Length <= 1)
                {
                    continue;
                }
                this[sp[0]] = sp[1];
            }
        }
        public string at(string key, string def = "")
        {
            if (!this.ContainsKey(key))
            {//設定されていないっぽい
                return def;
            }
            return this[key];
        }
    }
}
