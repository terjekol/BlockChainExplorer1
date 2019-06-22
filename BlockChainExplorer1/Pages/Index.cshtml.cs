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
            new Navigation("Transaction", "Hash", "GetTransactionByHash"),
            new Navigation("Transaction", "Index", "GetTransactionByIndex"),
            new Navigation("Block", "Height", "BlocksAtHeight"),
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
            var param = action.GetParameters().FirstOrDefault();
            var paramsObj = param == null ? new object[] { } : new[] { ConvertValue(paramValue, param.ParameterType) };
            var task = action.Invoke(explorer, paramsObj) as Task;
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            var obj = resultProperty.GetValue(task);
            obj = await IfLatestBlock(obj, explorer);
            Save(obj);
        }

        private object ConvertValue(string value, Type type)
        {
            if (type == typeof(long)) return Convert.ToInt64(value);
            if (type != typeof(DateTime)) return value;
            DateTime.TryParse(value, out var result);
            return result;
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

        public string GetNavigationActionName(string propertyName, object obj = null)
        {
            if (obj == null) obj = _object;
            return _navigations.FirstOrDefault(n => n.Type == obj.GetType().Name && n.PropertyName == propertyName)?.ActionName;
        }

        public string InputTypeFromCsType(Type t)
        {
            if (t == typeof(DateTime)) return "datetime";
            if (t == typeof(long)) return "number";
            return "text";
        }

        public object GetCollectionElement(PropertyInfo prop)
        {
            var collection = prop.GetValue(_object) as IEnumerable<object>;
            if (collection == null) return null;
            return collection.FirstOrDefault();
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
