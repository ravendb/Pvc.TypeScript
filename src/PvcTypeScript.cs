using PvcCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcPlugins
{
    public class PvcTypeScript : PvcPlugin
    {
        private readonly string compilerOptions;

        public PvcTypeScript(string compilerOptions)
        {
            this.compilerOptions = compilerOptions;
        }

        public override string[] SupportedTags
        {
            get
            {
                return new[] { ".ts" };
            }
        }

        public override IEnumerable<PvcStream> Execute(IEnumerable<PvcStream> inputStreams)
        {
            var tsFiles = inputStreams.Select(i => i.StreamName).ToList();
            var tsFilesString = string.Join(" ", tsFiles.Select(f => "\"" + f + "\""));

            // Should we embed the compiler instead?
            var compilerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft SDKs\TypeScript\1.0\tsc.exe");

            var p = Path.GetTempFileName();
            File.WriteAllText(p, string.Format("{0} {1}", this.compilerOptions, tsFilesString));

            var result = PvcUtil.StreamProcessExecution(compilerPath, Environment.CurrentDirectory, "@"+ p);

            File.Delete(p);

            string tsOutputLine;
            var outStreamReader = new StreamReader(result.Item1);
            while ((tsOutputLine = outStreamReader.ReadLine()) != null)
            {
                Console.WriteLine(tsOutputLine);
            }

            var errorStreamReader = new StreamReader(result.Item2);
            while ((tsOutputLine = errorStreamReader.ReadLine()) != null)
            {
                Console.WriteLine(tsOutputLine);
            }

            // TODO: Implement advanced compiler options that affect JS output.
            //
            // In the compiler options flag, if the user passes in either of the following options:
            //      --out (combine JS output into single file option)
            //      - "redirect JS output to directory" option
            //      - "generate declaration files" option
            //      - "generate source maps" options
            // 
            // Then we'll need to pick up the corresponding JS and .js.map files.
            return tsFiles
                .Select(f => f.Replace(".ts", ".js"))
                .Where(f => File.Exists(f))
                .Select(f => PvcUtil.PathToStream(f));
        }
    }
}
