using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private string hash;
        public string Hash
        {
            get { return hash; }
            set
            {
                hash = value;
                NotifyPropertyChanged();
            }
        }

        //private string fileId;
        //public string FileId
        //{
        //    get { return fileId; }
        //    set
        //    {
        //        fileId = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        //private string userId;
        //public string UserId
        //{
        //    get { return userId; }
        //    set
        //    {
        //        userId = value;
        //        NotifyPropertyChanged();
        //    }
        //}

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

        private ObservableCollection<BitSynkPeerModel> bitSynkPeers;
        public ObservableCollection<BitSynkPeerModel> BitSynkPeers
        {
            get
            {
                if(bitSynkPeers == null) {
                    bitSynkPeers = new ObservableCollection<BitSynkPeerModel>();
                }

                return bitSynkPeers;
            }

            set
            {
                bitSynkPeers = value;
                NotifyPropertyChanged();
            }
        }
    }
}
