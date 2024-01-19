using System;
using System.Text;

namespace FluentChange.Extensions.System.Helper
{
    public static class StringBuilderExtensions
    {
        public static void AppendLineIndented(this StringBuilder builder, int indentation, string value)
        {
            builder.AppendLine(Indentation(indentation) + value);
        }

        public static string Indentation(int indentationLevel)
        {
            switch (indentationLevel)
            {
                case 0: return "";
                case 1: return "    ";
                case 2: return "        ";
                case 3: return "            ";
                case 4: return "                ";
                case 5: return "                    ";
                case 6: return "                        ";
                case 7: return "                            ";
            }
            throw new NotImplementedException();
        }
    }
}
