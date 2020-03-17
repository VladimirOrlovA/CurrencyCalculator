using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Threading;

namespace СurrencyСalculator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Item> items = new List<Item>();

        // Получает или задает язык и региональные параметры. Используем для форматирования даты и чисел
        CultureInfo cultureInfoEN = new CultureInfo("en-US");
        CultureInfo cultureInfoRU = new CultureInfo("ru-RU");

        public MainWindow()
        {
            InitializeComponent();

            gCurrencyData.DataContext = items;

            if (DataLoad())
                tbTitle.Text += items[0].PubDate.ToString("d", cultureInfoRU);

            cbCurrency1.SelectionChanged += comboBox_SelectionChanged;
            cbCurrency2.SelectionChanged += comboBox_SelectionChanged;
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string cbName = ((ComboBox)sender).Name;

            KeyEventArgs exp = null;

            CurrencyDynamic();

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

                foreach (XmlNode xmlNode in xmlRoot)
                    foreach (XmlNode xmlNodeChild in xmlNode)
                    {
                        if (xmlNodeChild.Name == "item")
                        {
                            Item item = new Item();

                            foreach (XmlNode xmlItem in xmlNodeChild)
                            {
                                if (xmlItem.Name == "title") item.Title = xmlItem.InnerText;
                                if (xmlItem.Name == "pubDate") item.PubDate = Convert.ToDateTime(xmlItem.InnerText, cultureInfoRU);
                                if (xmlItem.Name == "description")
                                {
                                    //string val = xmlItem.InnerText.Replace(".", ",");
                                    string val = xmlItem.InnerText;
                                    item.Description = Convert.ToDouble(val, cultureInfoEN);
                                }
                                if (xmlItem.Name == "index") item.Index = xmlItem.InnerText;
                                if (xmlItem.Name == "change") item.Change = xmlItem.InnerText;
                            }
                            items.Add(item);
                        }
                    }

                List<string> currencyName = items.Select(f => f.Title).ToList();
                currencyName.Insert(0, "KZT");
                //currencyName.Sort();

                cbCurrency1.ItemsSource = currencyName;
                cbCurrency2.ItemsSource = currencyName;

                cbCurrency1.SelectedValue = "USD";
                cbCurrency2.SelectedValue = "KZT";

                firstStartResult();

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

        private void firstStartResult()
        {
            tbInput1.Text = "1";
            object sender = null;
            KeyEventArgs exp = null;
            TbInput1_KeyUp(sender, exp);
            CurrencyDynamic();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            tbUpdate.Foreground = Brushes.Black;
            items.Clear();
            if (DataLoad())
            {
                tbUpdate.Text = "последнее обновление " + DateTime.Now.ToString();

                firstStartResult();
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
                if (currName == item.Title)
                {
                    rate = item.Description;
                    break;
                }
            }
            return rate;
        }

        private void CurrencyDynamic()
        {
            DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;
            //DayOfWeek dayOfWeek = DayOfWeek.Monday;
            if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
            {
                tbkCurrency1 = CurrencyDynamicInfo(cbCurrency1, tbkCurrency1);
                tbkCurrency2 = CurrencyDynamicInfo(cbCurrency2, tbkCurrency2);
            }
            else
            {
                tbUpdate.Text = "в Сб Вс нет торгов";
                tbUpdate.Foreground = Brushes.Red;
                tbkCurrency1.Text = tbkCurrency2.Text = " ";
            }
        }

        private TextBlock CurrencyDynamicInfo(ComboBox comboBox, TextBlock textBlock)
        {
            if (comboBox.SelectedValue.ToString() == "KZT")
            {
                textBlock.Text = "";
                return textBlock;

            }

            foreach (var item in items)
            {

                if (comboBox.SelectedValue.ToString() == item.Title)
                {
                    string str = item.Change;
                    if (str.Contains('-')) textBlock.Foreground = Brushes.Green;
                    else if (str.Contains('+')) textBlock.Foreground = Brushes.Red;
                    else textBlock.Foreground = Brushes.Black;
                    textBlock.Text = str;
                    return textBlock;
                }
            }
            return textBlock;
        }

    }

}


