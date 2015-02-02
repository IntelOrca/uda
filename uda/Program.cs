using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using uda.Architecture;
using uda.Intermediate;
using uda.Language;
using uda.Strategy;

namespace uda
{
	internal class Program
	{
		private static string _inputFile;
		private static long _address;
		private static string _architecture = "x86";

		private static void Main(string[] args)
		{
			if (!ParseCommandLineArguments(args))
				return;

			// Read binary and create basic instructions
			AddressInstructionPair[] iis;
			if (_architecture.ToLower() == "x86") {
				using (var machineCodeReader = new AMD64MachineCodeReader(_inputFile))
					iis = machineCodeReader.Read(_address).ToArray();
			} else if (_architecture.ToLower() == "arm") {
				using (var machineCodeReader = new ARMMachineCodeReader(_inputFile))
					iis = machineCodeReader.Read(_address).ToArray();
			} else {
				Console.WriteLine("Unknown or unsupported architecture.");
				return;
			}

			// Create function and tree table
			Function function = new Function();
			function.Name = String.Format("sub_{0:X6}", _address);
			function.InstructionTreeTable = InstructionTreeTable.CreateFromInstructions(iis);

			// Run decompile strategies
			new LocalRenumberStrategy().Process(function);
			new LoopFinderStrategy().Process(function);
			new TreeInlinerStrategy().Process(function);

			// Write out generated source code
			CLanguageWriter langWriter = new CLanguageWriter();
			Console.WriteLine(langWriter.Write(function));
		}

		private static bool ParseCommandLineArguments(string[] args)
		{
			try {
				Queue<string> argQueue = new Queue<string>(args);
				while (argQueue.Count > 0) {
					string arg = argQueue.Dequeue();

					if (arg.StartsWith("--")) {
						string option = arg.Substring(2);
						arg = argQueue.Dequeue();

						switch (option) {
						case "address":
							_address = Convert.ToInt64(arg, 16);
							break;
						case "arch":
						case "architecture":
							_architecture = arg;
							break;
						}
					} else if (arg.StartsWith("-")) {
						char[] singleOptions = arg.Substring(1).ToCharArray();
						foreach (char c in singleOptions) {
							switch (c) {
							case 'v':
								PrintVersionInformation();
								return false;
							}
						}
					} else {
						if (!String.IsNullOrEmpty(_inputFile))
							throw new Exception();

						_inputFile = arg;
						continue;
					}
				}

				if (String.IsNullOrEmpty(_inputFile))
					throw new Exception();

				return true;
			} catch {
				PrintHelpInformation();
				return false;
			}
		}

		private static void PrintAboutInformation()
		{
			Console.WriteLine("Universal Decompiler and Analyser");
			Console.WriteLine("Copyright (C) Ted John 2015");
		}

		private static void PrintHelpInformation()
		{
			PrintAboutInformation();
			Console.WriteLine("Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString(2));
			Console.WriteLine("Usage: uda [flags] [options] <input path>");
		}

		private static void PrintVersionInformation()
		{
			PrintAboutInformation();
			Console.WriteLine("Version " + Assembly.GetExecutingAssembly().GetName().Version);
			Console.WriteLine("Build time: " + RetrieveLinkerTimestamp());
		}

		/// <remarks>http://stackoverflow.com/questions/1600962/displaying-the-build-date</remarks>
		private static DateTime RetrieveLinkerTimestamp()
		{
			string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;
			byte[] b = new byte[2048];
			System.IO.Stream s = null;

			try {
				s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				s.Read(b, 0, 2048);
			} finally {
				if (s != null) {
					s.Close();
				}
			}

			int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
			int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
			DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			dt = dt.AddSeconds(secondsSince1970);
			dt = dt.ToLocalTime();
			return dt;
		}
	}
}
