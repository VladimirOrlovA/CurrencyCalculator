using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Xml;
using System.Xml.Linq;

namespace СurrencyСalculator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Item> items = new List<Item>();
        double kzt = 1;

        public MainWindow()
        {
            InitializeComponent();
            gCurrencyData.DataContext = items;
            tbTitle.Text += DateTime.Now.ToString();
            DataLoad();

        }

        private void DataLoad()
        {
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load("https://nationalbank.kz/rss/rates_all.xml?switch=russian");
            }
            catch (Exception eXml)
            {
                tbUpdate.Foreground = Brushes.Red;
                string errMes = eXml.ToString();
                tbUpdate.Text = errMes.Substring(errMes.IndexOf(':') + 1); ;
                return;
            }

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
                            if (xmlItem.Name == "description") item.description =
                                    Convert.ToDouble(xmlItem.InnerText.Replace(".", ","));
                        }
                        items.Add(item);
                    }
                }

            List<string> currencyName = items.Select(f => f.title).ToList();
            currencyName.Add("KZT");
            cbCurrency1.ItemsSource = currencyName;
            cbCurrency2.ItemsSource = currencyName;

            cbCurrency1.SelectedIndex = 0;
            cbCurrency2.SelectedIndex = 0;
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            items.Clear();
            DataLoad();
        }

        private void TbInput1_KeyUp(object sender, KeyEventArgs e)
        {
            double result = Result(false);
            tbInput2.Text = result.ToString();
        }

        private void TbInput2_KeyUp(object sender, KeyEventArgs e)
        {
            double result = Result(true);
            tbInput1.Text = result.ToString();
        }

        private double Result(bool direction)
        {
            double curr1 = Convert.ToDouble(tbInput1.Text); 
            double coef = 0;
            double rate1 = 0;
            double rate2 = 0;

            string currName1 = cbCurrency1.SelectedValue.ToString();
            string currName2 = cbCurrency2.SelectedValue.ToString();

            rate1 = RateCurrency(currName1);
            rate2 = RateCurrency(currName2);

            if (direction)
                coef = rate2 / rate1;
            else
                coef = rate1 / rate2;

            double result = curr1 * coef;

            return result;
        }

        private double RateCurrency(string currName)
        {
            double rate = 0;

            if (currName == "KZT")
            {
                rate = 1;
            }
            else
            {
                foreach (var item in items)
                {
                    if (currName == item.title)
                    {
                        rate = item.description;
                        break;
                    }
                }
            }

            return rate;
        }
    }
}

