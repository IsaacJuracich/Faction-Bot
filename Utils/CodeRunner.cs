using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using MinecraftClient;
using System;
using System.Reflection;

namespace orbitFactionBot.Utils {
    class CodeRunner {
        public static async void Execute(string code) {
            try {
                Script script = CSharpScript.Create(code, ScriptOptions.Default.WithReferences(Assembly.GetExecutingAssembly()).WithImports("orbitFactionBot", "System.Threading.Tasks", "System"));
                var result = await script.RunAsync();
            }
            catch (CompilationErrorException e) {
                ConsoleIO.WriteLine(string.Join(Environment.NewLine, e.StackTrace));
            }
        }
    }
}
    