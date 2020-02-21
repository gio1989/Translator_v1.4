using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using translator.classes;
using Translator.Classes;

namespace Translator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ClipboardMonitor _clipboardMonitor;
        private string _proxyUserName;
        private string _proxyUserPassword;
        private string _lastClipboardtext;

        public MainWindow()
        {
            InitializeComponent();
            _clipboardMonitor = Resources["ClipWatch"] as ClipboardMonitor;

            var loginHelperObject = HelperMethods.LoadLoginHelperObjectJson();

            if (loginHelperObject != null && loginHelperObject.SavePasswordEnabled == "true")
            {
                txtProxyUserName.Text = loginHelperObject.UserName;
                txtProxyPassword.Password = loginHelperObject.Password;
                chBoxProxy.IsChecked = true;
                chBoxSaveCredentials.IsChecked = true;
            }

            Closed -= MainWindow_Closed;
            Closed += MainWindow_Closed;

            if (_clipboardMonitor != null)
            {
                //_clipboardMonitor.ClipboardData -= _clipboardMonitor_ClipboardData;
                _clipboardMonitor.ClipboardData += _clipboardMonitor_ClipboardData;
            }
        }

        private void _clipboardMonitor_ClipboardData(object sender, RoutedEventArgs e)
        {
            if (_clipboardMonitor.ClipboardContainsText)
            {
                try
                {
                    //if ((_lastClipboardtext != null && _lastClipboardtext.Equals(_clipboardMonitor.ClipboardText)) || (_clipboardMonitor.ClipboardText.Length > 200))
                    //    return;

                    if (_lastClipboardtext != null && (_clipboardMonitor.ClipboardText.Length > 200))
                        return;

                    _lastClipboardtext = _clipboardMonitor.ClipboardText;
                    //სასვენი ნიშნების მოცილება ტექსტიდან
                    _clipboardMonitor.ClipboardText = _clipboardMonitor.ClipboardText.Where(c => !char.IsPunctuation(c)).Aggregate("", (current, c) => current + c);
                    var selectedItem = SiteType.SelectedItem as ComboBoxItem;
                    if (selectedItem.Content.ToString() == "translate.ge")
                        TranslateByTranslateGe(_clipboardMonitor.ClipboardText);
                    else
                        TranslateByVoov(_clipboardMonitor.ClipboardText);
                }
                catch (Exception)
                {
                    txtBoxMain.Text = "ასეთი სიტყვა ვერ მოიძებნა";
                    labelMain.Text = "";
                }
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (_clipboardMonitor != null)
            {
                _clipboardMonitor.ClipboardData -= _clipboardMonitor_ClipboardData;
                _clipboardMonitor.Close();
            }
        }

        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }


        private void BtnExitClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void BtnMinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnPinChecked(object sender, RoutedEventArgs e)
        {
            Topmost = true;
        }

        private void BtnPin_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Topmost = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void OnProxyChecked(object sender, RoutedEventArgs e)
        {
            AnimateProxy(145);
        }

        private void OnProxyUnchecked(object sender, RoutedEventArgs e)
        {
            AnimateProxy(0);
            txtProxyPassword.Password = null;
            txtBoxMain.Text = null;
            //labelMain.Text = null;
            _proxyUserPassword = null;
        }

        private void AnimateProxy(int to)
        {
            var duration = new Duration(TimeSpan.FromSeconds(0.3));
            var da = new DoubleAnimation { Duration = duration };
            var sb = new Storyboard { Duration = duration };
            sb.Children.Add(da);
            Storyboard.SetTarget(da, ProxyGrid);
            Storyboard.SetTargetProperty(da, new PropertyPath(HeightProperty));
            da.To = to;

            sb.Completed += SbCompleted;

            sb.Begin();
        }

        private void SbCompleted(object sender, EventArgs e)
        {
            if (ProxyGrid.Height == 0)
                ProxyGrid.Visibility = Visibility.Collapsed;
            else
                ProxyGrid.Visibility = Visibility.Visible;

            ((ClockGroup)sender).Completed -= SbCompleted;
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (chBoxProxy.IsChecked.HasValue && chBoxProxy.IsChecked.Value)
            {
                if (string.IsNullOrEmpty(txtProxyUserName.Text) || string.IsNullOrEmpty(txtProxyPassword.Password))
                {
                    txtBoxMain.Text = "Proxy UserName or Password is Empty";
                    return;
                }

                _proxyUserName = txtProxyUserName.Text;
                _proxyUserPassword = txtProxyPassword.Password;

                AnimateProxy(0);
            }
        }

        private void OnBoxSaveCredentialsChecked(object sender, RoutedEventArgs e)
        {
            var jsonObject = new LoginHelperObject
            {
                SavePasswordEnabled = "true",
                UserName = txtProxyUserName.Text,
                Password = txtProxyPassword.Password
            };

            HelperMethods.WriteLoginHelperObjectJson(jsonObject);
        }

        private void OnBoxSaveCredentialsUnchecked(object sender, RoutedEventArgs e)
        {
            var jsonObject = new LoginHelperObject
            {
                SavePasswordEnabled = "false",
                UserName = "",
                Password = ""
            };

            HelperMethods.WriteLoginHelperObjectJson(jsonObject);
        }

        private void SiteType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtBoxMain != null && labelMain != null)
            {
                txtBoxMain.Text = string.Empty;
                labelMain.Text = string.Empty;
            }
        }

        private void TranslateByVoov(string selectedText)
        {
            try
            {
                var trimmedParam = !string.IsNullOrEmpty(selectedText) ? selectedText.Trim() : string.Empty;

                var uri = new Uri(HttpUtility.UrlDecode(string.Format("http://translate.voov.me/Words/{0}", trimmedParam)));
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                WebResponse resp = request.GetResponse();
                using (var sr = new System.IO.StreamReader(resp.GetResponseStream()))
                {
                    string responceText = string.Empty;

                    responceText = sr.ReadToEnd().Trim().ToString();
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(responceText);
                    var finalAnswer = "";
                    var answer = "";
                    var itemList = doc.DocumentNode.SelectNodes("//ul[contains(@class, 'sense-entry')]");

                    if (itemList != null)
                    {
                        foreach (var item in itemList)
                        {
                            answer += item.InnerText + Environment.NewLine + Environment.NewLine;
                        }
                    }
                    finalAnswer = Environment.NewLine + answer;
                    txtBoxMain.Text = finalAnswer;
                }
            }
            catch (Exception ex)
            {
                txtBoxMain.Text = "ასეთი სიტყვა ვერ მოიძებნა";
                labelMain.Text = "";
            }
        }

        private void TranslateByTranslateGe(string selectedText)
        {
            try
            {
                var trimmedParam = !string.IsNullOrEmpty(selectedText) ? selectedText.Trim() : string.Empty;

                var uri = new Uri(HttpUtility.UrlDecode(string.Format("http://www.translate.ge/api/{0}", trimmedParam)));
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                WebResponse resp = request.GetResponse();
                using (var sr = new System.IO.StreamReader(resp.GetResponseStream()))
                {
                    string responceText = string.Empty;

                    responceText = sr.ReadToEnd().Trim().ToString();
                    var parseTree = JsonConvert.DeserializeObject<Rootobject>(responceText);

                    string responseString = string.Empty;

                    if (parseTree != null && parseTree.Rows != null && parseTree.Rows.Any())
                    {
                        foreach (var responseObject in parseTree.Rows)
                        {
                            responseString += string.Format("{0} - {1}", responseObject.Value.Word, responseObject.Value.Text);
                            responseString += Environment.NewLine;
                            responseString += "-----------------";
                            responseString += Environment.NewLine;
                        }

                        txtBoxMain.Text = responseString;
                    }
                    else
                    {
                        txtBoxMain.Text = "ასეთი სიტყვა ვერ მოიძებნა";
                        labelMain.Text = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                txtBoxMain.Text = "ასეთი სიტყვა ვერ მოიძებნა";
                labelMain.Text = string.Empty;
            }
        }


    }
}
