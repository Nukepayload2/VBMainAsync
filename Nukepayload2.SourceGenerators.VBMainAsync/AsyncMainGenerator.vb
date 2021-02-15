Imports System.Text
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

<Generator(LanguageNames.VisualBasic)>
Public Class AsyncMainGenerator
    Implements ISourceGenerator

    Public Sub Initialize(context As GeneratorInitializationContext) Implements ISourceGenerator.Initialize
        ' Register syntax receiver
        context.RegisterForSyntaxNotifications(
            Function() New AllModulesSyntaxReceiver)
    End Sub

    Public Sub Execute(context As GeneratorExecutionContext) Implements ISourceGenerator.Execute
        ' Get syntax receiver
        Dim receiver = TryCast(context.SyntaxReceiver, AllModulesSyntaxReceiver)
        If receiver Is Nothing Then Return

        Dim compilation = context.Compilation
        Dim taskTypeSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")
        Dim stringSymbol = compilation.GetSpecialType(SpecialType.System_String)
        Dim stringArraySymbol = compilation.CreateArrayTypeSymbol(stringSymbol)

        Dim mainAsyncSymbols =
            From mdl In receiver.Modules
            Let semModel = compilation.GetSemanticModel(mdl.SyntaxTree),
                moduleSymbol = DirectCast(semModel.GetDeclaredSymbol(mdl), INamedTypeSymbol),
                MainAsync = (
                    From member In moduleSymbol.GetMembers
                    Where member.Kind = SymbolKind.Method AndAlso member.Name = "MainAsync"
                    Let MethodSymbol = DirectCast(member, IMethodSymbol),
                        retType = MethodSymbol.ReturnType
                    Where retType.Equals(taskTypeSymbol, SymbolEqualityComparer.Default)
                    Let params = MethodSymbol.Parameters,
                        NoParams = params.Length = 0
                    Where NoParams OrElse (
                        params.Length = 1 AndAlso
                        params.First.Type.Equals(stringArraySymbol, SymbolEqualityComparer.Default)
                    )
                    Select MethodSymbol, NoParams
                )
            Select ModuleName = moduleSymbol.Name, MainAsync = MainAsync.FirstOrDefault
            Where MainAsync IsNot Nothing

        Const Indent = "    "
        Const Indent2 = Indent + Indent
        Dim code As New StringBuilder
        For Each symbol In mainAsyncSymbols
            Dim argDecl = String.Empty
            Dim argCall = String.Empty
            If Not symbol.MainAsync.NoParams Then
                argDecl = "args As String()"
                argCall = "(args)"
            End If

            code.AppendLine($"Partial Module {symbol.ModuleName}
{Indent}Sub Main({argDecl})
{Indent2}{symbol.MainAsync.MethodSymbol.Name}{argCall}.GetAwaiter.GetResult()
{Indent}End Sub
End Module")
        Next

        If code.Length = 0 Then Return
        context.AddSource("GeneratedStartupObjects", code.ToString)
    End Sub

    Private Class AllModulesSyntaxReceiver
        Implements ISyntaxReceiver

        Public ReadOnly Property Modules As New List(Of ModuleBlockSyntax)

        Public Sub OnVisitSyntaxNode(syntaxNode As SyntaxNode) Implements ISyntaxReceiver.OnVisitSyntaxNode
            Dim node = TryCast(syntaxNode, ModuleBlockSyntax)
            If node IsNot Nothing Then
                ' Add all modules
                Modules.Add(node)
            End If
        End Sub
    End Class
End Class
