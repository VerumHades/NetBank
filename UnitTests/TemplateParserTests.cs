using NetBank.Parsing;
using NetBank.Types;

namespace UnitTests;

public class TemplateTests
{
    public record ParserTestDto
    {
        public static string Template = "CMD:{Command} ID/{Id} VAL:{Value}";
    
        public string Command { get; set; } = string.Empty;
        public int Id { get; set; }
        public long Value { get; set; }
    }
    
    [Fact]
    public void Parse_ValidString_PopulatesAllTypes()
    {
        // Arrange
        var parser = new Template<ParserTestDto>(ParserTestDto.Template);
        var input = "CMD:WITHDRAW ID/10500 VAL:250";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("WITHDRAW", result.Command);
        Assert.Equal(10500, result.Id);
        Assert.Equal(250, result.Value); // Assuming Amount.Value returns decimal
    }

    [Fact]
    public void Parse_PlaceholderAtEnd_CapturesRemainingString()
    {
        // Arrange: Template ends with a placeholder {Value}
        var parser = new Template<ParserTestDto>("OP {Command} {Value}");
        var input = "OP DEPOSIT 1000";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("DEPOSIT", result.Command);
        Assert.Equal(1000m, result.Value);
    }

    [Fact]
    public void Parse_EmptyMidValue_HandlesMissingData()
    {
        var parser = new Template<ParserTestDto>(ParserTestDto.Template);
        var input = "CMD:CHECK ID/ VAL:0";
        
        Assert.ThrowsAny<Exception>(() => parser.Parse(input));
    }

    [Fact]
    public void Parse_MismatchedLiteral_ThrowsFormatException()
    {
        // Arrange: Template expects 'ID/'
        var parser = new Template<ParserTestDto>(ParserTestDto.Template);
        var input = "CMD:WITHDRAW ID-10500 VAL:250"; // Using '-' instead of '/'

        // Act & Assert
        var ex = Assert.Throws<FormatException>(() => parser.Parse(input));
        Assert.Contains("Input does not match template", ex.Message);
    }

    [Fact]
    public void Parse_InvalidNumericData_ThrowsOnConversion()
    {
        // Arrange
        var parser = new Template<ParserTestDto>(ParserTestDto.Template);
        var input = "CMD:TEST ID/NotANumber VAL:100";

        // Act & Assert
        // Since the setter is a compiled expression calling int.Parse, 
        // it will throw a FormatException or TargetInvocationException
        Assert.ThrowsAny<Exception>(() => parser.Parse(input));
    }

    [Fact]
    public void Parse_CaseInsensitivity_MapsPlaceholdersCorrectly()
    {
        // Arrange: Template uses lowercase, DTO uses PascalCase
        var parser = new Template<ParserTestDto>("cmd:{command} id/{id}");
        var input = "cmd:RECOVERY id/99";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal("RECOVERY", result.Command);
        Assert.Equal(99, result.Id);
    }
}