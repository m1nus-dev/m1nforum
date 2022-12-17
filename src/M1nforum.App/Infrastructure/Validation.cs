using System;
using System.Collections.Generic;

// ideas and code taken from:
// https://github.com/runeapetersen/ArgumentValidator
// https://github.com/FluentValidation/FluentValidation

namespace M1nforum.Web.Infrastructure.Validation
{
	public class Validation
	{
		public List<ValidationException> ValidationExceptions { get; protected set; } = new List<ValidationException>();

		public Validation() { }

		internal Validation AddValidationException(string parameterName, string message)
		{
			lock (ValidationExceptions)
			{
				ValidationExceptions.Add(new ValidationException(parameterName, message));
			}

			return this;
		}
	}

	[Serializable]
	public record ValidationException(string parameterName, string message) { }

	public static class ValidationExtensions
	{
		public static Validation IsNotNull<T>(this Validation validation, T value, string parameterName) where T : class
		{
			if (validation == null) throw new ArgumentNullException(nameof(validation));
			if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentNullException(nameof(parameterName), "The parameter name must be specified.");

			if (value == null) return AddArgumentNullException(validation, parameterName, "The argument cannot be null.");

			return validation;
		}

		public static Validation IsNotNullOrEmpty(this Validation validation, string value, string parameterName)
		{
			if (validation == null) throw new ArgumentNullException(nameof(validation));
			if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentNullException(nameof(parameterName), "The parameter name must be specified.");

			return string.IsNullOrEmpty(value) ?
				AddArgumentOutOfRangeException(validation, parameterName, "Empty string or null reference is not accepted as an argument of type System.string") :
				validation;
		}

		public static Validation IsLengthInRange(this Validation validation, string value, string parameterName, int minLength, int maxLength)
		{
			if (validation == null) throw new ArgumentNullException(nameof(validation));
			if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentNullException(nameof(parameterName), "The parameter name must be specified.");
			if (minLength < 0) throw new ArgumentNullException(nameof(minLength));
			if (maxLength <= 0) throw new ArgumentNullException(nameof(maxLength));

			return (string.IsNullOrWhiteSpace(value) || value.Length < minLength || value.Length > maxLength) ?
				AddArgumentOutOfRangeException(validation, parameterName, "This field must be longer than " + minLength.ToString() + " and shorter than " + maxLength.ToString() + " characters") :
				validation;
		}

		public static Validation DateTimeIsInitialized(this Validation validation, DateTime value, string parameterName)
		{
			if (validation == null) throw new ArgumentNullException(nameof(validation));
			if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentNullException(nameof(parameterName));

			return value == DateTime.MinValue ? 
				AddArgumentOutOfRangeException(validation, parameterName, "must be initialized to a date other than DateTime.Min") :
				validation;
		}

		public static Validation NumericValueIsWithinRange(this Validation validation, int value, string parameterName, int lowerBoundInclusive, int upperBoundInclusive)
		{
			if (validation == null) throw new ArgumentNullException(nameof(validation));
			if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentNullException(nameof(parameterName));

			return !(lowerBoundInclusive <= value && value <= upperBoundInclusive) ? 
				AddArgumentOutOfRangeException(validation, parameterName, "must be within the range of " + lowerBoundInclusive + " - " + upperBoundInclusive) :
				validation;
		}

		public static List<ValidationException> GetValidationExceptions(this Validation validation)
		{
			if (validation == null) throw new ArgumentNullException(nameof(validation));

			return validation.ValidationExceptions;
		}

		private static Validation AddArgumentOutOfRangeException(Validation validation, string parameterName, string message)
		{
			return validation.AddValidationException(parameterName, message);
		}

		private static Validation AddArgumentNullException(Validation validation, string parameterName, string message)
		{
			return validation.AddValidationException(parameterName, message);
		}
	}
}
