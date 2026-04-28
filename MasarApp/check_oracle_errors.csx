#r "nuget: Microsoft.EntityFrameworkCore, 8.0.0"
#r "nuget: Oracle.EntityFrameworkCore, 8.23.60"

using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

var connStr = "Data Source=localhost:1521/XEPDB1;User Id=masar;Password=masar;"; // Try to guess connection string from logs?
// Actually we can read it from appsettings.json!
