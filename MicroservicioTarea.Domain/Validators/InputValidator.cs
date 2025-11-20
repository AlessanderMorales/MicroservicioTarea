using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MicroservicioTarea.Domain.Validators
{
    public static class InputValidator
    {
        private static readonly string[] SqlInjectionPatterns = new[]
        {
            @"(\bOR\b|\bAND\b).*=.*",
            @"';|--;|\/\*|\*\/",
            @"\bEXEC\b|\bEXECUTE\b",
            @"\bDROP\b|\bDELETE\b|\bUPDATE\b|\bINSERT\b",
            @"\bSELECT\b.*\bFROM\b",
            @"\bUNION\b.*\bSELECT\b",
            @"xp_cmdshell",
            @"\bSCRIPT\b.*>",
            @"<\s*script",
            @"javascript:",
            @"onerror\s*=",
            @"onload\s*="
        };

        public static string SanitizeString(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.Trim();
            input = Regex.Replace(input, @"\s+", " ");
            return input;
        }

        public static bool ContainsSqlInjection(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.ToUpper();

            foreach (var pattern in SqlInjectionPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }

        public static string ValidateAndSanitize(string? input, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = SanitizeString(input);

            if (ContainsSqlInjection(input))
                throw new ArgumentException($"El campo '{fieldName}' contiene caracteres o patrones no permitidos.");

            return input;
        }

        public static string SanitizeText(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = SanitizeString(input);
            input = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"javascript:", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"on\w+\s*=", "", RegexOptions.IgnoreCase);

            if (ContainsSqlInjection(input))
                throw new ArgumentException("El texto contiene patrones no permitidos.");

            return input;
        }

        public static DateTime ValidateDate(DateTime date, bool canBePast = false, bool canBeFuture = true)
        {
            var today = DateTime.Now.Date;
            var maxFutureDate = today.AddYears(10);

            if (!canBePast && date.Date < today)
                throw new ArgumentException("La fecha no puede ser anterior a hoy.");

            if (!canBeFuture && date.Date > today)
                throw new ArgumentException("La fecha no puede ser futura.");

            if (date > maxFutureDate)
                throw new ArgumentException("La fecha está demasiado lejos en el futuro (máximo 10 años).");

            if (date.Year < 1900)
                throw new ArgumentException("La fecha no es válida.");

            return date;
        }

        public static string ValidateStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("El estado no puede estar vacío.");

            status = status.Trim();
            var validStatuses = new[] { "SinIniciar", "EnProgreso", "Completada" };

            if (!validStatuses.Contains(status))
                throw new ArgumentException($"El estado '{status}' no es válido. Estados permitidos: {string.Join(", ", validStatuses)}");

            return status;
        }

        public static string ValidatePriority(string? priority)
        {
            if (string.IsNullOrWhiteSpace(priority))
                throw new ArgumentException("La prioridad no puede estar vacía.");

            priority = priority.Trim();
            var validPriorities = new[] { "Baja", "Media", "Alta" };

            if (!validPriorities.Contains(priority))
                throw new ArgumentException($"La prioridad '{priority}' no es válida. Prioridades permitidas: {string.Join(", ", validPriorities)}");

            return priority;
        }
    }
}
