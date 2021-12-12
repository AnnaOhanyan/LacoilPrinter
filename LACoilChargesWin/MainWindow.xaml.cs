using LACoil.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LACoilChargesWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string guid = "cacaeda3-613c-49dd-a131-2dfdc796f98a";
        Charge currentCharge = null;
        //string baseURL = "http://lacoil.am/";
        string baseURL = "http://172.31.5.10/";
        //string baseURL = "http://localhost:62522/";
        string secondUrl = "http://lacoil.am/";

        public static Thread checkThread = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PrintCheck_Click(object sender, RoutedEventArgs e)
        {
            currentCharge = null;

            try
            {
                FormMessage.Text = "";

                if (CheckNumber.Text == null || CheckNumber.Text.Length == 0)
                {
                    FormMessage.Text = "Չեքի համարը պարտադիր է";
                    FormMessage.Foreground = Brushes.Red;

                    return;
                }

                HttpWebRequest baseRequest = (HttpWebRequest)HttpWebRequest.Create(baseURL + "Charges/PrintCheck?checkNumber=" + CheckNumber.Text + "&guid=" + guid);
                baseRequest.Method = "GET";

                HttpWebRequest secondRequest = (HttpWebRequest)HttpWebRequest.Create(secondUrl + "Charges/PrintCheck?checkNumber=" + CheckNumber.Text + "&guid=" + guid);
                secondRequest.Method = "GET";

                HttpWebResponse response = null;

                try
                {
                    response = (HttpWebResponse)baseRequest.GetResponse();
                }
                catch (Exception)
                {
                    response = (HttpWebResponse)secondRequest.GetResponse();
                }

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);

                if ((int)response.StatusCode > 226 || response.StatusCode == HttpStatusCode.NotFound)
                {
                    FormMessage.Text = "Տեղի է ունեցել սխալ";
                    FormMessage.Foreground = Brushes.Red;
                    return;
                }

                string responce = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();

                try
                {
                    if (responce.Contains("errorMessage"))
                    {
                        ErrorModel errorModel = JsonConvert.DeserializeObject<ErrorModel>(responce);
                        FormMessage.Text = errorModel.ErrorMessage;
                        FormMessage.Foreground = Brushes.Red;

                        return;
                    }
                    else
                    {
                        currentCharge = JsonConvert.DeserializeObject<Charge>(responce);

                        PrintDocument pd = new PrintDocument();
                        pd.PrintPage += new PrintPageEventHandler
                           (this.pd_PrintPage);

                        pd.Print();

                        FormMessage.Text = "Վերցրեք չեքը";
                        FormMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 7, 135, 0));
                    }
                }
                catch (Exception)
                {
                    FormMessage.Text = "Տեղի է ունեցել սխալ";
                    FormMessage.Foreground = Brushes.Red;

                    return;
                }
            }
            catch (Exception)
            {
                FormMessage.Text = "Տեղի է ունեցել սխալ";
                FormMessage.Foreground = Brushes.Red;
            }
        }

        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            if (currentCharge != default)
            {
                float linesPerPage = 0;
                float leftMargin = 10;
                float topMargin = 0;
                System.Drawing.Font printFont = new System.Drawing.Font("Arial", 8);
                System.Drawing.Font headPrintFont = new System.Drawing.Font("Arial", 10);

                // Calculate the number of lines per page.
                linesPerPage = ev.MarginBounds.Height /
                   printFont.GetHeight(ev.Graphics);

                float margin = 0;
                ev.Graphics.DrawString("LACoil         Բարի գալուստ", printFont, System.Drawing.Brushes.Black,
                       leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + printFont.GetHeight(ev.Graphics);

                ev.Graphics.DrawString("=======================", printFont, System.Drawing.Brushes.Black,
                       leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + printFont.GetHeight(ev.Graphics);

                ev.Graphics.DrawString("ԱՆԴՈՐԱԳԻՐԸ N" + (currentCharge.ReturnDate == null ? currentCharge.CheckID : currentCharge.ReturnCheckID), printFont, System.Drawing.Brushes.Black,
                       leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + printFont.GetHeight(ev.Graphics);

                ev.Graphics.DrawString("Ամսաթիվ: " +
                    (currentCharge.ReturnDate != null ?
                    currentCharge.ReturnDate.Value.AddHours(4).ToString("G", CultureInfo.CreateSpecificCulture("es-ES"))
                    : currentCharge.CreationDate.AddHours(4).ToString("G", CultureInfo.CreateSpecificCulture("es-ES"))),
                    printFont, System.Drawing.Brushes.Black,
                       leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + printFont.GetHeight(ev.Graphics);

                ev.Graphics.DrawString("Տերմինալ։ " + currentCharge.DeviceNumber, printFont, System.Drawing.Brushes.Black,
                       leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + printFont.GetHeight(ev.Graphics);

                string[] data = Regex.Replace("Լց. հասցե։ " + currentCharge.DeviceAddress, "(.{1,24})", "$1\n").Split('\n');

                foreach (var d in data)
                {
                    ev.Graphics.DrawString(d, printFont, System.Drawing.Brushes.Black,
                           leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                    margin += 2 + printFont.GetHeight(ev.Graphics);
                }


                ev.Graphics.DrawString(currentCharge.ReturnDate == null ? "          ՎԱՃԱՌՔ" : "          ՎԵՐԱԴԱՐՁ", headPrintFont, System.Drawing.Brushes.Black,
                       leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + headPrintFont.GetHeight(ev.Graphics);

                ev.Graphics.DrawString(currentCharge.ProductName, printFont, System.Drawing.Brushes.Black,
                       leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + printFont.GetHeight(ev.Graphics);

                data = Regex.Replace(currentCharge.ReturnDate != null ?
                    $"{currentCharge.ReturnQty} լիտր x {currentCharge.Price} = {currentCharge.ReturnQty * currentCharge.Price} դրամ"
                    : $"{currentCharge.Qty} լիտր x {currentCharge.Price} = {currentCharge.Qty * currentCharge.Price} դրամ",
                        "(.{1,27})", "$1\n").Split('\n');

                foreach (var d in data)
                {
                    ev.Graphics.DrawString(d, printFont, System.Drawing.Brushes.Black,
                           leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                    margin += 2 + printFont.GetHeight(ev.Graphics);
                }

                ev.Graphics.DrawString("       ՀԱՍՏԱՏՎԵԼ Է", headPrintFont, System.Drawing.Brushes.Black,
                       leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + headPrintFont.GetHeight(ev.Graphics);

                ev.Graphics.DrawString("Քարտ   " + currentCharge.CartNumber, printFont, System.Drawing.Brushes.Black,
                       leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + printFont.GetHeight(ev.Graphics);

                ev.Graphics.DrawString("=======================", printFont, System.Drawing.Brushes.Black,
                   leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + printFont.GetHeight(ev.Graphics);

                ev.Graphics.DrawString("Ոչ ֆիսկալ փաստաթուղթ", printFont, System.Drawing.Brushes.Black,
                       leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + printFont.GetHeight(ev.Graphics);

                ev.Graphics.DrawString("=======================", printFont, System.Drawing.Brushes.Black,
                   leftMargin, topMargin + margin, new System.Drawing.StringFormat());
                margin += 2 + printFont.GetHeight(ev.Graphics);

                ev.Graphics.DrawString("Շնորհակալություն։ Բարի Երթ!", printFont, System.Drawing.Brushes.Black,
                   leftMargin, topMargin + margin, new System.Drawing.StringFormat());
            }
        }
    }
}