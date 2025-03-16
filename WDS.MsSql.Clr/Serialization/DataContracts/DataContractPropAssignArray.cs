using System;

namespace WDS.MsSql.Clr.Serialization.DataContracts
{
    public class DataContractPropAssignArray<TRes, TProp> : IDataContractAssign<TRes>
    {
        private readonly Action<TRes, TProp[]> _assignProp;
        private static readonly char[] _parseGroupItemValuesSeparator = { ',' };

        public DataContractPropAssignArray(Action<TRes, TProp[]> assignProp)
        {
            _assignProp = assignProp;
        }

        public void AssignProp(TRes res, string value)
        {
            var stringValues = value.Split(_parseGroupItemValuesSeparator, StringSplitOptions.RemoveEmptyEntries);
            var values = new TProp[stringValues.Length];
            for (var i = 0; i < stringValues.Length; i++)
                values[i] = (TProp)Convert.ChangeType(stringValues[i].Trim(), typeof(TProp));
            _assignProp(res, values);
        }
    }
}