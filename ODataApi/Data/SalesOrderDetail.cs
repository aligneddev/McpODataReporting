using System.ComponentModel.DataAnnotations;

namespace ODataApi.Data;

public partial class SalesOrderDetail
{
    [Key]
    public int SalesOrderId { get; set; }

    [Key]
    public int SalesOrderDetailId { get; set; }

    public short OrderQty { get; set; }

    public int ProductId { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal UnitPriceDiscount { get; set; }

    public decimal LineTotal { get; set; }

    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual SalesOrderHeader SalesOrder { get; set; } = null!;
}
