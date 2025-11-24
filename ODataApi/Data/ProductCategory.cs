using System.ComponentModel.DataAnnotations;

namespace ODataApi.Data;

public partial class ProductCategory
{
    [Key]
    public int ProductCategoryId { get; set; }

    public int? ParentProductCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<ProductCategory> InverseParentProductCategory { get; set; } = new List<ProductCategory>();

    public virtual ProductCategory? ParentProductCategory { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
