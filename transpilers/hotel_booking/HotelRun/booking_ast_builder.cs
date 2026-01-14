public static class BookingAstBuilder
{
    public static FormNode Build()
    {
        // MOCKED: normally built from ANTLR parse tree
        return new FormNode
        {
            Name = "BookingForm",
            Fields =
            {
                new FieldNode { Name = "FirstName" },
                new FieldNode { Name = "LastName" }
            },
            Buttons =
            {
                new ButtonNode { Name = "Save" }
            }
        };
    }
}
