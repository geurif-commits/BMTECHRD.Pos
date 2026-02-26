namespace BMTECHRD.Pos.Domain.Common;
public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}