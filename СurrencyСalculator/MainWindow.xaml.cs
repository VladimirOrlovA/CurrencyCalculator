using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace СurrencyСalculator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Item> items = new List<Item>();

        public MainWindow()
        {
            CultureInfo cultureInfo = new CultureInfo("ru-RU");

            InitializeComponent();

            gCurrencyData.DataContext = items;

            if (DataLoad())
                tbTitle.Text += items[0].pubDate.ToString("d");

            cbCurrency1.SelectionChanged += comboBox_SelectionChanged;
            cbCurrency2.SelectionChanged += comboBox_SelectionChanged;
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string cbName = ((ComboBox)sender).Name;

            KeyEventArgs exp = null;

            if (cbName == "cbCurrency1")
                TbInput1_KeyUp(sender, exp);
            else
                TbInput2_KeyUp(sender, exp);
        }

        private bool DataLoad()
        {
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load("https://nationalbank.kz/rss/rates_all.xml?switch=russian");
                XmlElement xmlRoot = xmlDoc.DocumentElement;

                int countItem = 0;

                foreach (XmlNode xmlNode in xmlRoot)
                    foreach (XmlNode xmlNodeChild in xmlNode)
                    {
                        if (xmlNodeChild.Name == "item")
                        {
                            countItem++;
                            Item item = new Item();

                            foreach (XmlNode xmlItem in xmlNodeChild)
                            {
                                if (xmlItem.Name == "title") item.title = xmlItem.InnerText;
                                if (xmlItem.Name == "pubDate") item.pubDate = Convert.ToDateTime(xmlItem.InnerText);
                                if (xmlItem.Name == "description")
                                {
                                    //string val = xmlItem.InnerText.Replace(".", ",");
                                    string val = xmlItem.InnerText;
                                    item.description = Convert.ToDouble(val);
                                    double dval = Convert.ToDouble(val);
                                }

                            }
                            items.Add(item);
                        }
                    }

                List<string> currencyName = items.Select(f => f.title).ToList();
                currencyName.Insert(0, "KZT");

                cbCurrency1.ItemsSource = currencyName;
                cbCurrency2.ItemsSource = currencyName;

                cbCurrency1.SelectedIndex = 5;
                cbCurrency2.SelectedIndex = 0;
                return true;
            }
            catch (Exception eXml)
            {
                tbUpdate.Foreground = Brushes.Red;
                string errMes = eXml.ToString();
                tbUpdate.Text = "Ресурс с данными о валюте не доступен";
                return false;
            }

        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            tbUpdate.Foreground = Brushes.Black;
            items.Clear();
            if (DataLoad())
            {
                tbUpdate.Text = "последнее обновление " + DateTime.Now.ToString();

                tbInput1.Text = "1";
                KeyEventArgs exp = null;
                TbInput1_KeyUp(sender, exp);
            }
            
        }

        private void TbInput1_KeyUp(object sender, KeyEventArgs e)
        {
            string userInput = tbInput1.Text;
            double result = Result(userInput, true);
            tbInput2.Text = "";
            tbInput2.Text = Math.Round(result, 2).ToString();
        }

        private void TbInput2_KeyUp(object sender, KeyEventArgs e)
        {
            string userInput = tbInput2.Text;
            double result = Result(userInput, false);
            tbInput1.Text = "";
            tbInput1.Text = Math.Round(result, 2).ToString();
        }

        private double Result(string userInput, bool direction)
        {
            double currency = 0;
            if (!string.IsNullOrEmpty(userInput) && !double.TryParse(userInput, out currency))
                currency = 0;

            double rate1 = 0;
            double rate2 = 0;

            string currName1 = cbCurrency1.SelectedValue.ToString();
            string currName2 = cbCurrency2.SelectedValue.ToString();

            rate1 = RateCurrency(currName1);
            rate2 = RateCurrency(currName2);


            double coef = 0;

            if (direction)
                coef = rate1 / rate2;
            else
                coef = rate2 / rate1;

            double result = currency * coef;

            return result;
        }

        private double RateCurrency(string currName)
        {
            double rate = 1;

            foreach (var item in items)
            {
                if (currName == item.title)
                {
                    rate = item.description;
                    break;
                }
            }
            return rate;
        }
    }
}

