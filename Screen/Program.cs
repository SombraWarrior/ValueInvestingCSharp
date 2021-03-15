using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using HtmlAgilityPack;
namespace Screen
{

    class Program
    {
        /// <summary>
        /// 通用價值股來自巴菲特股神其中一個選股法
        ///1.本益比 < 13
        ///2.股價淨值比 < 0.7
        ///3.上市股價為10元以上(避免水餃股)
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var httpClient = new HttpClient();
            // var date = DateTime.Now.AddDays(-3).ToString("yyyyMMdd");
            Console.Write("請輸入日期(yyyyMMdd)：");
            var date = Console.ReadLine();
            var response = httpClient.GetAsync("https://www.twse.com.tw/exchangeReport/BWIBBU_d?response=json&date="+ date +"&selectType=ALL&_=161").Result;
            Console.WriteLine("取得資料...");
            var responseBody = string.Empty;
            var result = new responseViewModel();
            var individualStockCompanyInfos = new List<IndividualStockCompResultViewModel>();
            if (response.IsSuccessStatusCode) 
            {
                Console.WriteLine("狀態：成功");
                responseBody = response.Content.ReadAsStringAsync().Result;
                
                result = JsonSerializer.Deserialize<responseViewModel>(responseBody);
                if (result.data != null) 
                {
                    Console.WriteLine("資料進行處理中，請稍後...");
                    result.data.ForEach(item => individualStockCompanyInfos.Add(new IndividualStockCompResultViewModel
                    {
                        Code = item[0].ToString(),
                        CompanyName = item[1].ToString(),
                        DividendYield = (item[2].ToString() == "-") ? 0.0 : double.Parse(item[2].ToString()),
                        PtE = (item[4].ToString() == "-") ? 0.0 : double.Parse(item[4].ToString()),
                        PB = (item[5].ToString() == "-") ? 0.0 : double.Parse(item[5].ToString()),
                        ReportYear = item[6].ToString(),
                        Price = Convert.ToDouble(GetPrice(item[0].ToString()))
                    }));

                  
                    individualStockCompanyInfos = individualStockCompanyInfos.Where(p => p.PtE < 13
                                                                                 && p.PB < 0.7 
                                                                                 && p.Price > 10).ToList();
                    //選股其實可以在優化，請各位自己思考吧...
                    Console.WriteLine($"代碼\t公司名稱\t殖利率\t本益比\t股價淨值比\t市價");
                    foreach (var item in individualStockCompanyInfos)
                    {
                        Console.WriteLine($"{item.Code}" +
                                         $"\t{item.CompanyName.PadLeft(6)}" +
                                         $"\t{item.DividendYield}" +
                                         $"\t{item.PtE}" +
                                         $"\t{item.PB}" +
                                         $"\t\t{item.Price}");
                    }
                }
            }
        }
        static string GetPrice(string stockNum)
        {
            var url = "https://tw.stock.yahoo.com/q/q?s=" + stockNum;
            string node = "/html/body/center/table[2]/tr/td/table/tr[2]/td[3]";
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var nameNode = doc.DocumentNode.SelectSingleNode(node);
            if (nameNode == null || nameNode.InnerText =="-")
                return "0";
            return nameNode.InnerText;
        }
    }
    public class responseViewModel
    {
        public string stat { get; set; }
        public string date { get; set; }
        public string title { get; set; }
        public List<string> fields { get; set; }
        public List<List<object>> data { get; set; }
        public string selectType { get; set; }
        public List<string> notes { get; set; }
     
    }
    public class IndividualStockCompResultViewModel
    {
        /// <summary>
        /// 股票號碼
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 公司名稱
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 殖利率
        /// </summary>
        public double DividendYield { get; set; }
      
        /// <summary>
        /// 本益比
        /// </summary>
        public double PtE { get; set; }
        /// <summary>
        /// 股價淨值比
        /// </summary>
        public double PB { get; set; }
        /// <summary>
        /// 財報年/季
        /// </summary>
        public string ReportYear { get; set; }

        public double Price { get; set; }
    }
}
