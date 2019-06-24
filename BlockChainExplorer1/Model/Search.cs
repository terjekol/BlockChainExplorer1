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

        public int MainCollectionNo => Indexes.Count == 0 ? 1 : Indexes[0] + 1;

        public object GetParamsObj(int delta, int index = 0)
        {
            while (Indexes.Count < index + 1) Indexes.Add(0);
            Indexes[index] += delta;

            return new
            {
                actionName = ActionName,
                paramValue = ParamValue,
                indexes = Indexes.ToArray()
            };
        }
    }
}
