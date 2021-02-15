Module Program
    Async Function MainAsync() As Task
        For i = 3 To 1 Step -1
            Console.WriteLine(i)
            Await Task.Delay(1000)
        Next
        Console.WriteLine("Hello World!")
    End Function
End Module
