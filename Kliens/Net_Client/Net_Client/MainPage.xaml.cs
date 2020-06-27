using Net_Client.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Net_Client
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private List<MeasurementClient> data;
        public MainPage()
        {
            InitializeComponent();
            data = new List<MeasurementClient>();
            measureValue.Keyboard = Keyboard.Numeric;
            greenhouseIDValue.Keyboard = Keyboard.Numeric;
            sensorIDValue.Keyboard = Keyboard.Numeric;
            referenceHumidity.Keyboard = Keyboard.Numeric;
            referenceTemperature.Keyboard = Keyboard.Numeric;
            //sendButton.IsEnabled = false;
        }
        int count = 0;
        int number_of_measurements = 0;
        bool IsGet = false;
       async void Handle_Clicked(object sender, System.EventArgs e)
        { 
            data = await GetAllMeasurement();
            datas.ItemsSource = data;
         }
        public async Task<List<MeasurementClient>> GetAllMeasurement()
        {
            List<MeasurementClient> alldata;
            using (var client = new HttpClient())
            {
            //http://10.0.2.2:5000/api/efmeasurements/
                 //https://measurementapi2020.azurewebsites.net/api/efmeasurements
                var response = await client.GetAsync(new Uri($"https://measurementapi2020.azurewebsites.net/api/efmeasurements")).ConfigureAwait(false);
                if(response.IsSuccessStatusCode)
                {
                    var jsonStream = await response.Content.ReadAsStreamAsync();
                    var json = await JsonDocument.ParseAsync(jsonStream);;
                    alldata = JsonConvert.DeserializeObject<List<MeasurementClient>>(json.RootElement.ToString());
                    number_of_measurements = alldata.Count;
                    IsGet = true;
                    return alldata;
                }
                else
                {
                    if(response!=null)
                    {
                        await DisplayAlert("Hiba", "Hibakód : " + response.StatusCode, "OK");
                    }
                    else
                    {
                        await DisplayAlert("Hiba", "Nincs kapcsolat a szerverrel", "OK");
                    }
                    alldata = null;
                }
                return alldata;
            }
        }
        public async Task PostMeasurement(DateTime date, int greenHouseID, int sensorId, double measuredValue)
        {
            MeasurementClient data = new MeasurementClient();
            number_of_measurements++;
            data.Id = number_of_measurements;
            data.Name = "Meres";
            data.DateTime = date;
            data.GreenHouseId = greenHouseID;
            data.SensorId = sensorId;
            data.MeasuredValue = measuredValue;
            if(checkHumidity.IsChecked)
            {
                data.Type = Classes.Type.Rh;
            }
            if(checkTemperature.IsChecked)
            {
                data.Type = Classes.Type.C;
            }
            //Ha mindkettő be van állítva a hőmérséklet az erősebb
            if(checkTemperature.IsChecked && checkHumidity.IsChecked)
            {
                data.Type = Classes.Type.C;
            }
            //Ha semmi sincs akkor is 
            if (!checkTemperature.IsChecked && !checkHumidity.IsChecked)
            {
                data.Type = Classes.Type.C;
            }
            string json = JsonConvert.SerializeObject(data);
            if(IsGet)
            {
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(
                        "https://measurementapi2020.azurewebsites.net/api/efmeasurements",
                         new StringContent(json, Encoding.UTF8, "application/json"));
                }
            }
            else
            {
                await DisplayAlert("Hiba", "Lekérdezés szükséges az első felküldés előtt!", "OK");
            }
            
        }
        private async void Button_Clicked(object sender, EventArgs e)
        {
            if(greenhouseIDValue.Text!=null && sensorIDValue.Text!=null && measureValue.Text!=null)
            {
                await PostMeasurement(datePicker.Date, Convert.ToInt16(greenhouseIDValue.Text), Convert.ToInt16(sensorIDValue.Text), Convert.ToDouble(measureValue.Text));
            }
            else
            {
                await DisplayAlert("Hiba", "Hibásan kitöltött mező!", "OK");
            }
            greenhouseIDValue.Text = null;
            sensorIDValue.Text = null;
            measureValue.Text = null;
        }
        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            if(referenceHumidity.Text!= null && referenceTemperature.Text != null)
            {
                await PostReferenceNumber(Convert.ToInt16(referenceTemperature.Text), Convert.ToInt16(referenceHumidity.Text));
            }
            else
            {
                await DisplayAlert("Hiba", "Hibásan kitöltött mező!", "OK");
            }
            referenceTemperature.Text = null;
            referenceHumidity.Text = null;
        }
        public static async Task PostReferenceNumber(double Temperature, double Humidity)
        {
            ReferenceValuesClient value = new ReferenceValuesClient { Id = 1 ,DateTime = DateTime.Now, Temperature = Temperature, Humidity = Humidity };
            string json = JsonConvert.SerializeObject(value);
            using (var client = new HttpClient())
            {
                var response = await client.DeleteAsync(
                    "https://measurementapi2020.azurewebsites.net/api/efreferencevalues/1");
            }
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "https://measurementapi2020.azurewebsites.net/api/efreferencevalues",
                     new StringContent(json, Encoding.UTF8, "application/json"));
            }
            
        }

        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filename = Path.Combine(path, "MeasuredDatas.txt");
            FileStream createFile = File.Open(filename, FileMode.Create);
            using (var streamWriter = new StreamWriter(createFile))
            {
                if(data.Count > 0)
                {
                    foreach (MeasurementClient mc in data)
                    {
                        streamWriter.WriteLine(mc.ToString());
                    }
                }
                else
                {
                    await DisplayAlert("Hiba", "Nincsen menteni való adat!","OK");
                    return;
                }
                
            }
            await DisplayAlert("Fájl mentése","A fájl mentésre került a következő helyen: " + path, "OK");
        }

        private async void Button_Clicked_3(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filename = Path.Combine(path, "MeasuredDatas.txt");
            string measuredDatas = "";
            using (var streamReader = new StreamReader(filename))
            {
                string content = streamReader.ReadToEnd();
                measuredDatas += content;
            }
            await DisplayAlert("Mentett fájl tartalma", measuredDatas, "Vissza");
        }
    }
}
