using ClipboardPurifier.Helpers;
using ClipboardPurifier.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace ClipboardPurifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        #endregion

        #region Fields
        private bool? isToggleOn = false;
        public bool? IsToggleOn
        {
            get { return isToggleOn; }
            set
            {
                isToggleOn = value;
                this.RaisePropertyChanged("IsToggleOn");
                ToggleRun();
            }
        }
        private int succNum;
        public int SuccNum
        {
            get { return succNum; }
            set
            {
                succNum = value;
                this.RaisePropertyChanged("SuccNum");
            }
        }
        private int errNum;
        public int ErrNum
        {
            get { return errNum; }
            set
            {
                errNum = value;
                this.RaisePropertyChanged("ErrNum");
            }
        }
        private TaskState state = TaskState.None;
        public TaskState State
        {
            get { return state; }
            set
            {
                state = value;
                this.RaisePropertyChanged("State");
            }
        }
        private bool isReserveText = true;
        public bool IsReserveText
        {
            get { return isReserveText; }
            set
            {
                isReserveText = value;
                this.RaisePropertyChanged("IsReserveText");
            }
        }
        #endregion

        #region Main Task Work
        //private BackgroundWorker bgw;
        private int interval = 1000;
        private bool needinit = false;
        private bool isbusy;

        public void Go()
        {
            isbusy = true;
            if (needinit)
                InitBeforeRun();
            try
            {
                if (IsReserveText)
                {
                    IDataObject? data = Clipboard.GetDataObject();
                    if (data == null)
                    {
                        return;
                    }
                    string[] formats = data.GetFormats();
                    if (formats.Any())
                    {
                        foreach (string format in formats)
                        {
                            if (data.GetDataPresent(format, false))
                            {
                                if (format == "Rich Text Format" || format == "HTML Format")
                                {
                                    var text = Clipboard.GetText();
                                    Clipboard.SetText(text);
                                    SuccNum += 1;
                                }
                            }
                        }
                    }
                    if (Clipboard.ContainsAudio() || Clipboard.ContainsFileDropList() || Clipboard.ContainsImage())
                    {
                        if (Clipboard.ContainsText())
                        {
                            var text = Clipboard.GetText();
                            Clipboard.SetText(text);
                            SuccNum += 1;
                        }
                    }
                }
                else//reserve image
                {
                    IDataObject? data = Clipboard.GetDataObject();
                    if (data == null)
                    {
                        return;
                    }
                    if (Clipboard.ContainsAudio() || Clipboard.ContainsFileDropList() || Clipboard.ContainsText())
                    {
                        if (Clipboard.ContainsImage())
                        {
                            var image = Clipboard.GetImage();
                            Clipboard.SetImage(image);
                            SuccNum += 1;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ErrNum += 1;
            }

            return;
        }

        private void InitBeforeRun()
        {
            interval = 1000;
            needinit = false;
        }

        public void StartRun()
        {
            if (State == TaskState.Started) return;
            State = TaskState.Started;
            needinit = true;
            initbgw();
            Monitor.Enter(this);

            if (!isbusy)
            {
                Thread t = new Thread(new ThreadStart(Bgw_DoWork));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }

            Monitor.Exit(this);
        }

        public void StopRun()
        {
            //Monitor.Enter(this);
            State = TaskState.None;
            //Monitor.Exit(this);
        }

        public void ToggleRun()
        {
            if (IsToggleOn.Value)
            {
                StartRun();
            }
            else
            {
                StopRun();
            }
        }

        private void initbgw()
        {
            //if (bgw == null)
            //{
            //    bgw = new BackgroundWorker();
            //    bgw.DoWork += Bgw_DoWork;
            //    bgw.RunWorkerCompleted += Bgw_RunWorkerCompleted;
            //}
        }

        private void Bgw_RunWorkerCompleted()
        {
            Monitor.Enter(this);
            if (State == TaskState.Started)
            {
                Thread t = new Thread(new ThreadStart(Bgw_DoWork));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
            else
            {
                StopRun();
                isbusy = false;
            }
            
            Monitor.Exit(this);
        }

        private void Bgw_DoWork()
        {
            Go();
            Thread.Sleep(interval);
            Bgw_RunWorkerCompleted();
        }
        #endregion

        #region Events
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("是否要退出程序？退出后，程序无法继续净化剪切板。如果要让程序在后台运行，请最小化窗口。", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                return;
            else
                e.Cancel = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IsToggleOn = Boolean.Parse(IniHelper.GetKeyValue("main", "IsToggleOn", "false", IniHelper.inipath));
            IsReserveText = Boolean.Parse(IniHelper.GetKeyValue("main", "IsReserveText", "true", IniHelper.inipath));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopRun();
            IniHelper.SetKeyValue("main", "IsToggleOn", IsToggleOn.Value.ToString(), IniHelper.inipath);
            IniHelper.SetKeyValue("main", "IsReserveText", IsReserveText.ToString(), IniHelper.inipath);
        }
        #endregion

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion
    }
}
