using System.ComponentModel.DataAnnotations;

namespace ODataApi.Data;

public partial class ProductModelProductDescription
{
    [Key]
    public int ProductModelId { get; set; }

    [Key]
    public int ProductDescriptionId { get; set; }

    [Key]
    public string Culture { get; set; } = null!;

    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual ProductDescription ProductDescription { get; set; } = null!;

    public virtual ProductModel ProductModel { get; set; } = null!;
}
