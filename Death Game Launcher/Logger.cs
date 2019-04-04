using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Death_Game_Launcher
{
    public class Logger
    {
        public const int INFO = 0;
        public const int WARN = 1;
        public const int ERROR = 2;

        private List<Item> _items;
        struct Item
        {
            public int lvl;
            public string txt;
            public Exception exception;
        }

        public Logger(int minLvl)
        {
            _items = new List<Item>();
            LogLevel = minLvl;
        }

        public int LogLevel { get; set; }

        public void Info(string s)
        {
            _items.Add(new Item() { lvl = INFO, txt = s });
        }
        public void Warn(string s)
        {
            _items.Add(new Item() { lvl = WARN, txt = s });
        }
        public void Error(Exception e, string s)
        {
            _items.Add(new Item() { lvl = ERROR, txt = s, exception = e });
        }

        public void SaveToFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                StreamWriter sw = new StreamWriter(File.OpenWrite(path));
                foreach (Item i in _items)
                {
                    if (i.lvl >= LogLevel)
                    {
                        switch (i.lvl)
                        {
                            case INFO:
                                sw.WriteLine("[INFO]: {0}", i.txt);
                                break;
                            case WARN:
                                sw.WriteLine("[WARNING]: {0}", i.txt);
                                break;
                            case ERROR:
                                sw.WriteLine("[ERROR]: {0}\n{1}", i.txt, i.exception.StackTrace);
                                break;
                            default:
                                sw.WriteLine("[UNKNOWN]: {0}", i.txt);
                                break;
                        }
                    }
                }
                sw.Close();
                sw.Dispose();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error saving log file.\n" + e.Message + "\n" + e.StackTrace);
            }
        }

        public string[] GetLines()
        {
            List<string> list = new List<string>();

            foreach (Item i in _items)
            {
                if (i.lvl >= LogLevel)
                {
                    switch (i.lvl)
                    {
                        case INFO:
                            list.Add("[INFO]: " + i.txt);
                            break;
                        case WARN:
                            list.Add("[WARN]: " + i.txt);
                            break;
                        case ERROR:
                            list.Add("[ERROR]: " + i.txt + "\n" + i.exception.StackTrace);
                            break;
                        default:
                            list.Add("[UNKNOWN]: " + i.txt);
                            break;
                    }
                }
            }

            return list.ToArray();
        }
    }
}
