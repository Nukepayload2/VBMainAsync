# VBMainAsync
Async entry point support for VB (16.9+)

https://www.nuget.org/packages/Nukepayload2.SourceGenerators.VBMainAsync

## Requirements
- Visual Studio 16.9 preview 2 or later
- .NET SDK 5.0.2 or later

## Features
- Use `Async Function MainAsync() As Task` as entry point
- Use `Async Function MainAsync(args As String()) As Task` as entry point

## Usage
This project haven't been published to Nuget yet. So, you can only use it as project reference.

- Add the source generator project to your solution.
- Build the source generator project.
- Select your console project, and add project reference to the source generator.
- Edit your project file. In the `<ProjectReference>` element, add ` OutputItemType="Analyzer" ReferenceOutputAssembly="False"` .
- Reload the solution.

## Sample
The following `MainAsync` function can be used as startup object:
```vbnet
Module Program
    Async Function MainAsync() As Task
        For i = 3 To 1 Step -1
            Console.WriteLine(i)
            Await Task.Delay(1000)
        Next
        Console.WriteLine("Hello World!")
    End Function
End Module
```
