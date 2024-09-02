namespace Spectre.Console.Tests.Unit;

public sealed class SelectionPromptTests
{
    private const string ESC = "\u001b";

    private class TestClass
    {
        public string Name { get; init; }
    }

    [Fact]
    public void Should_Not_Throw_When_Selecting_An_Item_With_Escaped_Markup()
    {
        // Given
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushKey(ConsoleKey.Enter);
        var input = "[red]This text will never be red[/]".EscapeMarkup();

        // When
        var prompt = new SelectionPrompt<string>()
                .Title("Select one")
                .AddChoices(input);
        prompt.Show(console);

        // Then
        console.Output.ShouldContain(@"[red]This text will never be red[/]");
    }

    [Fact]
    public void Should_Select_The_First_Leaf_Item()
    {
        // Given
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushKey(ConsoleKey.Enter);

        // When
        var prompt = new SelectionPrompt<string>()
                .Title("Select one")
                .Mode(SelectionMode.Leaf)
                .AddChoiceGroup("Group one", "A", "B")
                .AddChoiceGroup("Group two", "C", "D");
        var selection = prompt.Show(console);

        // Then
        selection.ShouldBe("A");
    }

    [Fact]
    public void Should_Search_In_Remapped_Object_Result()
    {

        // Given
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.EmitAnsiSequences();
        console.Input.PushText("2");
        console.Input.PushKey(ConsoleKey.Enter);
        var myObject = new List<TestClass>{ new() { Name = "Item 1" }, new() { Name = "Item 2" } };

        // When
        var prompt = new SelectionPrompt<TestClass>()
            .Title("Select one")
            .EnableSearch()
            .UseConverter(o => o.Name)
            .AddChoices(myObject);
        var selection = prompt.Show(console);

        // Then
        selection.Name.ShouldBe("Item 2");
    }

    [Fact]
    public void Should_Select_The_Last_Leaf_Item_When_Wrapping_Around()
    {
        // Given
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushKey(ConsoleKey.UpArrow);
        console.Input.PushKey(ConsoleKey.Enter);

        // When
        var prompt = new SelectionPrompt<string>()
            .Title("Select one")
            .Mode(SelectionMode.Leaf)
            .WrapAround()
            .AddChoiceGroup("Group one", "A", "B")
            .AddChoiceGroup("Group two", "C", "D");
        var selection = prompt.Show(console);

        // Then
        selection.ShouldBe("D");
    }

    [Fact]
    public void Should_Highlight_Search_Term()
    {
        // Given
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.EmitAnsiSequences();
        console.Input.PushText("1");
        console.Input.PushKey(ConsoleKey.Enter);

        // When
        var prompt = new SelectionPrompt<string>()
            .Title("Select one")
            .EnableSearch()
            .AddChoices("Item 1");
        prompt.Show(console);

        // Then
        console.Output.ShouldContain($"{ESC}[38;5;12m> Item {ESC}[0m{ESC}[1;38;5;12;48;5;11m1{ESC}[0m");
    }
}
