using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Win32;
using SampleApp.Model;
using SampleApp.Ado.DAL;


namespace SampleApp.ViewModel
{
    public class Action : INotifyPropertyChanged
    {
        const int HEADER_ROW_INDEX = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<Data> dataList;

        public List<Data> DataList
        {
            get
            {
                return dataList;
            }

            set
            {
                dataList = value;
                OnPropertyChanged("DataList");
            }
        
        }

        private string progress;

        public string Progress
        {
            get { return progress; }
            set
            {
                if (value != progress)
                {
                    progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }

        private DateTime startDate = DateTime.Today;

        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                if (value != startDate)
                {
                    startDate = value;
                    OnPropertyChanged("StartDate");
                }
            }
        }

        private DateTime endDate = DateTime.Today;

        public DateTime EndDate
        {
            get { return endDate; }
            set
            {
                if (value != endDate)
                {
                    endDate = value;
                    OnPropertyChanged("EndDate");
                }
            }
        }

        private decimal average;

        public decimal Average
        {
            get { return average; }
            set
            {
                if (value != average)
                {
                    average = value;
                    OnPropertyChanged("Average");
                }
            }
        }

        private double standardDeviation;

        public double StandardDeviation
        {
            get { return standardDeviation; }
            set
            {
                if (value != standardDeviation)
                {
                    standardDeviation = value;
                    OnPropertyChanged("StandardDeviation");
                }
            }
        }

        private decimal maxPrice;

        public decimal MaxPrice
        {
            get { return maxPrice; }
            set
            {
                if (value != maxPrice)
                {
                    maxPrice = value;
                    OnPropertyChanged("MaxPrice");
                }
            }
        }

        private decimal minPrice;

        public decimal MinPrice
        {
            get { return minPrice; }
            set
            {
                if (value != minPrice)
                {
                    minPrice = value;
                    OnPropertyChanged("MinPrice");
                }
            }
        }


        bool isCalculationEnabled = false;

        public bool IsCalculationEnabled
        {
            get
            {
                return isCalculationEnabled;
            }

            set
            {
                isCalculationEnabled = value;

                OnPropertyChanged("IsCalculationEnabled");
            }
        }

        private string mostExpensiveHourData;

        public string MostExpensiveHourData
        {
            get { return mostExpensiveHourData; }
            set
            {
                if (value != mostExpensiveHourData)
                {
                    mostExpensiveHourData = value;
                    OnPropertyChanged("MostExpensiveHourData");
                }
            }
        }

        public void GetData()
        {
            List<Data> result = new List<Data>();

            IEnumerable<DataRow> rows = AdoDAL.GetRows("dbo.GetData");
        }

        public void GetDataBetweenDates()
        {
            Dictionary<String, Object> parameters = new Dictionary<String, Object>
            {  
                {"Start", this.StartDate},
                {"End", this.EndDate.AddDays(1).AddMinutes(-1)},    
            };

            IEnumerable<DataRow> rows = AdoDAL.GetRows("dbo.GetDataBetweenDates", parameters);
         
            if (rows != null)
            {
                List<Data> result = new List<Data>();

                result.AddRange(rows.Select(cn => new Data
                {
                    Date = cn.Field<DateTime>("Date"),
                    Price = cn.Field<Decimal>("Price")
                }).ToList());

                this.DataList = result;
                this.Progress = String.Format("Result {0}", result.Count.ToString());
            }
        }

        public void LoadData()
        {
           string filePathName = SelectFile();

           if (filePathName != null)
           {

            List<Data> result = ReadDataFromFile(filePathName);
            DeleteDataFromDB();

            Task.Factory.StartNew(() =>
            {
                StoreData(result);
                GetDataBetweenDates();
            });
           }
        }

        public string SelectFile()
        {
            String filePathAndName = GetPathAndNameFromFileDialog();

            return filePathAndName;
        }

        private void StoreData(List<Data> dataList)
        {
            int count = 0;

                foreach (Data data in dataList)
                {
                    count++;
                    StoreData(data);
                    this.Progress = String.Format("Loading line: {0}", count.ToString());
                }

                this.StartDate = dataList[0].Date;
                this.EndDate = dataList[dataList.Count-1].Date;
                this.IsCalculationEnabled = true;
        }


        public void CalculateData()
        {
            GetAverageDataBetweenDates();
            GetMostExpensiveHourBetweenDates();
            GetDataBetweenDates();
        }

        public void GetAverageDataBetweenDates()
        {
            Dictionary<String, Object> parameters = new Dictionary<String, Object>
            {  
                {"Start", this.StartDate},
                {"End", this.EndDate.AddDays(1).AddMinutes(-1)},    
            };

            DataRow dr = AdoDAL.GetSingleRow("dbo.GetDataStatsBetweenDates", parameters);

            if (dr != null && dr.ItemArray[0].ToString() != string.Empty)
            {
                this.Average = dr.Field<Decimal>("Average");
                this.StandardDeviation = dr.Field<double>("StandardDeviation");
                this.MaxPrice = dr.Field<Decimal>("MaxPrice");
                this.MinPrice = dr.Field<Decimal>("MinPrice");
            }
        }

        public void GetMostExpensiveHourBetweenDates()
        {
            Dictionary<String, Object> parameters = new Dictionary<String, Object>
            {  
                {"Start", this.StartDate},
                {"End", this.EndDate.AddDays(1).AddMinutes(-1)},    
            };

            DataRow dr = AdoDAL.GetSingleRow("dbo.GetMostExpensiveHourDataBetweenDates", parameters);

            if (dr != null)
            {
                this.MostExpensiveHourData = dr.Field<DateTime>("date").ToString() + " " + dr.Field<decimal>("TotalHourPrice").ToString();
            }
        }

        public void DeleteDataFromDB()
        {
            AdoDAL.ExecuteProc("dbo.DeleteData");
        }

        public string GetPathAndNameFromFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return null;
            }
        }

        public List<Data> ReadDataFromFile(string filePathAndName)
        {
            List<Data> result = new List<Data>();
            List<String> allLines = new List<string>();

            try
            {
                allLines.AddRange(File.ReadAllLines(filePathAndName));
                allLines.RemoveAt(HEADER_ROW_INDEX);

                result.AddRange(allLines.Select(cn => new Data
                {
                    Date = Convert.ToDateTime(cn.Split(',')[0]),
                    Price = Convert.ToDecimal(cn.Split(',')[1]),
                }));
            }
            catch (Exception e)
            {
                this.Progress = String.Format("Sorry some sort of error has occurred... {0}",  e.Message);
            }

            return result;
        }

        public void StoreData(Data data)
        {
            Dictionary<String, Object> parameters = new Dictionary<String, Object>
            {  
                {"Date", data.Date},
                {"Price", data.Price},    
            };

            AdoDAL.ExecuteProc("dbo.SaveData", parameters);
        }
    }
}
