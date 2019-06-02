using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace Death_Game_Launcher
{
    public class Logger
    {
        public const int DEBUG = 0;
        public const int INFO = 1;
        public const int WARN = 2;
        public const int ERROR = 3;

        //List of log entries
        private List<Item> _items;

        private string _outPath;

        //Structure for log entries, containing level, message, and (for the case of an error) an exception
        struct Item
        {
            public int lvl;
            public string txt;
            public Exception exception;
        }
        //Initializes a new instance of the Logger class with the logging level being given in the parameter
        public Logger(int minLvl, string outputPath)
        {
            _items = new List<Item>();
            LogLevel = minLvl;
            this._outPath = outputPath;
            if (File.Exists(outputPath))
                File.Delete(outputPath);
            this.sw = new StreamWriter(File.OpenWrite(this._outPath));
        }
        //The minimum level of log entries to be saved at the end of the program
        public int LogLevel { get; set; }

        public void Debug(string s)
        {
            AddItem(new Item() { lvl = DEBUG, txt = s });
        }
        public void Info(string s)
        {
            AddItem(new Item() { lvl = INFO, txt = s });
        }
        public void Warn(string s)
        {
            AddItem(new Item() { lvl = WARN, txt = s });
        }
        public void Error(Exception e, string s)
        {
            AddItem(new Item() { lvl = ERROR, txt = s, exception = e });
        }

        /*public void SaveToFile(string path)
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
                                sw.WriteLine("[ERROR]: {0}\n\t{1}{2}", i.txt, i.exception.Message, i.exception.StackTrace);
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
            catch (ArgumentException e)
            {
                ErrorRedo(e);
            }
            catch (PathTooLongException e)
            {
                ErrorRedo(e);
            }
            catch (DirectoryNotFoundException e)
            {
                ErrorRedo(e);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error saving log file.\n" + e.Message + "\n" + e.StackTrace);
            }
        }*/
        private bool ErrorRedo(Exception e)
        {
            MessageBox.Show("Failed to save log in given file.\n" + e.Message + "\n" + e.StackTrace);
            bool b = false;
            foreach (Item item in _items)
            {
                if (item.lvl >= LogLevel)
                {
                    b = true;
                    break;
                }
            }
            if (b)
            {
                MessageBox.Show("Please select a folder to save log file in.");
                FolderBrowserDialog d = new FolderBrowserDialog();
                if (d.ShowDialog() == DialogResult.OK)
                    _outPath = Path.Combine(d.SelectedPath, "DLG-Log.txt");
                else
                    _outPath = "C:\\DLG-Log.txt";
                return true;
            }
            else
                return false;
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
                        case DEBUG:
                            list.Add("[DEBUG]: " + i.txt);
                            break;
                        case INFO:
                            list.Add("[INFO]: " + i.txt);
                            break;
                        case WARN:
                            list.Add("[WARN]: " + i.txt);
                            break;
                        case ERROR:
                            list.Add("[ERROR]: " + i.txt + "\n\t" + i.exception.Message + i.exception.StackTrace);
                            break;
                        default:
                            list.Add("[UNKNOWN]: " + i.txt);
                            break;
                    }
                }
            }

            return list.ToArray();
        }

        private int attempt = 0;
        StreamWriter sw;
        private void AddItem(Item i)
        {
            //if (sw == null) sw = new StreamWriter(File.OpenWrite(this._outPath));
            _items.Add(i);
            if (i.lvl >= LogLevel)
            {
                try
                {
                    
                    //StreamWriter sw = new StreamWriter(File.OpenWrite(this._outPath));
                    switch (i.lvl)
                    {
                        case DEBUG:
                            sw.WriteLine("[DEBUG]{0}: {1}", DateTime.Now.ToString("HH:mm:ss.ffff"), i.txt);
                            break;
                        case INFO:
                            sw.WriteLine("[INFO]{0}: {1}", DateTime.Now.ToString("HH:mm:ss.ffff"), i.txt);
                            break;
                        case WARN:
                            sw.WriteLine("[WARNING]{0}: {1}", DateTime.Now.ToString("HH:mm:ss.ffff"), i.txt);
                            break;
                        case ERROR:
                            sw.WriteLine("[ERROR]{0}: {1}\n\t{2}{3}", DateTime.Now.ToString("HH:mm:ss.ffff"), i.txt, i.exception.Message, i.exception.StackTrace);
                            break;
                        default:
                            sw.WriteLine("[UNKNOWN]{0}: {1}", DateTime.Now.ToString("HH:mm:ss.ffff"), i.txt);
                            break;
                    }
                    //sw.Close();
                    //sw.Dispose();
                    sw.Flush();
                    attempt = 0;
                }
                catch (ArgumentException e)
                {
                    if (attempt < 10)
                    {
                        if (ErrorRedo(e))
                        {
                            attempt++;
                            AddItem(i);
                        }
                    }
                    else
                        throw new LoggerDiedException() { Message = "Logger failed to write to output file too many times." };
                }
                catch (PathTooLongException e)
                {
                    if (attempt < 10)
                    {
                        if (ErrorRedo(e))
                        {
                            attempt++;
                            AddItem(i);
                        }
                    }
                    else
                        throw new LoggerDiedException() { Message = "Logger failed to write to output file too many times." };
                }
                catch (DirectoryNotFoundException e)
                {
                    if (attempt < 10)
                    {
                        if (ErrorRedo(e))
                        {
                            attempt++;
                            AddItem(i);
                        }
                    }
                    else
                        throw new LoggerDiedException() { Message = "Logger failed to write to output file too many times." };
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error saving log file.\n" + e.Message + "\n" + e.StackTrace);
                }
            }
        }

        public void Close()
        {
            Info("Logger closing.");
            sw.Close();
        }
    }
}
