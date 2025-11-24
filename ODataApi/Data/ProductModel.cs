using System.ComponentModel.DataAnnotations;

namespace ODataApi.Data;

public partial class ProductModel
{
    [Key]
    public int ProductModelId { get; set; }

    public string Name { get; set; } = null!;

    public string? CatalogDescription { get; set; }

    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<ProductModelProductDescription> ProductModelProductDescriptions { get; set; } = new List<ProductModelProductDescription>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
