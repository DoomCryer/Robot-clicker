using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using Newtonsoft.Json;

namespace Robot_clicker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private ChromiumWebBrowser chrome;
        private string[] matches;
        private static Timer timer = new Timer();
        private string[] hashSet = Array.Empty<string>();

        private void Form1_Load(object sender, EventArgs e)
        {
            CefSettings settings = new CefSettings();
            Cef.Initialize(settings);
            txtUrl.Text = "http://client.work-zilla.com/executive.aspx";
            keyWords.Text = @"Создать мотивашки в группе";
            chrome = new ChromiumWebBrowser(txtUrl.Text);
            this.pContainer.Controls.Add(chrome);
            chrome.Dock = DockStyle.Fill;
            chrome.AddressChanged += Chrome_AddressChanged;
        }

        private void Chrome_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            Invoke(new MethodInvoker(() =>
            {
                txtUrl.Text = e.Address;
            }));
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            chrome.Load(txtUrl.Text);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            timer.Interval = 5000;
            timer.Tick += Loop;
            timer.Start();

            //var js = PrepareJS();
            //var task = chrome.EvaluateScriptAsync(js);
            //task.ContinueWith(t =>
            //{
            //    var responce = JsonConvert.SerializeObject(t.Result.Result);
            //    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responce);
            //    hashSet = result.Keys.ToArray();
            //});
        }

        private void Loop(object sender, EventArgs e)
        {
            var js = PrepareJS();
            var task = chrome.EvaluateScriptAsync(js);
            task.ContinueWith(t =>
            {
                var responce = JsonConvert.SerializeObject(t.Result.Result);
                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responce);
                hashSet = result.Keys.ToArray();
            });
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            chrome.Refresh();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if(chrome.CanGoForward)
                chrome.Forward();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if(chrome.CanGoBack)
                chrome.Back();
        }

        private void btnDev_Click(object sender, EventArgs e)
        {
            chrome.ShowDevTools();
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
        }

        private string PrepareJS()
        {
            var keys = keyWords.Text.Split(',').Select(s=> s.Trim()).ToArray();
            var search = string.Join(",", keys.Select(s => $"\"{s}\"").ToArray());

            var sb = new StringBuilder();
            sb.Append(@"
                var HashSet = function() {
                var set = { };
                this.add = function(key)
                    {
                        set[key] = true;
                    };
                this.remove = function(key)
                    {
                        delete set[key];
                    };
                this.contains = function(key)
                    {
                        return set.hasOwnProperty(key);
                    };
                this.showAll = function(){
                      return set;
                    };
                };
                var set = new HashSet();
                var search = [");
            sb.Append(search);
            sb.Append(@"
                ];
                function findOrders(string)
                    {
                        var matches = document.evaluate(""//span[contains(., '"" + string + ""')]"", document.documentElement, null,
                      XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null);
                        var orders = [];
                        for (var i = 0; i < matches.snapshotLength; i++)
                        {
                            var el = matches.snapshotItem(i);
                            while ((el = el.parentElement) && !el.classList.contains(""workorder"")) ;
                            orders.push(el);
                        }
                        return orders;
                    }
            ");
            foreach (var s in hashSet)
            {
                sb.AppendLine($"set.add(\"{s}\");");
            }
            sb.Append(@"
                function main(){
                    console.log(""NEW ITERATION"");
                    for (var j = 0; j<search.length; j++) {
                        var orders = findOrders(search[j]);
                        for (var i = 0; i<orders.length; i++) {
                            var order = orders[i];
                            if (set.contains(order.id)) {
                                continue;
                            }
                            console.log(order.id);
                            set.add(order.id);
                            order.getElementsByClassName(""wo-accept"")[0].dispatchEvent(new MouseEvent(""click"", {
                                ""view"": window,
                                        ""bubbles"": true,
                                        ""cancelable"": false
                                    }));

                            order.getElementsByClassName(""cf-ok"")[0].dispatchEvent(new MouseEvent(""click"", {
                                ""view"": window,
                                        ""bubbles"": true,
                                        ""cancelable"": false
                                    }));

                            order.getElementsByTagName(""textarea"")[0].value = ""Привет! У меня есть вопрос!"";
                        }
                    }
                    return set.showAll();
                }
                main();"
            );


            return sb.ToString();
        }
    }
}
