using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Info.Blockchain.API.BlockExplorer;
using Info.Blockchain.API.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlockChainExplorer1.Pages
{
    public class IndexModel : PageModel
    {
        private object _object;
        public PropertyInfo[] Props { get; private set; }

        public async Task OnGet()
        {
            var explorer = new BlockExplorer();
            var latestBlock = await explorer.GetLatestBlockAsync();
            var block = await explorer.GetBlockByHashAsync(latestBlock.Hash);
            Save(block);
        }

        private void Save(object obj)
        {
            var type = obj.GetType();
            Props = type.GetProperties();
            _object = obj;
        }

        public string GetValue(PropertyInfo prop)
        {
            var value = prop.GetValue(_object);
            if (prop.Name == "Transactions")
            {
                var transactions = (IEnumerable<object>)value;
                return string.Join(',', transactions.Select(t=>((Transaction)t).Index));
            }
            return value.ToString();
        }
    }
}
