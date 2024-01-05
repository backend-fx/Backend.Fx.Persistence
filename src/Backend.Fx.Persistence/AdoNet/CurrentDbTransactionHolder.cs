using System;
using System.Data;
using Backend.Fx.Util;
using JetBrains.Annotations;

namespace Backend.Fx.Persistence.AdoNet
{
    [UsedImplicitly]
    public class CurrentDbTransactionHolder : CurrentTHolder<IDbTransaction>, ICurrentTHolder<IDbTransaction>
    {
        public override IDbTransaction ProvideInstance()
        {
            throw new NotSupportedException("The current DB transaction cannot be created on the fly");
        }

        protected override string Describe(IDbTransaction? instance)
        {
            return instance == null ? "<NULL>" : $"DbTransaction #{instance.GetHashCode()}";   
        }
    }
}