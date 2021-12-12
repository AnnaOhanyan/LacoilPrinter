using System.Collections.Generic;

namespace LACoil.Models
{
    public class FindCardModel
    {
        public string Name { get; set; }
        public string ProductName { get; set; }
        public Currency Currency { get; set; }
        public int Type { get; set; }
        public string CarNumber { get; set; }
        public string CustomerName { get; set; }
        public List<ValueListItem<int, string>> Products { get; set; }
        public double? Amount { get; set; }
    }

    public enum Currency
    {
        AMD = 1,
        RUB = 2,
    }
    public class ValueListItem<TValue, TText>
    {
        public ValueListItem(TValue value, TText text)
        {
            Id = value;
            Text = text;
        }

        public TValue Id { get; set; }
        public TText Text { get; set; }
    }

}