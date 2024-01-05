namespace Backend.Fx.Persistence.IdGeneration;

public interface IIdGenerator<out TId> 
{
    TId NextId();
}