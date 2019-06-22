using System.Reflection;
using Info.Blockchain.API.BlockExplorer;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlockChainExplorer1.Pages
{
    public class IndexModel : PageModel
    {
        private object _object;

        public async void OnGet()
        {
            var explorer = new BlockExplorer();
            var latestBlock = await explorer.GetLatestBlockAsync();
            _object = await explorer.GetBlockByHashAsync(latestBlock.Hash);
        }

        public PropertyInfo[] Props => _object.GetType().GetProperties(BindingFlags.Public);
        public string GetValue(PropertyInfo prop) => prop.GetValue(_object).ToString();
    }
}
