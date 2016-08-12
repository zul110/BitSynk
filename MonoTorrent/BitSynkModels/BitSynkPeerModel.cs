using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoTorrent.BitSynkModels {
    public class BitSynkPeerModel : BaseModel {
        private Uri connectionUri;
        public Uri ConnectionUri
        {
            get { return connectionUri; }
            set
            {
                connectionUri = value;
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

        private int piecesCount;
        public int PiecesCount
        {
            get { return piecesCount; }
            set
            {
                piecesCount = value;
                NotifyPropertyChanged();
            }
        }
    }
}
