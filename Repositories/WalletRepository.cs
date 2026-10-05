using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace X_menu_app.Repositories
{
    public static class WalletRepository
    {
        private static string GetFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "X_menu_app");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, "wallet.json");
        }

        public static List<Models.Transaction> Load()
        {
            try
            {
                var path = GetFilePath();
                if (!File.Exists(path)) return new List<Models.Transaction>();
                var json = File.ReadAllText(path);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = JsonSerializer.Deserialize<List<Models.Transaction>>(json, opts);
                return list ?? new List<Models.Transaction>();
            }
            catch
            {
                return new List<Models.Transaction>();
            }
        }

        public static void Save(IEnumerable<Models.Transaction> transactions)
        {
            try
            {
                var path = GetFilePath();
                var opts = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(transactions, opts);
                File.WriteAllText(path, json);
            }
            catch
            {
                // ignore failures for now
            }
        }
    }
}
