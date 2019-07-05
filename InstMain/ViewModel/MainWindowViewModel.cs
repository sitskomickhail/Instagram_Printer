using InstMain.Infrastructure;
using InstMain.Model;
using Microsoft.Win32;
using PrinterLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace InstMain.ViewModel
{
    class MainWindowViewModel : BaseViewModel
    {
        InstagramModel model;

        private int _successPrint;
        public int SuccessPrint
        {
            get { return _successPrint; }
            set { _successPrint = value; OnPropertyChanged(); }
        }

        private int _waitingPtint;
        public int WaitingPrint
        {
            get { return _waitingPtint; }
            set { _waitingPtint = value; OnPropertyChanged(); }
        }

        private int _unsuccessPrint;
        public int UnsuccessPrints
        {
            get { return _unsuccessPrint; }
            set { _unsuccessPrint = value; OnPropertyChanged(); }
        }

        private string _hashtag1;
        public string Hashtag_1
        {
            get { return _hashtag1; }
            set { _hashtag1 = value; }
        }

        private string _hashtag2;
        public string Hashtag_2
        {
            get { return _hashtag2; }
            set { _hashtag2 = value; }
        }


        public ObservableCollection<InstagramModel> InstagramPhotos { get; set; }
        public ICommand StartCommand { get; set; }
        public ICommand PauseCommand { get; set; }
        public ICommand OpenPhotoCommand { get; set; }
        public ICommand OpenPrinter { get; set; }

        public MainWindowViewModel()
        {
            string day = (DateTime.Now.Day < 10) ? $"0{DateTime.Now.Day}" : $"{DateTime.Now.Day}";
            string month = (DateTime.Now.Month < 10) ? $"0{DateTime.Now.Month}" : $"{DateTime.Now.Month}";
            Hashtag_1 = "#chernobylquest";
            Hashtag_2 = $"#chernobylquest{day}{month}";

            IsEnabledStartBtn = true;

            model = new InstagramModel();
            OpenPhotoCommand = new RelayCommand(OnOpenPhotoCommandExecute);
            StartCommand = new RelayCommand(OnStartCommandExecute);
        }

        private bool _isEnabledStartBtn;

        public bool IsEnabledStartBtn
        {
            get { return _isEnabledStartBtn; }
            set { _isEnabledStartBtn = value; OnPropertyChanged(); }
        }

        private bool _isEnabledPauseBtn;

        public bool IsEnabledPauseBtn
        {
            get { return _isEnabledPauseBtn; }
            set { _isEnabledPauseBtn = value; OnPropertyChanged(); }
        }


        private void OnStartCommandExecute(object obj)
        {
            if (_hashtag1.Length == 0 && _hashtag2.Length == 0)
                MessageBox.Show("Не определены значения хештегов", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                IsEnabledStartBtn = false;
                IsEnabledPauseBtn = true;

                if (Hashtag_1.Contains("#"))
                    model.Hashtag_1 = Hashtag_1.Remove(0, 1);
                else
                    model.Hashtag_1 = Hashtag_1;
                if (Hashtag_2.Contains("#"))
                    model.Hashtag_2 = Hashtag_2.Remove(0, 1);
                else
                    model.Hashtag_2 = Hashtag_2;
                model.Start();
            }
        }



        private void OnOpenPhotoCommandExecute(object obj)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.InitialDirectory = $@"{Environment.CurrentDirectory}\Templates";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                model.TemplateName = openFileDialog.FileName;
            }
        }
    }
}