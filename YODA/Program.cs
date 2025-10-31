namespace SimpleInstructionMachine;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var folder = ".";
        if (args.Length > 0)
            folder = args[0];

        var debug = args.Any(s => s.Equals("--debug", StringComparison.InvariantCultureIgnoreCase));
        
        Console.WriteLine("Starting landing computer running York's Obscenely Dumb Architecture (YODA) - Release Build 12x.11g-34 + Anti-gravity module");
        Console.WriteLine($"Folder Path: {folder}\n" );
        Console.WriteLine($"Connecting to Engine Control System.... SUCCESS!" );
        Console.WriteLine($"Connecting to Landing Control System.... SUCCESS!" );
        Console.WriteLine($"Connecting to Interplanetary Communication System.... SUCCESS!" );
        Console.WriteLine($"All systems are GO!" );
        
        var machine = new VirtualMachine(debug);
        await machine.Run(folder);    
    }
    
}