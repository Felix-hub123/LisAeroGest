namespace LisAeroGest.Data.Entities
{
    public interface ISoftDelete
    {
        bool WasDeleted { get; set; }
    }
}
