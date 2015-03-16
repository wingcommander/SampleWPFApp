using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace SampleApp.Model
{
    public class Data : TableEntity, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        DateTime date;
        public DateTime Date
        {
            get
            {
                return date;
            }

            set
            {
                date = value;
                OnPropertyChanged("date");
            }
        }

        Decimal price;
        public Decimal Price
        {
            get
            {
                return price;
            }

            set
            {
                price = value;
                OnPropertyChanged("price");
            }
        }
    }
}
