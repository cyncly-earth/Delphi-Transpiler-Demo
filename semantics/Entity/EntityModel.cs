public sealed class EntityModel
{
    public List<EntityDefinition> Entities { get; } = new();
}

public sealed class EntityDefinition
{
    public string Name { get; set; }
    public List<EntityField> Fields { get; } = new();
}

public sealed class EntityField
{
    public string Name { get; set; }
    public string Type { get; set; }
}
