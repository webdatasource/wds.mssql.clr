using System;

namespace WDS.MsSql.Clr.Serialization.DataContracts
{
    public class DataContractPropAssignValue<TRes, TProp> : IDataContractAssign<TRes>
    {
        private readonly Action<TRes, TProp> _assignProp;
        
        public DataContractPropAssignValue(Action<TRes, TProp> assignProp)
        {
            _assignProp = assignProp;
        }
        
        public void AssignProp(TRes res, string value)
        {
            var val = (TProp)Convert.ChangeType(value, typeof(TProp));
            _assignProp(res, val);
        }
    }
}