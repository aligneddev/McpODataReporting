using System.ComponentModel.DataAnnotations;

namespace ODataApi.Data;

public partial class BuildVersion
{
    [Key]
    public byte SystemInformationId { get; set; }

    public string DatabaseVersion { get; set; } = null!;

    public DateTime VersionDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}
