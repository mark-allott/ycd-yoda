using SimpleInstructionMachine.Abstractions;
using SimpleInstructionMachine.Interfaces;
using SimpleInstructionMachine.Logging;
using SimpleInstructionMachine.Strategies;
using SimpleInstructionMachine.VirtualProcessors;

namespace SimpleInstructionMachine;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var folder = ".";
        if (args.Length > 0)
            folder = args[0];

        var debug = (args.Length > 1 && args[1] == "--debug");

        //  This would be better served as part of a DI implementation
        //  Original solution did not have DI support, so continuing that scheme here (for now)
        
        //  Logging is to the Console
        var logger = new ConsoleLogger(debug);
        //  Virtual display uses the console logger
        var virtualDisplay = new ByteVirtualDisplay(logger, KnownMemory.ControlFlags);
        //  memory system uses the configured virtual display if updates are required
        var memorySystem = new ByteMemoryAccess(byte.MaxValue + 1, virtualDisplay);
        //  Filename strategy is the default for the exercises (0..7 as-is, 8..15 adds .txt extension)
        var filenameStrategy = new DefaultFileNameStrategy();
        //  Default filesystem uses the specified folder and naming strategy
        var fileSystem = new DefaultFileSystemStrategy(folder, filenameStrategy);
        //  Define the CPU to be used for the VM
        var cpu = new YodaProcessor(debug, logger, fileSystem, memorySystem);
        
        //  Define the VM with the specified components  
        var vm = new ByteVirtualMachine(debug, memorySystem, virtualDisplay, fileSystem,
            cpu as IVirtualProcessorStrategyAsync, logger);
        //  Bootstrap the machine - i.e. load the "program" into memory from the file named "boot" in the filesystem
        vm.Boot();
        
        //  Create a token and run the program that was bootstrapped in an async manner
        var tokenSource = new CancellationTokenSource();
        await vm.RunAsync(tokenSource.Token);    
    }
}