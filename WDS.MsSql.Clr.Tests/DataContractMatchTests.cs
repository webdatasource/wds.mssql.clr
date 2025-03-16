using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace WDS.MsSql.Clr.Tests
{
    [TestFixture]
    public class DataContractMatchTests
    {
        private static readonly Assembly _dapiAssembly = Assembly.LoadFile(Path.GetFullPath("../../../../wds.kesa.dotnet/Dapi/WDS.Dapi.Core/bin/Debug/net8.0/WDS.Dapi.Core.dll"));

        [Test]
        // [TestCase("WDS.Dapi.Core.PublicDataContracts.Input.Jobs.JobConfig", typeof(JobConfig))]
        // [TestCase("WDS.Dapi.Core.PublicDataContracts.Output.Tasks.DownloadTask", typeof(DownloadTask))]
        [TestCase("WDS.Dapi.Core.PublicDataContracts.Output.Tasks.Info.DownloadTaskStatus", typeof(DownloadTaskStatus))]
        public void JobConfigTest(string dapiDataContractTypeFullName, Type thisDataContractType)
        {
            var dapiDataContractType = _dapiAssembly.GetType(dapiDataContractTypeFullName);
            CompareProperties(dapiDataContractType, thisDataContractType);
        }

        private static void CompareProperties(Type apiType, Type clrType)
        {
            var apiProperties = apiType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var clrProperties = clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var apiProperty in apiProperties)
            {
                var clrProperty = clrProperties.SingleOrDefault(p => p.Name == apiProperty.Name);
                Assert.IsNotNull(clrProperty, $"Property '{apiProperty.Name}' in type '{apiType.FullName}' is missing in the CLR library.");
                if (apiProperty.PropertyType.IsEnum)
                    CompareEnums(apiProperty.PropertyType, clrProperty.PropertyType);
                else if (Nullable.GetUnderlyingType(apiProperty.PropertyType)?.IsEnum == true)
                    CompareEnums(Nullable.GetUnderlyingType(apiProperty.PropertyType), Nullable.GetUnderlyingType(clrProperty.PropertyType));
                else if (apiProperty.PropertyType.IsClass && apiProperty.PropertyType != typeof(string))
                    CompareProperties(apiProperty.PropertyType, clrProperty.PropertyType);
                else
                    Assert.AreEqual(apiProperty.PropertyType, clrProperty.PropertyType, $"Type mismatch in property '{apiProperty.Name}' of type '{apiType.FullName}'. API type: {apiProperty.PropertyType}, CLR type: {clrProperty.PropertyType}.");
            }
        }

        private static void CompareEnums(Type apiType, Type clrType)
        {
            var apiEnumValues = Enum.GetNames(apiType);
            var clrEnumValues = Enum.GetNames(clrType);
            foreach (var apiEnumValue in apiEnumValues)
                Assert.Contains(apiEnumValue, clrEnumValues, $"Enum value '{apiEnumValue}' of type '{apiType}' is missing in the CLR library.");
        }
    }
}