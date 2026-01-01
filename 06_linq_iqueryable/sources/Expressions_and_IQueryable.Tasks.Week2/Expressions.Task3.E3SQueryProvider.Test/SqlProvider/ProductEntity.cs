using System.ComponentModel.DataAnnotations.Schema;

namespace Expressions.Task3.E3SQueryProvider.Test.SqlProvider;

[Table("Products", Schema = "dbo")]
public class ProductEntity
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public string ProductType { get; set; }
    public int UnitsInStock { get; set; }
}
