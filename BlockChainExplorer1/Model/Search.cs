using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlockChainExplorer1.Model
{
    public class Search
    {
        public long Id { get; set; }
        public string ActionName { get; set; }
        public string ParamValue { get; set; }
        public string User { get; set; }
        [NotMapped]
        public List<int> Indexes { get; set; }

        public int GetCollectionNo(int index)
        {
            return index >= Indexes.Count ? 1 : Indexes[index] + 1;
        }

        public object GetParamsObj(int delta, int index = 0)
        {
            while (Indexes.Count < index + 1) Indexes.Add(0);
            var indexes = Indexes.ToArray();
            indexes[index] += delta;

            return new
            {
                actionName = ActionName,
                paramValue = ParamValue,
                indexes = indexes
            };
        }
    }
}
