namespace Workplan.SharedKernel.Common;

// Result/Result<T> hiyerarşisinin generic kod tarafından reflection'sız Fail(Error)
// üretebilmesi için static abstract factory sözleşmesi (bkz. ValidationBehavior).
public interface IFailable<TSelf> where TSelf : IFailable<TSelf>
{
    static abstract TSelf Fail(Error error);
}
