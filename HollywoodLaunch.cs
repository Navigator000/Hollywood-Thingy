using System.Collections.Concurrent;

// i would like to credit this program to me and JetBrains Rider 
//init
SecuritySystem system = new(39441, 17801);
Display displayInstance = new();

//main
while (true) 
{
    Console.Beep();
    Console.Clear();
    
    if (system.IsArmed())
    {
        Console.WriteLine("[GLS_SYS]: ACCESS CODE VALID. AUTHORIZING ACCESS TOKEN..."); // gls means nothing btw it's just to get a name
        await Task.Delay(1500);
        Console.Clear();

        if (system.PassCheck())
        {
            await Task.Delay(1000);
            Console.Clear();
            Console.WriteLine("======= FLIGHT OPERATIONS INTERFACE =======");
            Console.WriteLine("Select countdown sequence length (seconds) [Default: 10s]:");
            
            var timerInput = Console.ReadLine();
            if (!int.TryParse(timerInput, out int launchWindowTime) || launchWindowTime <= 0)
            {
                launchWindowTime = 10;
            }
            
            Console.Clear();             
            
            // async countdown
            await displayInstance.RunVisualCountdown(launchWindowTime);
            break;
        }
        else
        {
            displayInstance.DrawSolidAlertBlock("PASSPHRASE VERIFICATION FAILED", false);
            await Task.Delay(2000);
        }
    }
    else
    {
        displayInstance.DrawSolidAlertBlock("HARDWARE SECURITY ARM FAILED", false);
        await Task.Delay(2000);
    }
}

//classes

public class SecuritySystem //this handles entry
{
    private int ArmCode { get; }
    private int Passcode { get; }

    public SecuritySystem(int armCode, int passcode)
    {
        ArmCode = armCode; // i realise it is basically impossible to get the code unless you find it. so im putting it in the read me. this will be a lot of fun and ill get to see who doesnt read the readmes
        Passcode = passcode;
    }

    private string ReadMaskedInput(string prompt)
    {
        Console.WriteLine(prompt);
        string input = "";
        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (input.Length > 0)
                {
                    input = input.Remove(input.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else if (keyInfo.KeyChar != '\u0000')
            {
                input += keyInfo.KeyChar;
                Console.Write("*");
            }
        }
        return input; // this whole snippet gives that **** masking to the code for that encryption effect
    }

    public bool IsArmed()
    {
        string input = ReadMaskedInput("Enter Ground Station Terminal Arm Code:");
        return int.TryParse(input, out int verify) && verify == ArmCode;
    }

    public bool PassCheck()
    {
        string input = ReadMaskedInput("Enter Mission Director Authorization Key:"); // i tried to make it as Hollywood as possible
        return int.TryParse(input, out int finalpass) && finalpass == Passcode; // the problem is that it sounds awful
    }
}

public class Display


{
    //this is where the main cool display stuff is added. a lot of these logs are fake because.. we don't have a real rocket.
    private readonly ConcurrentQueue<string> _telemetryLog = new();
    private readonly string[] _mockEvents = [
        "PROP_LOX: Pressurizing main oxidizer tanks...",
        "AVIONICS: Switching main computer array to internal cell arrays.",
        "FTS_MGR: Ordnance line loop back test [NOMINAL].",
        "GUIDANCE: Telemetry link established with launch vehicle platform A.",
        "PAD_ENG: Retracting umbilical connections to upper stage structural nodes." // this is NOT how rocket logs work this is pure and deliberate hollywood nonsense 
    ];
    public async Task PostLaunch()
    {
        Console.Clear();

        // we initialise "controls"
        string activeCmdBuffer = "";
        bool injectOverspeed = false;
        bool ignoreAlertSystem = false;

        int totalCalculatedPoints = 10000;
        int monitorRefreshRateMs = 10; 

        int lastArcX = -1; int lastArcY = -1;
        int lastAppX = -1; int lastAppY = -1;

        //  calculates data for stuff like progress and current pos
        for (int step = 0; step <= totalCalculatedPoints; step++)
        {
            double progress = (double)step / totalCalculatedPoints;
            double currentMET = progress * 450.0; 

            // calculate currentRange at the very top so that the stupid and dumb overspeed thing reads it
            int currentRange = 15000 - (int)(progress * 14531);

            // this was hell i had to learn more math
            int r = 5400 - (int)(progress * 4900);
            int x = 5390 - (int)(progress * 4850);
            int y = 3 + (int)(Math.Sin(currentMET * 0.1) * 3 * (1.0 - progress)); // we use sin here because angular trajectories are basically a requirement for this postlaunch thing
            int z = 180 - (int)(progress * 165);

            double rdot = 12.50 - (progress * 12.17) + (Math.Sin(currentMET * 0.05) * 0.15 * (1.0 - progress));
            double xdot = 11.80 - (progress * 11.48);
            double ydot = -0.05 + (Math.Cos(currentMET * 0.08) * 0.04 * (1.0 - progress));
            double zdot = -1.45 + (progress * 1.33); //obvious

            int currentElv = (int)(-3.14 + (Math.Sin(currentMET * 0.02) * 0.8 * (1.0 - progress)));
            int currentAzi = (int)(1.17 + (Math.Cos(currentMET * 0.02) * 0.4 * (1.0 - progress)));

            // inject overspeed is basically a test command that can be inputted to show overspeed alert. this is ignorable
            if (injectOverspeed) 
            { 
                rdot = 9.85; 
            }

            // this is the visuals
            if (step % 20 == 0 || step == totalCalculatedPoints)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                
                // the background
                for (int h = 2; h <= 12; h++)
                {
                    int w = (int)Math.Round(18 + Math.Sqrt(Math.Max(0, 100 - (h - 7) * (h - 7) * 4)));
                    Console.SetCursorPosition(w, h); Console.Write("\\");
                }

                // tunnel frame
                Console.SetCursorPosition(46, 11); Console.Write("=======[ 500 ft APPROACH ]===");
                Console.SetCursorPosition(52, 12); Console.Write("\\           /");
                Console.SetCursorPosition(52, 13); Console.Write(" \\_________/ ");
            }
            
            Console.SetCursorPosition(0, 15);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(new string('-', Console.WindowWidth));

            // we keep the data here so that it is away from the visual elements and it gives you what COULD Be real data. Again this is almost definitely not accurate
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(1, 16); Console.Write($"R   {r,-5} "); 
            Console.SetCursorPosition(1, 17); Console.Write($"X   {x,-5} ");
            Console.SetCursorPosition(1, 18); Console.Write($"Y   {y,-3} ");
            Console.SetCursorPosition(1, 19); Console.Write($"Z   {z,-3} ");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(14, 16); Console.Write($"R_dot  {rdot,6:F2} ");
            Console.SetCursorPosition(14, 17); Console.Write($"X_dot  {xdot,6:F2} ");
            Console.SetCursorPosition(14, 18); Console.Write($"Y_dot  {ydot,6:F2} ");
            Console.SetCursorPosition(14, 19); Console.Write($"Z_dot  {zdot,6:F2} ");

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(40, 16); Console.Write($"MET: 10/15:24:{(int)currentMET:D3}   Pitch: 179    ");
            Console.SetCursorPosition(40, 17); Console.Write($"Alt: 206             Azi:   {currentAzi,5:F2} ");
            Console.SetCursorPosition(40, 18); Console.Write($"[HHL/dt]  Rng: {currentRange,-5}  Rdot: {rdot,5:F2} ");
            Console.SetCursorPosition(40, 19); Console.Write($"[HHLRaw]  Rng: {currentRange,-5}  Rdot: 1.75 ");

            //  le master caution system
            if (currentRange < 2000 && rdot > 5.0 && !ignoreAlertSystem)
            {
                Console.SetCursorPosition(25, 14);
                if ((step / 10) % 2 == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("OVERSPEED"); // you can make it hide and show itself at intervals to make it blink
                    Console.Write("\a"); // but if you are planning to do that i ask you to reconsider
                    Console.ResetColor(); // we keep it from not blinking so that EVERYONE sees that the thing is going overspeed
                }
            }
            else
            {
                Console.SetCursorPosition(25, 14);
                Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write(new string('-', 44));
                Console.ResetColor();
            }

            // this plots everything on the left (the rocket left)
            int arcPlotX = 2 + (int)Math.Round(progress * 22);
            int arcPlotY = 13 - (int)Math.Round(Math.Pow(progress, 0.4) * 10.0);

            if (arcPlotX > 25) arcPlotX = 25;

            if (lastArcX != -1 && (lastArcX != arcPlotX || lastArcY != arcPlotY))
            {
                Console.SetCursorPosition(lastArcX, lastArcY);
                Console.ForegroundColor = ConsoleColor.DarkGreen;

                char dynamicTrailChar = '.';
                int deltaX = Math.Abs(arcPlotX - lastArcX);
                int deltaY = Math.Abs(lastArcY - arcPlotY);

                if (deltaX > 0 && deltaY > 0)        dynamicTrailChar = (deltaY > deltaX) ? '|' : '/';
                else if (deltaX > 0 && deltaY == 0)   dynamicTrailChar = '-';
                else if (deltaX == 0 && deltaY > 0)   dynamicTrailChar = '|';

                Console.Write(dynamicTrailChar);
            }
            Console.SetCursorPosition(arcPlotX, arcPlotY);
            Console.ForegroundColor = ConsoleColor.Green; Console.Write("▲");
            lastArcX = arcPlotX; lastArcY = arcPlotY;

            // right side
            int appPlotX = 46 + 10 + (int)(18 * (1.0 - progress));
            int appPlotY = 12 + (int)Math.Round(Math.Sin(currentMET * 0.1) * 1.0 * (1.0 - progress));

            if (lastAppX != -1 && (lastAppX != appPlotX || lastAppY != appPlotY))
            {
                Console.SetCursorPosition(lastAppX, lastAppY);
                Console.ForegroundColor = ConsoleColor.DarkCyan; Console.Write("-");
            }
            Console.SetCursorPosition(appPlotX, appPlotY);
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("◄");
            lastAppX = appPlotX; lastAppY = appPlotY;

            // this reads those commands. so you can type overspeed or ignore. again, could be solved with just a button widget but this is hollywood
            //style. practicality is in hell.
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyPress = Console.ReadKey(true);
                
                if (keyPress.Key == ConsoleKey.Enter)
                {
                    string finalCmd = activeCmdBuffer.Trim().ToUpper();
                    activeCmdBuffer = ""; 

                    if (finalCmd == "OVERSPEED") injectOverspeed = true;
                    if (finalCmd == "IGNORE")    ignoreAlertSystem = true;
                    if (finalCmd == "RESET")     { injectOverspeed = false; ignoreAlertSystem = false; }
                }
                else if (keyPress.Key == ConsoleKey.Backspace)
                {
                    if (activeCmdBuffer.Length > 0) activeCmdBuffer = activeCmdBuffer.Remove(activeCmdBuffer.Length - 1);
                }
                else if (keyPress.KeyChar != '\u0000')
                {
                    activeCmdBuffer += keyPress.KeyChar;
                }
            }

            // footer
            Console.SetCursorPosition(0, 22);
            Console.ForegroundColor = ConsoleColor.Yellow;
            string unifiedFooterText = $"[MATRIX: {step}/{totalCalculatedPoints}] Rng: {currentRange}ft | CMD PROMPT > {activeCmdBuffer}";
            Console.Write(unifiedFooterText.PadRight(Console.WindowWidth));
            Console.ResetColor();

            await Task.Delay(monitorRefreshRateMs);
        }

        Console.Clear(); // again just more hollywood bs
        DrawSolidAlertBlock("DOCKING LOCK ACQUIRED // COUPLING MECHANICAL CONNECTIONS NOMINAL", true);
        await Task.Delay(4000);
    }



    public void DrawSolidAlertBlock(string text, bool isSuccess, char spinnerFrame = ' ')  //yknow how we see those ACCESS DENIED stuff in hollywood movies? this is an attempt at making just that
    {
        // this is for the alert block. all of this is simply position logic
        int startTop = 2; 

        string contentText = spinnerFrame != ' ' ? $"{text} {spinnerFrame}" : text;
        int totalWidth = Math.Max(40, contentText.Length + 4);  
        int globalLeftOffset = Math.Max(0, (Console.WindowWidth - totalWidth) / 2); 

        int leftSpacesCount = (totalWidth - contentText.Length) / 2; 
        int rightSpacesCount = totalWidth - contentText.Length - leftSpacesCount;
        
        string emptyRow = new string(' ', totalWidth);
        string centeredRow = $"{new string(' ', leftSpacesCount)}{contentText}{new string(' ', rightSpacesCount)}";

        Console.SetCursorPosition(globalLeftOffset, startTop);
        Console.BackgroundColor = isSuccess ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed; 
        Console.ForegroundColor = ConsoleColor.White; 
        Console.Write(emptyRow);

        Console.SetCursorPosition(globalLeftOffset, startTop + 1);
        Console.Write(centeredRow);

        Console.SetCursorPosition(globalLeftOffset, startTop + 2);
        Console.Write(emptyRow);
        Console.ResetColor(); 
    }

    public async Task RunVisualCountdown(int seconds) //The reason the names are so literal is because they used to be just lazy names like function, main, drawer
    { //its an awful solution because eventually it gets kind of annoying remembering the purpose for each
        // so out of spite i decided to just follow regular naming convention
        // a lot of this refactoring was again done with the help of JB Rider
        Console.CursorVisible = false;
        _telemetryLog.Clear();
        
        using var countdownCts = new CancellationTokenSource();
        bool abortTriggered = false;

        // this thread runs the spinner
        Task animationTask = Task.Run(() => SpinAnimationLoop(seconds, countdownCts.Token));

        //this thread runs the telemetry
        Task telemetryTask = Task.Run(() => GenerateTelemetryLoop(countdownCts.Token));

        DateTime startTime = DateTime.UtcNow;

        // this thread watches for abort
        while (!countdownCts.Token.IsCancellationRequested)
        {
            double elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
            int remaining = (int)Math.Ceiling(seconds - elapsed);
            if (remaining <= 0) break;

            if (Console.KeyAvailable)
            {
                Console.SetCursorPosition(0, Console.WindowHeight - 2);
                Console.Write("ENTER COMMAND: ");

                #pragma warning disable CS8600
                string cmd = Console.ReadLine()?.Trim().ToUpper(); // null causes crash
                #pragma warning restore CS8600 // lazy will fix late
                
                if (cmd == "ABORT")
                {
                    abortTriggered = true;
                    countdownCts.Cancel();
                    break;
                }
            }
            await Task.Delay(100);
        }

        // stops async tasks
        countdownCts.Cancel();
        await Task.WhenAll(animationTask, telemetryTask);

        Console.Clear();
        if (abortTriggered)
        {
            DrawSolidAlertBlock("LAUNCH SEQUENCER MANUALLY ABORTED // SAFING TANKS", false);
            if (OperatingSystem.IsWindows())
                {
                    Console.Beep(400, 800);
                }
                else
                {
                    Console.Write("\a"); // for linux
                }

        }
        else
        {
            DrawSolidAlertBlock("IGNITION SEQUENCE COMPLETE // LIFTOFF", true);
            if (OperatingSystem.IsWindows())
                {
                    Console.Beep(800, 800);
                }
                else
                {
                    Console.Write("\a"); 
                }

                await Task.Delay(4000);
                await PostLaunch();
                Console.CursorVisible = true;
        }
        
        await Task.Delay(4000);
        Console.CursorVisible = true; //python needed some ansi for this thank you for the ease c#
    }

    private void SpinAnimationLoop(int totalSeconds, CancellationToken token)
    {
        char[] frames = ['|', '/', '-', '\\'];
        int frameIndex = 0;
        DateTime startTime = DateTime.UtcNow;

        while (!token.IsCancellationRequested)
        {
            double elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
            int remainingSeconds = (int)Math.Ceiling(totalSeconds - elapsedSeconds);

            if (remainingSeconds <= 0) break;

            string displayMessage = $"LAUNCH SEQUENCE T-MINUS {remainingSeconds}s";
            
            // we didnt use console.clear cause the screen flash can look ugly
            DrawSolidAlertBlock(displayMessage, true, frames[frameIndex]);
            DrawTelemetryTerminalArea();

            frameIndex = (frameIndex + 1) % frames.Length;
            Thread.Sleep(100);
        }
    }

    private void GenerateTelemetryLoop(CancellationToken token)
    {
        Random rand = new();
        int index = 0;
        
        while (!token.IsCancellationRequested)
        {
            Thread.Sleep(rand.Next(1000, 2500)); // telemetry reports at varying timing intervals so its always random
            if (token.IsCancellationRequested) break;

            string logLine = index < _mockEvents.Length 
                ? _mockEvents[index++] 
                : $"SYS_MON: Propulsion line pressure at {rand.Next(290, 310)} kPa [NOMINAL].";

            _telemetryLog.Enqueue($"[{DateTime.UtcNow:HH:mm:ss}] {logLine}"); 
        }
    }

    private void DrawTelemetryTerminalArea()
    {
        // this is so we can print the telemetry in a specified area
        int logStartRow = 7;
        int maxVisibleLogs = Math.Max(3, Console.WindowHeight - logStartRow - 4);
        
        Console.SetCursorPosition(0, logStartRow);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("--- SYSTEM TELEMETRY STREAM -----------------------------------");
        Console.ResetColor();

        var logs = _telemetryLog.ToArray();
        int startIndex = Math.Max(0, logs.Length - maxVisibleLogs);

        for (int i = 0; i < maxVisibleLogs; i++)
        {
            int logIndex = startIndex + i;
            Console.SetCursorPosition(0, logStartRow + 1 + i);
            
            if (logIndex < logs.Length)
            {
                // clears line ahead of writing to wipe previous text lengths 
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, logStartRow + 1 + i);
                Console.WriteLine(logs[logIndex]);
            }
        }

        // this is where the user can put a command "abort"
        Console.SetCursorPosition(0, Console.WindowHeight - 2);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Type 'ABORT' and hit Enter to halt countdown immediately: "); //its over
        Console.ResetColor();
    }
}

public class FlightData
{
    public int Time { get; set; } //json read functions
    public int X { get; set; }
    public int Y { get; set; }
    public int Alt { get; set; }
    public int Vel { get; set; }
    #pragma warning disable 
    public string Log { get; set; } // i genuinely had no clue on how to fix this so i just used pragma
    // is it a cardinal sin? who knows
}