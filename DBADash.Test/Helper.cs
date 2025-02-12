﻿using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics;
using System.IO;

namespace DBADashConfig.Test
{
    internal class Helper
    {
        public static string GetConfigJson()
        {
            return File.ReadAllText(Initialize.ServiceConfigPath);
        }


        public static void RunProcess(ProcessStartInfo psi)
        {
            psi.CreateNoWindow = false;
            psi.UseShellExecute = false;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            using var p = Process.Start(psi);
            if (p == null)
            {
                throw new Exception("Process is NULL");
            }
            p.WaitForExit();
            var output = p.StandardOutput.ReadToEnd();
            var error = p.StandardError.ReadToEnd();
            Console.WriteLine(output);
            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }
        }

        public static int GetConnectionCount()
        {
            if (File.Exists(Initialize.ServiceConfigPath))
            {
                string json = GetConfigJson();
                var cfg = DBADash.CollectionConfig.Deserialize(json);
                return cfg.SourceConnections.Count;
            }
            else
            {
                return 0;
            }
        }

        public static SqlConnectionStringBuilder GetRandomConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = Guid.NewGuid().ToString(),
                InitialCatalog = "DABDashUnitTest" + Guid.NewGuid().ToString(),
                Password = "TestEncryption",
                ApplicationName = "DBADash"
            };
            return builder;
        }
    }
}
