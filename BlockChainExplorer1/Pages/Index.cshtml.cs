using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Info.Blockchain.API.BlockExplorer;
using Info.Blockchain.API.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Xml;
using Newtonsoft.Json;

namespace BlockChainExplorer1.Pages
{
    public class IndexModel : PageModel
    {
        private object _object;
        public string ActionName { get; private set; }
        public PropertyInfo[] SimpleProps { get; private set; }
        public PropertyInfo[] CollectionProps { get; private set; }
        public IEnumerable<MethodInfo> Actions { get; private set; }
        public IEnumerable<string> ActionNames { get; private set; }

        private readonly Navigation[] _navigations =
        {
            new Navigation("Block", "PreviousBlockHash", "GetBlockByHash"),
        };


        public async Task OnGet(string actionName, string paramValue)
        {
            ActionName = ShortenName(actionName);
            Actions = typeof(BlockExplorer)
                .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetParameters().Length <= 1);
            ActionNames = Actions.Select(m => ShortenName(m.Name));
            if (actionName == null) return;
            var action = Actions.SingleOrDefault(a => a.Name.Contains(actionName));
            if (action == null) return;
            var explorer = new BlockExplorer();
            var hasParam = action.GetParameters().Length > 0;
            var param = hasParam ? new object[] { paramValue } : new object[] { };
            var task = action.Invoke(explorer, param) as Task;
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            var obj = resultProperty.GetValue(task);
            obj = await IfLatestBlock(obj, explorer);
            Save(obj);
        }

        private string ShortenName(string s)
        {
            if (s == null) return s;
            return s.Replace("Get", "").Replace("Async", "");
        }

        private async Task<object> IfLatestBlock(object obj, BlockExplorer explorer)
        {
            var latestBlock = obj as LatestBlock;
            if (latestBlock == null) return obj;
            return await explorer.GetBlockByHashAsync(latestBlock.Hash);
        }

        private void Save(object obj)
        {
            var type = obj.GetType();
            var allProps = type.GetProperties();
            SimpleProps = allProps.Where(p => !p.PropertyType.Name.Contains("Collection")).ToArray();
            CollectionProps = allProps.Where(p => p.PropertyType.Name.Contains("Collection")).ToArray();
            _object = obj;
        }

        public string GetValue(PropertyInfo prop)
        {
            return prop.GetValue(_object).ToString();
        }

        public string GetNavigationActionName(string propertyName)
        {
            return _navigations.FirstOrDefault(n => n.Type == _object.GetType().Name && n.PropertyName == propertyName)?.ActionName;
        }

        public string InputTypeFromCsType(Type t)
        {
            if (t == typeof(DateTime)) return "datetime";
            if (t == typeof(long)) return "number";
            return "text";
        }

        public PropertyInfo[] GetCollectionElementProps(PropertyInfo prop)
        {
            var collection = prop.GetValue(_object) as IEnumerable<object>;
            if (collection == null) return null;
            var element = collection.FirstOrDefault();
            if (element == null) return null;
            return element.GetType().GetProperties();
        }
    }

    public class Navigation
    {
        public string Type;
        public string PropertyName;
        public string ActionName;

        public Navigation(string type, string propertyName, string actionName)
        {
            Type = type;
            PropertyName = propertyName;
            ActionName = actionName;
        }
    }
}
