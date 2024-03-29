﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tiny_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        
        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public  Node root;

        public Node StartParsing(List<Token> TokenStream)
        {

            this.InputPointer = 0;
            this.TokenStream = new List<Token>();

            for (int i = 0; i < TokenStream.Count; i++)
            {

                this.TokenStream.Add(TokenStream[i]);

            }
            root = new Node("RootNode");

            root.Children.Add(Program());

            return root;
        }
        Node Program()
        {
            Node program = new Node("Program");

            program.Children.Add(FunctionBlock());
            program.Children.Add(MainBlock());

            return program;
        }
        Node FunctionBlock()
        {
            Node functions = new Node("FunctionBlock");

            if (InputPointer + 2 < TokenStream.Count &&
                (Token_Class.Integer == TokenStream[InputPointer].token_type
                || Token_Class.T_String == TokenStream[InputPointer].token_type
              || Token_Class.Float == TokenStream[InputPointer].token_type)
                && Token_Class.Identifier == TokenStream[InputPointer + 1].token_type
                && Token_Class.LParanthesis == TokenStream[InputPointer + 2].token_type)
            {
                functions.Children.Add(Function());
                functions.Children.Add(FunctionBlock());
            }
            else
                return null;

            return functions;
        }

        Node MainBlock()
        {
            Node mainFn = new Node("MainFunction");

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Integer)
            {
                mainFn.Children.Add(match(Token_Class.Integer));
            }

            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Float)
            {
                mainFn.Children.Add(match(Token_Class.Float));
            }

            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.T_String)
            {
                mainFn.Children.Add(match(Token_Class.T_String));
            }
            mainFn.Children.Add(match(Token_Class.Main));
            mainFn.Children.Add(match(Token_Class.LParanthesis));
            mainFn.Children.Add(match(Token_Class.RParanthesis));
            mainFn.Children.Add(FunctionBody());

            // To Don't Appear in Parser Tree
            if (mainFn.Children.ElementAt(0) == null)
                return null;

            return mainFn;
        }
        Node Function()
        {
            Node function = new Node("function");

            function.Children.Add(FunctionDecl());
            function.Children.Add(FunctionBody());

            return function;
        }

        Node FunctionDecl()
        {
            Node functiondecl = new Node("functiondecl");

            if (TokenStream[InputPointer].token_type == Token_Class.Integer)
            {
                functiondecl.Children.Add(match(Token_Class.Integer));
            }

            else if (TokenStream[InputPointer].token_type == Token_Class.Float)
            {
                functiondecl.Children.Add(match(Token_Class.Float));
            }

            else if (TokenStream[InputPointer].token_type == Token_Class.T_String)
            {
                functiondecl.Children.Add(match(Token_Class.T_String));
            }
            functiondecl.Children.Add(FunctionName());
            functiondecl.Children.Add(ParameterList());

            return functiondecl;
        }
        Node FunctionName()
        {
            Node FnName = new Node("FnName");
            FnName.Children.Add(match(Token_Class.Identifier));
            return FnName;
        }

        Node ParameterList()
        {
            Node paramList = new Node("paramList");

            paramList.Children.Add(match(Token_Class.LParanthesis));
            paramList.Children.Add(Parameter());
            paramList.Children.Add(match(Token_Class.RParanthesis));

            return paramList;
        }

        Node Parameter()
        {
            Node parameter = new Node("parameter");

            if ((Token_Class.Integer == TokenStream[InputPointer].token_type
                || Token_Class.String == TokenStream[InputPointer].token_type
              || Token_Class.Float == TokenStream[InputPointer].token_type)
            && Token_Class.Identifier == TokenStream[InputPointer + 1].token_type)
            {
                if (TokenStream[InputPointer].token_type == Token_Class.Integer)
                {
                    parameter.Children.Add(match(Token_Class.Integer));
                }

                else if (TokenStream[InputPointer].token_type == Token_Class.Float)
                {
                    parameter.Children.Add(match(Token_Class.Float));
                }

                else if (TokenStream[InputPointer].token_type == Token_Class.T_String)
                {
                    parameter.Children.Add(match(Token_Class.T_String));
                }
                parameter.Children.Add(match(Token_Class.Identifier));
                parameter.Children.Add(Parameters());
            }
            else
                return null;

            return parameter;
        }

        Node Parameters()
        {
            Node parameters = new Node("Parameters");

            if (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                parameters.Children.Add(match(Token_Class.Comma));
                if (TokenStream[InputPointer].token_type == Token_Class.Integer)
                {
                    parameters.Children.Add(match(Token_Class.Integer));
                }

                else if (TokenStream[InputPointer].token_type == Token_Class.Float)
                {
                    parameters.Children.Add(match(Token_Class.Float));
                }

                else if (TokenStream[InputPointer].token_type == Token_Class.T_String)
                {
                    parameters.Children.Add(match(Token_Class.T_String));
                }
                parameters.Children.Add(match(Token_Class.Identifier));
                parameters.Children.Add(Parameters());
            }
            else
                return null;

            return parameters;
        }


        // FunctionCall Arguments
        Node ArgumentList()
        {
            Node argList = new Node("argList");
            if (Token_Class.LParanthesis == TokenStream[InputPointer].token_type
                && Token_Class.RParanthesis == TokenStream[InputPointer + 1].token_type)
            {
                argList.Children.Add(match(Token_Class.LParanthesis));
                argList.Children.Add(match(Token_Class.RParanthesis));
            }
            else
            {
                argList.Children.Add(match(Token_Class.LParanthesis));
                argList.Children.Add(Argument());
                argList.Children.Add(match(Token_Class.RParanthesis));
            }

            return argList;
        }



        Node Argument()
        {
            Node argument = new Node("argument");

            // Equation | Term | String

            if (resetInputPtr(InputPointer, Expression()))
            {
                argument.Children.Add(Expression());
                argument.Children.Add(Arguments());
            }

            return argument;
        }

        Node Arguments()
        {

            Node arguments = new Node("arguments");
            if (Token_Class.Comma == TokenStream[InputPointer].token_type
                && (Token_Class.Identifier == TokenStream[InputPointer + 1].token_type
                || Token_Class.Number == TokenStream[InputPointer + 1].token_type
                || Token_Class.LParanthesis == TokenStream[InputPointer + 1].token_type
                || Token_Class.String == TokenStream[InputPointer + 1].token_type))
            {
                arguments.Children.Add(match(Token_Class.Comma));
                arguments.Children.Add(Argument());
            }
            else
                return null;

            return arguments;
        }

        Node FunctionBody()
        {
            Node funcBody = new Node("FunctionBody");

            funcBody.Children.Add(match(Token_Class.Lbraces));
            funcBody.Children.Add(Statements());
            funcBody.Children.Add(ReturnStatement());
            funcBody.Children.Add(match(Token_Class.Rbraces));

            return funcBody;
        }

        Node Statements()
        {
            Node stats = new Node("statements");
            int x = InputPointer;
            if (Statement() != null)
            {
                InputPointer -= InputPointer - x;
                stats.Children.Add(Statement());
                stats.Children.Add(Statements());
            }
            else return null;
            return stats;
        }

        Node Statement()
        {
            Node statement = new Node("statement");

            if (InputPointer < TokenStream.Count)
            {
                if (Token_Class.Read == TokenStream[InputPointer].token_type)
                {
                    statement.Children.Add(Read_Statement());
                }
                else if (Token_Class.Write == TokenStream[InputPointer].token_type)
                {
                    statement.Children.Add(Write_Statement());
                }
                // x := 2
                else if (Token_Class.Identifier == TokenStream[InputPointer].token_type
               && Token_Class.Assignment == TokenStream[InputPointer + 1].token_type
               && Token_Class.Semicolon != TokenStream[InputPointer + 2].token_type)
                {
                    statement.Children.Add(Assignment_Statement());
                    statement.Children.Add(match(Token_Class.Semicolon));
                }

                else if (Token_Class.Integer == TokenStream[InputPointer].token_type
                || Token_Class.T_String == TokenStream[InputPointer].token_type
              || Token_Class.Float == TokenStream[InputPointer].token_type)
                {
                    statement.Children.Add(Declaration_Statement());
                }
                else if (Token_Class.Identifier == TokenStream[InputPointer].token_type
                    && Token_Class.LParanthesis == TokenStream[InputPointer + 1].token_type)
                {
                    statement.Children.Add(FunctionCall());
                    statement.Children.Add(match(Token_Class.Semicolon));
                }
                else if (Token_Class.Repeat == TokenStream[InputPointer].token_type)
                {
                    statement.Children.Add(Repeat_Statement());
                }

                else if (Token_Class.If == TokenStream[InputPointer].token_type)
                {
                    statement.Children.Add(If_Statement());
                }
                else
                    return null;
                return statement;
            }
            return null;
        }
        Node Declaration_Statement()
        {
            Node varDecl = new Node("Declaration Statement");

            if (TokenStream[InputPointer].token_type == Token_Class.Integer)
            {
                varDecl.Children.Add(match(Token_Class.Integer));
            }

            else if (TokenStream[InputPointer].token_type == Token_Class.Float)
            {
                varDecl.Children.Add(match(Token_Class.Float));
            }

            else if (TokenStream[InputPointer].token_type == Token_Class.T_String)
            {
                varDecl.Children.Add(match(Token_Class.T_String));
            }
            varDecl.Children.Add(Declaration());

            // Handle Datatype := ;
            // Handle Datatype ;
            // Datatype x :=
            if ((Token_Class.Assignment == TokenStream[InputPointer - 1].token_type
                || (Token_Class.Integer == TokenStream[InputPointer].token_type
                || Token_Class.T_String == TokenStream[InputPointer].token_type
              || Token_Class.Float == TokenStream[InputPointer].token_type))
                && Token_Class.Semicolon == TokenStream[InputPointer].token_type)
                return null;

            varDecl.Children.Add(match(Token_Class.Semicolon));

            return varDecl;
        }

        Node Declaration()
        {
            Node declaration = new Node("Declaration");

            if (Token_Class.Identifier == TokenStream[InputPointer].token_type
                && Token_Class.Assignment == TokenStream[InputPointer + 1].token_type)
            {
                declaration.Children.Add(Assignment_Statement());
                declaration.Children.Add(Declarations());
            }
            else if (Token_Class.Identifier == TokenStream[InputPointer].token_type)
            {
                declaration.Children.Add(match(Token_Class.Identifier));
                declaration.Children.Add(Declarations());
            }

            return declaration;
        }

        Node Declarations()
        {
            Node declarations = new Node("Declarations");
            if (Token_Class.Comma == TokenStream[InputPointer].token_type
                && Token_Class.Identifier == TokenStream[InputPointer + 1].token_type
                && Token_Class.Assignment == TokenStream[InputPointer + 2].token_type)
            {
                declarations.Children.Add(match(Token_Class.Comma));
                declarations.Children.Add(Assignment_Statement());
                declarations.Children.Add(Declarations());
            }
            else if (Token_Class.Comma == TokenStream[InputPointer].token_type)
            {
                declarations.Children.Add(match(Token_Class.Comma));
                declarations.Children.Add(match(Token_Class.Identifier));
                declarations.Children.Add(Declarations());
            }

            else
                return null;

            return declarations;
        }

        Node Assignment_Statement()
        {
            Node AssignState = new Node("AssignState");

            AssignState.Children.Add(match(Token_Class.Identifier));
            AssignState.Children.Add(match(Token_Class.Assignment));
            AssignState.Children.Add(Expression());

            return AssignState;
        }

        Node Read_Statement()
        {
            Node readState = new Node("ReadStatement");

            readState.Children.Add(match(Token_Class.Read));
            readState.Children.Add(match(Token_Class.Identifier));
            readState.Children.Add(match(Token_Class.Semicolon));

            return readState;
        }


        Node Write_Statement()
        {
            Node writeState = new Node("WriteStatement");

            if (Token_Class.Identifier == TokenStream[InputPointer + 1].token_type
                || Token_Class.Number == TokenStream[InputPointer + 1].token_type
                || Token_Class.String == TokenStream[InputPointer + 1].token_type)
            {
                writeState.Children.Add(match(Token_Class.Write));
                writeState.Children.Add(Expression());
            }

            else if (Token_Class.Endl == TokenStream[InputPointer + 1].token_type)
            {
                writeState.Children.Add(match(Token_Class.Write));
                writeState.Children.Add(match(Token_Class.Endl));
            }
            writeState.Children.Add(match(Token_Class.Semicolon));


            return writeState;
        }


        Node Expression()
        {
            Node exp = new Node("Expression");
            int x = InputPointer;

            if (InputPointer < TokenStream.Count && Token_Class.String == TokenStream[InputPointer].token_type)
            {
                exp.Children.Add(match(Token_Class.String));
            }

            else if (resetInputPtr(x, Equation()) && InputPointer + 1 < TokenStream.Count
                && (Token_Class.PlusOp == TokenStream[InputPointer + 1].token_type
                || Token_Class.MinusOp == TokenStream[InputPointer + 1].token_type
                || Token_Class.MultiplyOp == TokenStream[InputPointer + 1].token_type
                || Token_Class.DivideOp == TokenStream[InputPointer + 1].token_type
                || Token_Class.LParanthesis == TokenStream[InputPointer].token_type
                ))
            {
                exp.Children.Add(Equation());
            }
            else if (InputPointer < TokenStream.Count && (Token_Class.Identifier == TokenStream[InputPointer].token_type
                || Token_Class.Number == TokenStream[InputPointer].token_type))
            {
                exp.Children.Add(Term());
            }

            return exp;


        }
        Node Equation()
        {
            Node equation = new Node("Equation");

            if ((InputPointer < TokenStream.Count) && (Token_Class.Identifier == TokenStream[InputPointer].token_type
              || Token_Class.Number == TokenStream[InputPointer].token_type))
            {
                equation.Children.Add(TermAndOp());
            }


            else if ((InputPointer < TokenStream.Count) && Token_Class.LParanthesis == TokenStream[InputPointer].token_type)
            {
                equation.Children.Add(match(Token_Class.LParanthesis));
                equation.Children.Add(Equation());
                equation.Children.Add(match(Token_Class.RParanthesis));
                equation.Children.Add(SubEquation());
            }

            return equation;
        }
        Node TermAndOp()
        {
            Node termAndOp = new Node("TermAndOp");

            if (Token_Class.Identifier == TokenStream[InputPointer].token_type
              || Token_Class.Number == TokenStream[InputPointer].token_type)
            {
                termAndOp.Children.Add(Term());
                termAndOp.Children.Add(SubEquation());
            }
            return termAndOp;
        }
        Node SubEquation()
        {
            Node subEquation = new Node("SubEquation");

            // To Handle -->  x:=2+ ;
            // x:= (2+);
            if ((Token_Class.PlusOp == TokenStream[InputPointer].token_type
                || Token_Class.MinusOp == TokenStream[InputPointer].token_type)
                && Token_Class.Semicolon != TokenStream[InputPointer + 1].token_type
                && Token_Class.RParanthesis != TokenStream[InputPointer + 1].token_type)
            {
                subEquation.Children.Add(AddOp());
                subEquation.Children.Add(Equation());
            }

            else if ((Token_Class.MultiplyOp == TokenStream[InputPointer].token_type
                || Token_Class.DivideOp == TokenStream[InputPointer].token_type)
                && Token_Class.Semicolon != TokenStream[InputPointer + 1].token_type
                && Token_Class.RParanthesis != TokenStream[InputPointer + 1].token_type)
            {
                subEquation.Children.Add(MultOp());
                subEquation.Children.Add(Equation());
            }


            return subEquation;

        }


        Node Term()
        {
            Node term = new Node("Term");

            if (InputPointer + 1 < TokenStream.Count && Token_Class.Identifier == TokenStream[InputPointer].token_type
            && Token_Class.LParanthesis == TokenStream[InputPointer + 1].token_type)
            {
                term.Children.Add(FunctionCall());
            }
            else if (InputPointer < TokenStream.Count && Token_Class.Identifier == TokenStream[InputPointer].token_type)
            {
                term.Children.Add(match(Token_Class.Identifier));
            }
            else if (InputPointer < TokenStream.Count && Token_Class.Number == TokenStream[InputPointer].token_type)
            {
                term.Children.Add(match(Token_Class.Number));
            }

            return term;


        }


        private Node AddOp()
        {
            //AddOp --> + | -
            var addOp = new Node("AddOp");
            if (TokenStream[InputPointer].token_type == Token_Class.PlusOp)
                addOp.Children.Add(match(Token_Class.PlusOp));
            else if (TokenStream[InputPointer].token_type == Token_Class.MinusOp)
                addOp.Children.Add(match(Token_Class.MinusOp));
            return addOp;
        }

        private Node MultOp()
        {
            //MultOp --> * | /
            var mul = new Node("MultOp");
            if (TokenStream[InputPointer].token_type == Token_Class.MultiplyOp)
                mul.Children.Add(match(Token_Class.MultiplyOp));
            else
                mul.Children.Add(match(Token_Class.DivideOp));
            return mul;
        }

        Node FunctionCall()
        {
            Node fnCall = new Node("fnCall");

            fnCall.Children.Add(match(Token_Class.Identifier));
            fnCall.Children.Add(ArgumentList());

            return fnCall;
        }
        Node StatementsWithRet()
        {
            Node st = new Node("StatementsWithRet");
            st.Children.Add(Statements());
            if (Token_Class.Return == TokenStream[InputPointer].token_type)
                st.Children.Add(ReturnStatement());
            else
                return null;
            return st;
        }
        Node If_Statement()
        {
            Node IfStat = new Node("IfStat");
            IfStat.Children.Add(match(Token_Class.If));
            IfStat.Children.Add(ConditionStatement());
            IfStat.Children.Add(match(Token_Class.Then));
            IfStat.Children.Add(Statements());

            if (Token_Class.Elseif == TokenStream[InputPointer].token_type)
                IfStat.Children.Add(ElseIf_Statement());

            else if (Token_Class.Else == TokenStream[InputPointer].token_type)
                IfStat.Children.Add(Else_Statement());

            else if (Token_Class.End == TokenStream[InputPointer].token_type)
                IfStat.Children.Add(match(Token_Class.End));

            return IfStat;
        }

        Node ElseIf_Statement()
        {
            Node elIF = new Node("elseIF");
            elIF.Children.Add(match(Token_Class.Elseif));
            elIF.Children.Add(ConditionStatement());
            elIF.Children.Add(match(Token_Class.Then));
            elIF.Children.Add(Statements());

            if (Token_Class.Elseif == TokenStream[InputPointer].token_type)
                elIF.Children.Add(ElseIf_Statement());

            else if (Token_Class.Else == TokenStream[InputPointer].token_type)
                elIF.Children.Add(Else_Statement());

            else if (Token_Class.End == TokenStream[InputPointer].token_type)
                elIF.Children.Add(match(Token_Class.End));


            return elIF;
        }


        Node Else_Statement()
        {
            Node els = new Node("else");
            els.Children.Add(match(Token_Class.Else));
            els.Children.Add(Statements());
            els.Children.Add(match(Token_Class.End));

            return els;
        }

        Node ReturnStatement()
        {
            Node retStat = new Node("ReturnStat");
            retStat.Children.Add(match(Token_Class.Return));
            retStat.Children.Add(Expression());
            retStat.Children.Add(match(Token_Class.Semicolon));
            return retStat;
        }

        Node Repeat_Statement()
        {
            Node repeatStat = new Node("RepeatStat");

            repeatStat.Children.Add(match(Token_Class.Repeat));
            repeatStat.Children.Add(Statements());
            repeatStat.Children.Add(match(Token_Class.Until));
            repeatStat.Children.Add(ConditionStatement());

            return repeatStat;
        }

        Node ConditionStatement()
        {
            Node ConditionStat = new Node("ConditionStat");

            ConditionStat.Children.Add(Condition());
            ConditionStat.Children.Add(AnotherCondition());

            return ConditionStat;
        }

        Node Condition()
        {
            Node condition = new Node("condition");

            condition.Children.Add(Term());
            if (TokenStream[InputPointer].token_type == Token_Class.LessThanOp)
            {
                condition.Children.Add(match(Token_Class.LessThanOp));
            }

            else if (TokenStream[InputPointer].token_type == Token_Class.GreaterThanOp)
            {
                condition.Children.Add(match(Token_Class.GreaterThanOp));
            }

            else if (TokenStream[InputPointer].token_type == Token_Class.EqualOp)
            {
                condition.Children.Add(match(Token_Class.EqualOp));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.NotEqualOp)
            {
                condition.Children.Add(match(Token_Class.NotEqualOp));
            }
            condition.Children.Add(Term());
            condition.Children.Add(AnotherCondition());
            return condition;
        }

        Node AnotherCondition()
        {
            Node cond = new Node("cond");
            if (Token_Class.Or == TokenStream[InputPointer].token_type || Token_Class.And == TokenStream[InputPointer].token_type)
            {
                if (TokenStream[InputPointer].token_type == Token_Class.Or)
                {
                    cond.Children.Add(match(Token_Class.Or));
                }

                else if (TokenStream[InputPointer].token_type == Token_Class.And)
                {
                    cond.Children.Add(match(Token_Class.And));
                }

                cond.Children.Add(Condition());
            }
            else
                return null;

            return cond;
        }

        bool resetInputPtr(int x, Node a)
        {
            InputPointer -= InputPointer - x;

            if (a != null)
            {
                return true;
            }
            else return false;
        }

        // Implement your logic here

        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " and " +
                        TokenStream[InputPointer].token_type.ToString() +
                        "  found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
               if(ExpectedToken.ToString() == "Main")
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                       + ExpectedToken.ToString() + "\r\n");
                    InputPointer+=5;
                    return null;
                }
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString()  + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
