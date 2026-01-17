using DelphiTranspiler.Semantics.SemanticModels;

public static class EntityModelBuilder
{
    public static EntityModel BuildEntityModel(
        IReadOnlyDictionary<string, SemanticType> types)
    {
        var model = new EntityModel();

        foreach (var type in types.Values)
        {
            // ❌ Ignore UI classes
            if (!(type is ClassType classType))
                continue;

            if (classType.Name.Contains("PersonView"))
                continue;

            var entity = new EntityDefinition
            {
                Name = classType.Name
            };

            foreach (var field in classType.Fields)
        {
            entity.Fields.Add(new EntityField
            {
                Name = NormalizeField(field.Key),
                Type = MapType(field.Value)
            });
        }

        model.Entities.Add(entity);
    }

        return model;
    }

    private static string NormalizeField(string name)
{
    // cFirst → first
    if (name.StartsWith("c") && name.Length > 1)
        return name.Substring(1).ToLower();

    return name.ToLower();
}

    private static string MapType(string type)
    {
        return type switch
        {
            "Integer" => "int",
            "String" => "string",
            _ => "string"
        };
    }
}
