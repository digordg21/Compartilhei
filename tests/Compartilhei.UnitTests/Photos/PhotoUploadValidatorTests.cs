using Compartilhei.Application.Photos.Validation;

namespace Compartilhei.UnitTests.Photos;

public class PhotoUploadValidatorTests
{
    [Theory]
    [InlineData("foto.jpg", "image/jpeg")]
    [InlineData("foto.jpeg", "image/jpeg")]
    [InlineData("foto.png", "image/png")]
    [InlineData("foto.webp", "image/webp")]
    public void Should_Accept_Valid_Image(
        string fileName,
        string contentType)
    {
        // Arrange
        var validator = new PhotoUploadValidator();

        // Act
        var act = () => validator.Validate(
            fileName,
            5 * 1024 * 1024,
            contentType);

        // Assert
        act();
    }

    [Fact]
    public void Should_Reject_File_Larger_Than_20MB()
    {
        // Arrange
        var validator = new PhotoUploadValidator();

        // Act
        var act = () => validator.Validate(
            "foto.jpg",
            20 * 1024 * 1024 + 1,
            "image/jpeg");

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Theory]
    [InlineData("foto.gif", "image/gif")]
    [InlineData("foto.bmp", "image/bmp")]
    [InlineData("foto.txt", "text/plain")]
    public void Should_Reject_Unsupported_File(
        string fileName,
        string contentType)
    {
        // Arrange
        var validator = new PhotoUploadValidator();

        // Act
        var act = () => validator.Validate(
            fileName,
            1024,
            contentType);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Reject_Empty_FileName()
    {
        var validator = new PhotoUploadValidator();

        var act = () => validator.Validate(
            string.Empty,
            1024,
            "image/jpeg");

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Reject_Empty_ContentType()
    {
        var validator = new PhotoUploadValidator();

        var act = () => validator.Validate(
            "foto.jpg",
            1024,
            string.Empty);

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Should_Reject_Zero_FileSize()
    {
        var validator = new PhotoUploadValidator();

        var act = () => validator.Validate(
            "foto.jpg",
            0,
            "image/jpeg");

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}