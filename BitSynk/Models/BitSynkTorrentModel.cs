﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitSynk.Models {
    public class BitSynkTorrentModel : BaseModel {
        private string name;
        public string Name
        {
            get { return name; }
            set {
                name = value;
                NotifyPropertyChanged();
            }
        }

        private string state;
        public string State
        {
            get { return state; }
            set
            {
                state = value;
                NotifyPropertyChanged();
            }
        }

        private double progress;
        public double Progress
        {
            get { return progress; }
            set {
                progress = value;
                NotifyPropertyChanged();
            }
        }

        private double downloadSpeed;
        public double DownloadSpeed
        {
            get { return downloadSpeed; }
            set
            {
                downloadSpeed = value;
                NotifyPropertyChanged();
            }
        }

        private double uploadSpeed;
        public double UploadSpeed
        {
            get { return uploadSpeed; }
            set
            {
                uploadSpeed = value;
                NotifyPropertyChanged();
            }
        }
    }
}
