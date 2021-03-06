Imports EnvDTE
Imports System.Diagnostics
Imports System.IO

Public Module Valve

    Sub CloseToolWindows()
        DTE.ExecuteCommand("View.CommandWindow")
        DTE.Windows.Item(Constants.vsWindowKindCommandWindow).Close()

        DTE.Windows.Item("{73F6DD5A-437E-11D3-B88E-00C04F79F802}").Close()

        DTE.ExecuteCommand("View.Output")
        DTE.Windows.Item(Constants.vsWindowKindOutput).Close()

        DTE.ExecuteCommand("View.TaskList")
        DTE.Windows.Item(Constants.vsWindowKindTaskList).Close()

        DTE.ExecuteCommand("View.FindResults1")
        DTE.Windows.Item(Constants.vsWindowKindFindResults1).Close()

        DTE.ExecuteCommand("View.FindResults2")
        DTE.Windows.Item(Constants.vsWindowKindFindResults2).Close()
    End Sub

    Sub ModuleHeader()
        'DESCRIPTION: This macro adds the standard copyright information to the top of a module.

        ActiveDocument.Selection.StartOfDocument()

        ' Create the standard file prologue
        Dim Header As String

        Header = "//====== Copyright © 1996-2005, Valve Corporation, All rights reserved. =======" + vbCrLf
        Header = Header + "//" + vbCrLf
        Header = Header + "// Purpose: " + vbCrLf
        Header = Header + "//" + vbCrLf
        Header = Header + "//=============================================================================" + vbCrLf + vbCrLf

        ' Add the single inclusion macros for header files
        Dim DotHPos As String
        DotHPos = InStr(ActiveDocument.Name, ".h")

        Dim InclusionKey As String
        If DotHPos > 0 Then
            InclusionKey = Left(ActiveDocument.Name, DotHPos - 1)
            InclusionKey = UCase(InclusionKey) + "_H"
            Header = Header + "#ifndef " + InclusionKey + vbCrLf
            Header = Header + "#define " + InclusionKey + vbCrLf
            Header = Header + "#ifdef _WIN32" + vbCrLf
            Header = Header + "#pragma once" + vbCrLf
            Header = Header + "#endif" + vbCrLf + vbCrLf
        End If

        ActiveDocument.Selection.Text = Header

        ' Add the "#endif" for header files
        If DotHPos > 0 Then
            ActiveDocument.Selection.EndOfDocument()
            Header = vbCrLf + "#endif // " + InclusionKey + vbCrLf
            ActiveDocument.Selection.Text = Header
        End If

        ActiveDocument.Selection.StartOfDocument()

    End Sub


    Sub TypeHeader()
        'DESCRIPTION: This macro adds a description block above a type declaration

        ' Select the text on the current line and store it for parsing
        ActiveDocument.Selection.SelectLine()

        Dim TypeDec As String

        TypeDec = ActiveDocument.Selection.Text
        ActiveDocument.Selection.StartOfLine()

        ' Check to make sure that this line is a type delcaration
        If InStr(TypeDec, "class") = 0 And InStr(TypeDec, "struct") = 0 And InStr(TypeDec, "interface") = 0 And InStr(TypeDec, "enum") = 0 Then
            MsgBox("This line does not contain a class, struct, interface, or enum declaration.")
        Else
            ' Construct the type header
            Dim Header As String
            Header = "//-----------------------------------------------------------------------------" + vbCrLf
            Header = Header + "// Purpose: " + vbCrLf
            Header = Header + "//-----------------------------------------------------------------------------" + vbCrLf

            ' Write the header
            ActiveDocument.Selection.Text = Header
        End If

    End Sub

    Sub FunctionHeader()
        'DESCRIPTION: This macro creates a function header for C functions or C++ member functions

        ' Select the text on the current line and store it for parsing
        ActiveDocument.Selection.SelectLine()
        Dim FunctionImp = ActiveDocument.Selection.Text
        ActiveDocument.Selection.StartOfLine()
        Dim LineNum = ActiveDocument.Selection.CurrentLine

        Dim FunctionName = Left(FunctionImp, InStr(FunctionImp, "("))

        ' Check to make sure that this line is a class delcaration
        If Len(FunctionName) = 0 Then
            MsgBox("This line does not contain a function implementation.")
        Else
            Dim FuncArray = Split(FunctionName)

            Dim ReturnType = ""

            ' Get the return type and function name
            Dim Element
            For Each Element In FuncArray
                If InStr(Element, "(") = 0 Then
                    ReturnType = ReturnType + Element + " "
                Else
                    FunctionName = Left(Element, Len(Element) - 1)
                End If
            Next

            ReturnType = Trim(ReturnType)

            If ReturnType = "BOOL" Then
                ReturnType = "Returns TRUE on success, FALSE on failure."
            ElseIf ReturnType = "bool" Then
                ReturnType = "Returns true on success, false on failure."
            End If

            ' Place the function parameters in its own string
            Dim Params = Right(FunctionImp, Len(FunctionImp) - InStr(FunctionImp, "("))
            Do While InStr(Params, ")") = 0
                ActiveDocument.Selection.LineDown()
                ActiveDocument.Selection.SelectLine()
                Params = Left(Params, InStr(Params, vbCrLf) - 1)
                Params = Params + Trim(ActiveDocument.Selection.Text)
            Loop
            ActiveDocument.Selection.GotoLine(LineNum - 1)
            Params = Left(Params, InStr(Params, ")") - 1)
            Params = Trim(Params)

            ' Remove any /* */ comments from Params
            Dim Pos = InStr(Params, "/*")
            Do While Pos
                Dim EndComment = InStr(Params, "*/")
                If EndComment Then
                    Dim StartString = Left(Params, InStr(Params, "/*") - 1)
                    Dim Pos2 = Len(Params) - InStr(Params, "*/") + 3
                    Dim EndString = Mid(Params, InStr(Params, "*/") + 2, Pos2)

                    StartString = Trim(StartString)
                    EndString = Trim(EndString)

                    Params = StartString + EndString
                    Pos = InStr(Params, "/*")
                Else
                    Pos = 0
                End If
            Loop

            ' Create an array of individual parameters
            Dim ParamsArray = Split(Params, ",")

            ' Construct the parameters section
            Dim ParamSection = ""
            Dim AddNewLine = 0
            For Each Element In ParamsArray
                Element = Trim(Element)
                Element = Right(Element, Len(Element) - InStrRev(Element, " "))
                If AddNewLine = 1 Then
                    ParamSection = ParamSection + vbCrLf + "//			"
                End If
                ParamSection = ParamSection + Element + " - "
                AddNewLine = 1
            Next

            ' Construct the rest of the header
            Dim Header = "//-----------------------------------------------------------------------------" + vbCrLf
            Header = Header + "// Purpose: " + vbCrLf

            If ParamSection <> "void - " And ParamSection <> "" Then
                Header = Header + "// Input  : " + ParamSection + vbCrLf
            End If

            If ReturnType <> "void" And ReturnType <> "" Then
                Header = Header + "// Output : " + ReturnType + vbCrLf
            End If

            Header = Header + "//-----------------------------------------------------------------------------" + vbCrLf

            ' Write the header
            ActiveDocument.Selection.Text = Header

        End If
    End Sub

    'DESCRIPTION: Comments in or out a line of code, then moves to the next line.
    Sub ToggleComment()

        ActiveDocument.Selection.SelectLine()
        Dim LineText As String = ActiveDocument.Selection.Text

        Dim FirstTwoChars = Left(LineText, 2)
        If Len(FirstTwoChars) < 2 Then
            ActiveDocument.Selection.Text = ""
        ElseIf FirstTwoChars = "//" Then
            ActiveDocument.Selection.Text = Right(LineText, Len(LineText) - 2)
        Else
            ActiveDocument.Selection.Text = "//" + LineText
        End If

    End Sub

    Sub JumpToHeader()
        '////////////////////////////////////////////
        'DESCRIPTION: Switch Between Header and cpp
        '////////////////////////////////////////////
        Dim myDocument
        Dim a
        Dim b
        Dim c
        Dim Flag
        Dim Flag1
        Flag1 = 0
        Flag = 1
        a = ActiveDocument.FullName
        Dim tmp = InStr(a, ".cpp")
        If tmp Then
            b = Left(a, Len(a) - 3) + "h"
            c = Left(a, Len(a) - 3) + "h"
            Flag1 = 1
        Else
            tmp = InStr(a, ".c")
            If tmp Then
                b = Left(a, Len(a) - 1) + "h"
                c = Left(a, Len(a) - 1) + "h"
                Flag1 = 1
            Else
                tmp = InStr(a, ".h")
                If tmp Then
                    b = Left(a, Len(a) - 1) + "c"
                    c = Left(a, Len(a) - 1) + "cpp"
                    Flag1 = 1
                End If
            End If
        End If
        For Each myDocument In Application.Documents
            If myDocument.FullName = b Then
                myDocument.Active = True
                Flag = 0
                Exit For
            End If

            If myDocument.FullName = c Then
                myDocument.Active = True
                Flag = 0
                b = c
                Exit For
            End If

        Next
        If Flag And Flag1 Then
            DTE.Documents.Open(b, "Text")
        End If
    End Sub
End Module

