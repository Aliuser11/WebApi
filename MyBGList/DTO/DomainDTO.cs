using System.ComponentModel.DataAnnotations;

namespace MyBGList.DTO
{
    public class DomainDTO : IValidatableObject
    {
        [Required]

        public int Id { get; set; }

        //Add a built-in validator to the Name property
        [RegularExpression("^[A-Za-z]+$",
            ErrorMessage = "Value must contain only letters (no spaces, digits, or other chars)")]
        /*considered valid only if it's not null, not empty, and containing only uppercase and lowercase letters - without digits, spaces, or any other character.*/
        public string? Name { get; set; } 

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
             /*6.3.3 IValidatableObject: consider the model valid only if the Id value is equal to 3 or if the Name value is equal to "Wargames".*/
            //If the model is invalid, the validator should emit the following error message: => "Id and/or Name values must match an allowed Domain."

            return (Id != 3 && Name != "Wargames")
            ? new[] { new ValidationResult(
                "Id and/or Name values must match an allowed Domain.") }
            : new ValidationResult[0];
        }

    }
}

