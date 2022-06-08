using System;
using System.Collections.Generic;

namespace _Dictionary
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> d6 = new Dictionary<string, string>() { { "AA", "ABB" }, { "AB", "ABC" } };
            _Dictionary d1 = new _Dictionary();
            _Dictionary d2 = new _Dictionary(5);
            _Dictionary d3 = new _Dictionary(EqualityComparer<string>.Default);
            _Dictionary d4 = new _Dictionary(6, EqualityComparer<string>.Default);
            _Dictionary d5 = new _Dictionary(d6);
            _Dictionary d7 = new _Dictionary(d6, EqualityComparer<string>.Default);
            d1.Add("AC", "ACD");
            d2.Add("AD", "AED");
            d3.Add("AC", "ACD");
            d4.Add("AG", "AET");
            d5.Add("AF", "ATD");
            Console.WriteLine(d1["AC"]);
            d2["AD"] = "AAA";
            Console.WriteLine(d5.Count);
            d1.Clear();
            bool b = d5.ContainsValue("ATDD");
            bool c = d3.Remove("AC");
            foreach (var item in d5)
            {
                Console.WriteLine($"Key: {item.Key}  Value: {item.Value}");
            }
        }
    }
}
