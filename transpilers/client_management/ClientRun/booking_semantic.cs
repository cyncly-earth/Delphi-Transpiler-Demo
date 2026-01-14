public static class BookingSemantic
{
    public static ScreenIR Analyze(FormNode form)
    {
        var screen = new ScreenIR
        {
            ScreenName = form.Name
        };

        foreach (var field in form.Fields)
        {
            screen.Fields.Add(new FieldIR
            {
                Name = ToCamel(field.Name),
                Type = "string"
            });
        }

        foreach (var btn in form.Buttons)
        {
            screen.Actions.Add(new ActionIR
            {
                Name = ToCamel(btn.Name),
                Calls = "AddBooking"
            });
        }

        return screen;
    }

    static string ToCamel(string name)
        => char.ToLower(name[0]) + name.Substring(1);
}
