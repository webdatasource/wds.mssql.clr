namespace WDS.MsSql.Clr.Serialization.DataContracts
{
    public interface IDataContractAssign<in TRes>
    {
        void AssignProp(TRes res, string value);
    }
}