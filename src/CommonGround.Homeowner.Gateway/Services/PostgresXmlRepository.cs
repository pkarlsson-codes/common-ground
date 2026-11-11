using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace CommonGround.Homeowner.Gateway.Services;

/// <summary>
/// Keeps the ASP.NET Data Protection key ring in <c>data_protection_keys</c>, so auth
/// cookies stay valid when the gateway restarts.
/// </summary>
public sealed class PostgresXmlRepository(GatewayDatabase database) : IXmlRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        using var command = database.DataSource.CreateCommand(
            $"SELECT xml FROM {GatewayDatabase.Schema}.data_protection_keys ORDER BY id");
        using var reader = command.ExecuteReader();
        var elements = new List<XElement>();
        while (reader.Read())
        {
            elements.Add(XElement.Parse(reader.GetString(0)));
        }

        return elements;
    }

    /// <inheritdoc/>
    public void StoreElement(XElement element, string friendlyName)
    {
        ArgumentNullException.ThrowIfNull(element);

        using var command = database.DataSource.CreateCommand(
            $"INSERT INTO {GatewayDatabase.Schema}.data_protection_keys (friendly_name, xml) VALUES ($1, $2)");
        command.Parameters.AddWithValue((object?)friendlyName ?? DBNull.Value);
        command.Parameters.AddWithValue(element.ToString(SaveOptions.DisableFormatting));
        command.ExecuteNonQuery();
    }
}
