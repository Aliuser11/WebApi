﻿using CsvHelper.Configuration.Attributes;
using MyBGList.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyBGList.DTO
{
    /* the class that will contain all the client-defined input parameters that we are currently receiving in the BoardGamesController's Get action method
     later on added : Implementing the IValidatableObject
     */
    //public class RequestDTO
    //{
    //    [DefaultValue(0)]
    //    public int PageIndex { get; set; }

    //    [DefaultValue(10)]
    //    [Range(1, 100)]
    //    public int PageSize { get; set; }


    //    [DefaultValue("Name")]
    //    [SortColumnValidator(typeof(BoardGameDTO))]
    //    public string? SortColumn { get; set; } = "Name";

    //    [DefaultValue("ASC")]
    //    [SortOrderValidator]
    //    public string? SortOrder { get; set; } = "ASC";

    //    [DefaultValue(null)]
    //    public string? FilterQuery { get; set; } = null;
    //}

    /* changed with Implementing the IValidatableObject */
    public class RequestDTO<T> : IValidatableObject
    {
        [DefaultValue(0)]
        public int PageIndex { get; set; } = 0;

        [DefaultValue(10)]
        [Range(1, 100)]
        public int PageSize { get; set; } = 10;


        [DefaultValue("Name")]
        public string? SortColumn { get; set; } = "Name";

        [SortOrderValidator]
        [DefaultValue("ASC")]
        public string? SortOrder { get; set; } = "ASC";

        [DefaultValue(null)]
        public string? FilterQuery { get; set; } = null;

        public IEnumerable<ValidationResult>Validate(
            ValidationContext validationContext)
        {
            var validator = new SortColumnValidatorAttribute(typeof(BoardGameDTO));
            var result = validator
                .GetValidationResult(SortColumn, validationContext);
                return (result != null)
                ? new [] { result }
                : new ValidationResult[0];
        }
    }
}
