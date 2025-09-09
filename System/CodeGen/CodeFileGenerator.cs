using FluentChange.Extensions.System.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace FluentChange.Extensions.System.CodeGen
{

    public class CodeGenerator
    {
        private List<CodeResultGenerator> files = new List<CodeResultGenerator>();
        public CodeFileGenerator CreateFile(string path, string filename)
        {
            var fileGen = new CodeFileGenerator();
            fileGen.FilePath = path;
            fileGen.FileName = filename;

            files.Add(fileGen);
            return fileGen;
        }

        public CodeCompiledLibGenerator CreateCompiledLib(string path, string filename)
        {
            var fileGen = new CodeCompiledLibGenerator();
            fileGen.FilePath = path;
            fileGen.FileName = filename;

            files.Add(fileGen);
            return fileGen;
        }

        public void Generate()
        {
            foreach (var file in files)
            {
                file.Generate();
            }
        }
    }

    public abstract class CodeResultGenerator
    {
        internal List<String> Usings = new List<String>();
        internal String FilePath;
        internal String FileName;

        protected List<NamespaceGenerator> namespaces = new List<NamespaceGenerator>();

        public CodeResultGenerator AddUsing(string reference)
        {
            Usings.Add(reference);
            return this;
        }

        public NamespaceGenerator CreateNamespace(string name)
        {
            var ns = new NamespaceGenerator();
            ns.Name = name;

            namespaces.Add(ns);
            return ns;
        }

        internal string GenerateCode()
        {

            var builder = new StringBuilder();
            foreach (var usingRef in Usings)
            {
                builder.AppendLine("using " + usingRef + ";");
            }
            builder.AppendLine("");

            foreach (var ns in namespaces)
            {
                ns.Generate(builder, 0);
            }

            return builder.ToString();
        }

        internal abstract void Generate();
    }

    public class CodeCompiledLibGenerator : CodeResultGenerator
    {
        internal List<String> AssemblyLocations = new List<String>();

        public MemoryStream StreamSource;
        public MemoryStream StreamLib;
        public MemoryStream StreamPdb;
        public bool Success = false;
        public CodeCompiledLibGenerator AddAssemblyByType(Type type)
        {
            AddAssembly(type.Assembly);

            return this;
        }

        public CodeCompiledLibGenerator AddAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                var location = assembly.Location;
                if (!AssemblyLocations.Contains(location))
                {
                    AssemblyLocations.Add(location);


                    var referencedAssemblies = assembly.GetReferencedAssemblies();
                    foreach (var refAssembly in referencedAssemblies)
                    {
                        var subAssembly = GetAssemblyByName(refAssembly.Name);
                        AddAssembly(subAssembly);
                    }
                }
            }

            return this;
        }

        Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                   SingleOrDefault(assembly => assembly.GetName().Name == name);
        }

        internal override void Generate()
        {
            var source = GenerateCode();
            StreamSource = new MemoryStream();

            var writer = new StreamWriter(StreamSource);
            writer.Write(source);
            writer.Flush();
            StreamSource.Position = 0;

            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            //references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            foreach (var assemblyLocation in AssemblyLocations.Distinct())
            {
                references.Add(MetadataReference.CreateFromFile(assemblyLocation));
            }


            CSharpCompilation compilation = CSharpCompilation.Create(
              "MyLib",
              new[] { syntaxTree },
              references,
              new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            StreamLib = new MemoryStream();
            StreamPdb = new MemoryStream();

            var emitResult = compilation.Emit(StreamLib, StreamPdb);
            if (!emitResult.Success)
            {
                var xy = emitResult.Diagnostics.FirstOrDefault();
                if (xy != null)
                {
                    Console.WriteLine(xy.GetMessage());
                    var tempSourceFolder = @"C:\Code\2025\IfficienT.Platform.Core\Logic\Modules\CalcSpace\Calculation\TempSource\";
                    File.WriteAllText(tempSourceFolder + "output.cs", source);
                }
            }
            else
            {
                Success = true;
            }

        }

        public void SaveToFiles(string folder)
        {
            var path = folder;

            var assemblyFile = Path.Combine(path, FileName + ".dll");
            var sourceFile = Path.Combine(path, FileName + ".cs");
            var pdbFile = Path.Combine(path, FileName + ".pdb");

            File.WriteAllBytes(sourceFile, StreamSource.ToArray());
            File.WriteAllBytes(assemblyFile, StreamLib.ToArray());
            File.WriteAllBytes(pdbFile, StreamPdb.ToArray());
            Console.WriteLine("Source File written");
        }
    }



    public class CodeFileGenerator : CodeResultGenerator
    {

        internal override void Generate()
        {
            var code = GenerateCode();

            if (!FileName.EndsWith(".cs")) FileName += ".cs";
            var fullPath = Path.Combine(FilePath, FileName);

            File.WriteAllText(fullPath, code);
        }
    }



    public class NamespaceGenerator
    {
        private List<ClassGenerator> classes = new List<ClassGenerator>();
        internal string Name { get; set; }

        public ClassGenerator CreateClass(string name, string baseclass = null)
        {
            var @class = new ClassGenerator();
            @class.Name = name;
            @class.BaseClass = baseclass;
            classes.Add(@class);
            return @class;
        }

        public ClassGenerator CreateClass(string name, Action<ClassGenerator> classAction, string baseclass = null)
        {
            var @class = new ClassGenerator();
            @class.Name = name;
            @class.BaseClass = baseclass;
            classes.Add(@class);
            classAction.Invoke(@class);

            return @class;
        }

        internal void Generate(StringBuilder builder, int indentation)
        {

            builder.AppendLine("namespace " + Name);
            builder.AppendLine("{");
            builder.AppendLine();

            foreach (var @class in classes)
            {
                @class.Generate(builder, indentation + 1);
                builder.AppendLine();
            }

            builder.AppendLine("}");
        }
    }


    public abstract class AbstractClassGenerator
    {
        protected List<ClassPropertyGenerator> properties = new List<ClassPropertyGenerator>();
        protected List<ClassMethodGenerator> methods = new List<ClassMethodGenerator>();

        public ClassPropertyGenerator CreatePropertyLambda(string type, string name, bool isPrivate, string lambda)
        {
            var prop = new ClassPropertyGenerator();
            prop.Name = name;
            prop.TypeName = type;
            prop.IsPrivate = isPrivate;
            prop.Lambda = lambda;
            properties.Add(prop);
            return prop;
        }

        public ClassPropertyGenerator CreateStaticField(string type, string name, bool isPrivate, string init)
        {
            var prop = new ClassPropertyGenerator();
            prop.Name = name;
            prop.TypeName = type;
            prop.IsPrivate = isPrivate;
            prop.IsStatic = true;
            prop.IsProperty = false;
            prop.DefaultInit = init;
            properties.Add(prop);
            return prop;
        }
        public ClassPropertyGenerator CreateProperty(string type, string name, bool isPrivate, string? defaultInit)
        {
            var prop = new ClassPropertyGenerator();
            prop.Name = name;
            prop.TypeName = type;
            prop.IsPrivate = isPrivate;
            prop.DefaultInit = defaultInit;
            properties.Add(prop);
            return prop;
        }
        public ClassMethodGenerator CreateMethod(string returnType, string name, Dictionary<string, Type> parameters,
            bool isPrivate, string region, Action<StringBuilder, int> codeAction = null, bool isOverride = false)

        {
            var method = new ClassMethodGenerator();
            method.Name = name;
            method.ReturnType = returnType;
            method.Region = region;
            method.IsPrivate = isPrivate;
            method.Parameters = parameters;
            method.CodeAction = codeAction;
            method.IsOverride = isOverride;
            methods.Add(method);
            return method;
        }

        protected void GenerateInternal(StringBuilder builder, int indentation)
        {
            foreach (var prop in properties)
            {
                prop.Generate(builder, indentation);
            }

            var groupedMethods = methods.GroupBy(m => m.Region);
            foreach (var regionGroup in groupedMethods)
            {
                var region = regionGroup.Key;

                var regionGen = new ClassRegionGenerator();
                regionGen.Name = region;
                regionGen.CodeAction = (b, i) =>
                {
                    foreach (var method in regionGroup)
                    {
                        method.Generate(b, i);
                    }
                };

                regionGen.Generate(builder, indentation);
            }
        }
    }

    public class ClassGenerator : AbstractClassGenerator
    {
        internal string Name { get; set; }
        internal string BaseClass { get; set; }


        private List<ClassRegionGenerator> regions = new List<ClassRegionGenerator>();


        public ClassRegionGenerator CreateRegion(string name)
        {
            var region = new ClassRegionGenerator();
            region.Name = name;
            regions.Add(region);
            return region;
        }
        public ClassRegionGenerator CreateRegion(string name, Action<StringBuilder, int> codeAction)
        {
            var region = new ClassRegionGenerator();
            region.Name = name;
            region.CodeAction = codeAction;
            regions.Add(region);
            return region;
        }

        public ClassRegionGenerator CreateRegion(string name, Action<ClassRegionGenerator> regionAction)
        {
            var region = new ClassRegionGenerator();
            region.Name = name;
            regions.Add(region);
            regionAction.Invoke(region);
            return region;
        }

        internal void Generate(StringBuilder builder, int indentation)
        {
            if (String.IsNullOrEmpty(BaseClass))
            {
                builder.AppendLineIndented(indentation, "public class " + Name);
            }
            else
            {
                builder.AppendLineIndented(indentation, "public class " + Name + " : " + BaseClass);
            }

            builder.AppendLineIndented(indentation, "{");
            foreach (var region in regions)
            {
                region.Generate(builder, indentation + 1);
            }
            base.GenerateInternal(builder, indentation + 1);
            builder.AppendLineIndented(indentation, "}");
        }


    }

    public class ClassRegionGenerator : AbstractClassGenerator
    {
        internal string Name;
        internal Action<StringBuilder, int> CodeAction;
        internal void Generate(StringBuilder builder, int indentation)
        {
            if (!String.IsNullOrEmpty(Name))
            {
                builder.AppendLineIndented(indentation, "");
                builder.AppendLineIndented(indentation, "#region " + Name);
                builder.AppendLineIndented(indentation, "");
            }
            if (CodeAction != null) CodeAction.Invoke(builder, indentation);
            base.GenerateInternal(builder, indentation);
            if (!String.IsNullOrEmpty(Name))
            {
                builder.AppendLineIndented(indentation, "");
                builder.AppendLineIndented(indentation, "#endregion");
            }
        }
    }


    public class ClassPropertyGenerator
    {
        internal string Name { get; set; }
        internal string TypeName { get; set; }
        internal bool IsPrivate { get; set; }
        internal bool IsStatic { get; set; } = false;
        internal bool IsProperty { get; set; } = true;
        internal string? Lambda { get; set; }
        internal string DefaultInit;

        internal void Generate(StringBuilder builder, int indentation)
        {
            var propCode = "";

            if (IsPrivate) propCode = "private ";
            else propCode = "public ";

            if (IsStatic) propCode += "static ";

            propCode += TypeName + " ";
            propCode += Name;

            if (String.IsNullOrEmpty(Lambda))
            {
                if (IsProperty) propCode += " { get; set; }";
                if (!String.IsNullOrEmpty(DefaultInit)) propCode += " = " + DefaultInit + ";";
            }
            else
            {
                propCode += " => " + Lambda + ";";
            }

            builder.AppendLineIndented(indentation, propCode);
        }
    }
    public class ClassMethodGenerator
    {
        internal string Region;
        internal string Name;
        internal string ReturnType;
        internal Action<StringBuilder, int> CodeAction;
        internal Dictionary<string, Type> Parameters;
        internal bool IsPrivate;
        internal bool IsOverride = false;

        public CodeLinesGenerator Code = new CodeLinesGenerator();

        internal void Generate(StringBuilder builder, int indentation)
        {
            var returnTypeName = ReturnType;

            if (returnTypeName == "Void") returnTypeName = returnTypeName.ToLower();

            var parameterString = "";

            if (Parameters != null)
            {
                var paramsList = Parameters.Select(p => p.Value.Name + " " + p.Key);
                parameterString = String.Join(", ", paramsList);
            }

            var modifiers = "public";
            if (IsPrivate) modifiers = "private";

            if (IsOverride) modifiers += " override";

            builder.AppendLineIndented(indentation, modifiers + " " + returnTypeName + " " + Name + "(" + parameterString + ")");
            builder.AppendLineIndented(indentation, "{");
            Code.Generate(builder, indentation + 1);
            if (CodeAction != null) CodeAction.Invoke(builder, indentation + 1);
            builder.AppendLineIndented(indentation, "}");

        }

    }


    public interface ICodeLine
    {
        public void Generate(StringBuilder builder, int indentation);
    }

    public class CodeIfGenerator : ICodeLine
    {
        internal string Check;
        internal CodeLinesGenerator trueCase;
        internal CodeLinesGenerator elseCase;

        public void Generate(StringBuilder builder, int indentation)
        {
            builder.AppendLineIndented(indentation, "if (" + Check + ")");
            builder.AppendLineIndented(indentation, "{");
            trueCase.Generate(builder, indentation + 1);
            builder.AppendLineIndented(indentation, "}");
            if (elseCase != null)
            {
                builder.AppendLineIndented(indentation, "else");
                builder.AppendLineIndented(indentation, "{");
                elseCase.Generate(builder, indentation + 1);
                builder.AppendLineIndented(indentation, "}");
            }
        }
    }

    public class CodeLinesRawGenerator : ICodeLine
    {
        internal List<string> Lines = new List<string>();

        public CodeLinesRawGenerator Add(string line)
        {
            Lines.Add(line);
            return this;
        }

        public void Generate(StringBuilder builder, int indentation)
        {
            foreach (var line in Lines)
            {
                var lineX = line;
                if (!lineX.EndsWith(";")) lineX += ";";
                builder.AppendLineIndented(indentation, lineX);
            }

        }

    }



    public class CodeLinesGenerator
    {
        protected List<ICodeLine> lines = new List<ICodeLine>();


        public CodeIfGenerator CreateIf(string check, Action<CodeLinesGenerator> trueCase, Action<CodeLinesGenerator> elseCase = null)
        {
            var ifGen = new CodeIfGenerator();
            ifGen.Check = check;
            ifGen.trueCase = new CodeLinesGenerator();
            trueCase.Invoke(ifGen.trueCase);
            if (elseCase != null)
            {
                ifGen.elseCase = new CodeLinesGenerator();
                elseCase.Invoke(ifGen.elseCase);
            }
            lines.Add(ifGen);
            return ifGen;
        }

        public CodeLinesRawGenerator Raw()
        {
            var raw = new CodeLinesRawGenerator();
            lines.Add(raw);
            return raw;
        }

        public CodeLinesRawGenerator Raw(Action<CodeLinesRawGenerator> codeAction)
        {
            var raw = new CodeLinesRawGenerator();
            lines.Add(raw);
            codeAction.Invoke(raw);
            return raw;
        }



        internal void Generate(StringBuilder builder, int indentation)
        {
            foreach (var line in lines)
            {
                line.Generate(builder, indentation);
            }
        }

    }




    public class ClassGeneratorHelpers
    {
        public static string GenerateClassField(string modelName, string fieldName, bool isPrivate, bool initNew, bool isStatic = false, string initParam = "")
        {
            var builder = new StringBuilder();
            if (isPrivate) builder.Append("private ");
            else builder.Append("public ");

            if (isStatic) builder.Append("static ");

            builder.Append(modelName + " ");
            builder.Append(fieldName);

            if (initNew)
            {
                builder.Append(" = new " + modelName + "()");
            }
            if (!String.IsNullOrEmpty(initParam))
            {
                builder.Append(" = " + initParam);
            }
            builder.Append(";");

            return builder.ToString();
        }
        public static string GenerateClassFieldLamda(string modelName, string fieldName, bool isPrivate, string lambda)
        {
            var builder = new StringBuilder();
            if (isPrivate) builder.Append("private ");
            else builder.Append("public ");

            builder.Append(modelName + " ");
            builder.Append(fieldName);
            builder.Append(" => " + lambda);

            builder.Append(";");

            return builder.ToString();
        }

        public static string GenerateMethod(int indentation, string methodName, string returntype, Dictionary<string, Type> parameters, Action<StringBuilder, int> builderAction, bool isOverride = false)
        {
            var returnTypeName = returntype;

            if (returnTypeName == "Void") returnTypeName = returnTypeName.ToLower();

            var parameterString = "";

            if (parameters != null)
            {
                var paramsList = parameters.Select(p => p.Value.Name + " " + p.Key);
                parameterString = String.Join(", ", paramsList);
            }

            var modifiers = "public";

            if (isOverride) modifiers += " override";


            var builder = new StringBuilder();
            builder.AppendLineIndented(indentation, modifiers + " " + returnTypeName + " " + methodName + "(" + parameterString + ")");
            builder.AppendLineIndented(indentation, "{");
            builderAction.Invoke(builder, indentation + 1);
            builder.AppendLineIndented(indentation, "}");
            return builder.ToString();
        }
        public static string GenerateSwitchCase(int indentation, string variableName, Dictionary<string, string> cases)
        {
            var builder = new StringBuilder();
            builder.AppendLineIndented(indentation, "switch (" + variableName + ")");
            builder.AppendLineIndented(indentation, "{");
            foreach (var def in cases)
            {
                var caseCode = def.Value;
                //if (!caseCode.EndsWith(";")) caseCode = caseCode + ";";

                if (def.Key != "default")
                {
                    if (!caseCode.StartsWith("return")) caseCode = caseCode + " break;";
                    builder.AppendLineIndented(indentation + 1, "case \"" + def.Key + "\": " + caseCode);
                }
                else
                {
                    builder.AppendLineIndented(indentation + 1, def.Key + ": " + caseCode);
                }

            }
            builder.AppendLineIndented(indentation, "}");
            return builder.ToString();

        }
        public static string GenerateIf(int indentation, string check, Action<StringBuilder, int> trueCaseAction, Action<StringBuilder, int> elseCaseAction = null)
        {
            var builder = new StringBuilder();
            builder.AppendLineIndented(indentation, "if (" + check + ")");
            builder.AppendLineIndented(indentation, "{");
            trueCaseAction.Invoke(builder, indentation + 1);
            builder.AppendLineIndented(indentation, "}");
            if (elseCaseAction != null)
            {
                builder.AppendLineIndented(indentation, "else");
                builder.AppendLineIndented(indentation, "{");
                elseCaseAction.Invoke(builder, indentation + 1);
                builder.AppendLineIndented(indentation, "}");
            }
            return builder.ToString();

        }
        public static string GenerateIfSingleLine(int indentation, string check, Action<StringBuilder> trueCaseAction)
        {
            var builder = new StringBuilder();
            var ifCode = new StringBuilder();
            trueCaseAction.Invoke(ifCode);
            builder.AppendLineIndented(indentation, "if (" + check + ") { " + ifCode.ToString() + " }");

            return builder.ToString();

        }
        public static string GenerateTryCatch(int indentation, Action<StringBuilder, int> tryAction, Action<StringBuilder, int> catchAction = null)
        {
            var builder = new StringBuilder();
            builder.AppendLineIndented(indentation, "try");
            builder.AppendLineIndented(indentation, "{");
            tryAction.Invoke(builder, indentation + 1);
            builder.AppendLineIndented(indentation, "}");
            if (catchAction != null)
            {
                builder.AppendLineIndented(indentation, "catch (Exception ex)");
                builder.AppendLineIndented(indentation, "{");
                catchAction.Invoke(builder, indentation + 1);
                builder.AppendLineIndented(indentation, "}");
            }
            return builder.ToString();

        }
    }
}