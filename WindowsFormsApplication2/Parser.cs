using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataStructure;


namespace WindowsFormsApplication2
{
    //[System.Diagnostics.DebuggerStepThrough]
    static public class Parser
    {
        static public int Counter = 0;
        static public Scanner.Token Current = new Scanner.Token();
        static Node RootNode;
        static public bool Done = true;
        static public int NewNestedBlocks = 0;
        static int Count = 0;
        

        static public bool IsNumeric(object Expression)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
        static public void Adjust(Node Root)
        {
            if (IsNumeric(Root.Text)) Root.Text = "NUM (" + Root.Text + ")";
            else if (!Root.Text.ToLower().Contains("op")) Root.Text = "ID (" + Root.Text + ")";
            if (Root.Children.Count > 0) Adjust(Root.Children.First());
            if (Root.Children.Count > 1) Adjust(Root.Children.Last());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        static public Node GetNodesFromBrackets()
        {
            List<Scanner.Token> Tokens = Scanner.Result;
            Node[] Draft = new Node[Tokens.Count];
            List<Scanner.Token> DraftList = new List<Scanner.Token>();
            #region Get All Expression
            for (int i = Counter; i < Scanner.Result.Count; i++)
            {

                if (Current.TokenType == "LPAREN") Count++;
                DraftList.Add(Tokens[i]);
                if (Current.TokenType == "RPAREN")
                {
                    Count--;
                    if (Count == 0) break;
                }
                GetNextToken();
                if (Counter >= Scanner.Result.Count) throw new Exception("Syntax Error - You didnt close all the open brackets");
            }

            if (DraftList.Count == 3 && DraftList.First().TokenValue == "(" && DraftList.Last().TokenValue == ")")
            {
                Node Sole = new Node(DraftList[1].TokenValue);
                Sole.IsOval = true;
                return Sole;
            }
            #endregion
            #region Translate tokens to string
            string x = "";
            foreach (Scanner.Token Found in DraftList)
            {
                x += (Found.TokenType == "NUM") ? "N" : (Found.TokenType == "ID") ? "I" : Found.TokenValue;
            }
            #endregion
            #region Remove useless brackets
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] == '(' && (x.IndexOf(')', i) - i == 2 || x.IndexOf(')', i) - i == 1))
                {

                    DraftList.RemoveAt(x.IndexOf(')', i));
                    DraftList.RemoveAt(i);
                    x = x.Remove(x.IndexOf(')', i), 1);
                    x = x.Remove(i, 1);
                    i = -1;
                }
            }
            #endregion
            #region Distribute
            Node Root = new Node("");
            string Text = "";
            bool Reached = false;
            List<Scanner.Token> LeftTerm = new List<Scanner.Token>();
            List<Scanner.Token> RightTerm = new List<Scanner.Token>();
            string SLeftTerm = "";
            string SRightTerm = "";
            if (x.Contains("<") || x.Contains(">") || x.Contains("="))
            {
                if (x.Contains("<")) Text = "<";
                if (x.Contains(">")) Text = ">";
                if (x.Contains("=")) Text = "=";
                Root = new DataStructure.Node("OP " + Text);
                Root.IsOval = true;
                foreach (Scanner.Token Found in DraftList)
                {
                    if (Reached == false && Found.TokenValue != Text) LeftTerm.Add(Found);
                    else if (Reached == true && Found.TokenValue != Text) RightTerm.Add(Found);
                    else if (Found.TokenValue == Text)
                        Reached = true;
                }
                SLeftTerm = x.Substring(0, x.IndexOf(Text));
                SRightTerm = x.Substring(x.IndexOf(Text) + 1);
            }
            else
            {
                LeftTerm = DraftList;
                SLeftTerm = x;
            }
            #endregion
            if (LeftTerm.Count == 0 || SLeftTerm == "") return new Node("");
            Node LeftRoot = new Node("");
            #region Left
            Dictionary<string, Node> Nodes = new Dictionary<string, Node>();

            int Index = 0;

            for (int i = 0; i < SLeftTerm.Length; i++)
            {
                if (SLeftTerm[i] == '(' && SLeftTerm.IndexOf(')', i) - i == 4)
                {

                    Node Op = new Node("OP " + LeftTerm[SLeftTerm.IndexOf(SLeftTerm.Substring(i, 5)) + 2].TokenValue);
                    Op.IsOval = true;
                    Node First = new Node(LeftTerm[SLeftTerm.IndexOf(SLeftTerm.Substring(i, 5)) + 1].TokenValue);
                    First.IsOval = true;
                    Op.Children.Add(First);
                    Node Second = new Node(LeftTerm[SLeftTerm.IndexOf(SLeftTerm.Substring(i, 5)) + 3].TokenValue);
                    Second.IsOval = true;
                    Op.Children.Add(Second);
                    Nodes.Add("@ab" + Index, Op);
                    SLeftTerm = SLeftTerm.Remove(i, 5);

                    SLeftTerm = SLeftTerm.Insert(i, "@"); //here
                    LeftTerm.RemoveRange(i, 5);
                    Scanner.Token s = new Scanner.Token();
                    s.TokenValue = "@ab" + Index.ToString();
                    LeftTerm.Insert(i, s);
                    i = -1;
                    Index++;
                }
                else
                {
                    try
                    {
                        if (SLeftTerm.IndexOf(")", i) == -1) throw new Exception("Syntax Error - You didnt close all the open brackets");
                        if (SLeftTerm[i] == '(' && !CheckForbidden(SLeftTerm.Substring(i + 1, SLeftTerm.IndexOf(")", i) - (i + 1))) && SLeftTerm.Substring(i, (SLeftTerm.IndexOf(")", i) - i) + 1).Length != SLeftTerm.Length)
                        {
                            TranslateEq(SLeftTerm.Substring(i + 1, SLeftTerm.IndexOf(")", i) - (i + 1)), ref Index, LeftTerm.GetRange(i + 1, (SLeftTerm.IndexOf(")", i) - (i + 1))), ref Nodes);
                            Index--;
                            LeftTerm.RemoveRange(i, (SLeftTerm.IndexOf(")", i) - i) + 1);
                            SLeftTerm = SLeftTerm.Remove(i, (SLeftTerm.IndexOf(")", i) - i) + 1);
                            SLeftTerm = SLeftTerm.Insert(i, "@");
                            Scanner.Token s = new Scanner.Token();
                            s.TokenValue = "@ab" + Index.ToString();
                            LeftTerm.Insert(i, s);
                            i = SLeftTerm.Length; //hreere
                            Index++;
                        }
                    }
                    catch
                    {

                    }

                }

            }
            SLeftTerm = SLeftTerm.Replace("(", "").Replace(")", "");
            for (int i = 0; i < LeftTerm.Count; i++)
            {
                if (LeftTerm[i].TokenValue == "(" || LeftTerm[i].TokenValue == ")")
                {
                    LeftTerm.RemoveAt(i);
                    i = -1;
                }
            }//here

            for (int i = 0; SLeftTerm.Length != 1; i += 3) //here
            {
                CheckBracketsSyntax(SLeftTerm, LeftTerm);
                Node Op = new Node("OP " + LeftTerm[SLeftTerm.LastIndexOf(SLeftTerm.Substring(i, 3)) + 1].TokenValue);
                Op.IsOval = true;
                Node First = new Node(LeftTerm[SLeftTerm.LastIndexOf(SLeftTerm.Substring(i, 3))].TokenValue);
                First.IsOval = true;
                Op.Children.Add(First);
                Node Second = new Node(LeftTerm[SLeftTerm.LastIndexOf(SLeftTerm.Substring(i, 3)) + 2].TokenValue);
                Second.IsOval = true;
                Op.Children.Add(Second);
                Nodes.Add("@ab" + Index, Op);
                SLeftTerm = SLeftTerm.Remove(i, 3);
                SLeftTerm = SLeftTerm.Insert(i, "@"); //here
                LeftTerm.RemoveRange(i, 3);
                Scanner.Token s = new Scanner.Token();
                s.TokenValue = "@ab" + Index.ToString();
                LeftTerm.Insert(i, s);
                i = -3; //hreere
                Index++;
            }
            //Node Temp;
            //Node TTemp;
            //string t;
            if (Nodes.Count == 0 && LeftTerm.Count == 1)
            {
                LeftRoot = new Node(LeftTerm[0].TokenValue);
                LeftRoot.IsOval = true;
            }
            else if (Nodes.Count > 0)
            {
                AssignNodes(Nodes.Last().Value, ref Nodes);
                LeftRoot = Nodes.Last().Value;

            }
            Count = 0;
            #endregion
            Node RightRoot = new Node("");
            if (RightTerm.Count > 0 && SRightTerm != "")
            {
                #region Right
                Nodes = new Dictionary<string, Node>();
                Index = 0;
                for (int i = 0; i < SRightTerm.Length; i++)
                {
                    if (SRightTerm[i] == '(' && SRightTerm.IndexOf(')', i) - i == 4)
                    {
                        Node Op = new Node("OP " + RightTerm[SRightTerm.IndexOf(SRightTerm.Substring(i, 5)) + 2].TokenValue);
                        Op.IsOval = true;
                        Node First = new Node(RightTerm[SRightTerm.IndexOf(SRightTerm.Substring(i, 5)) + 1].TokenValue);
                        First.IsOval = true;
                        Op.Children.Add(First);
                        Node Second = new Node(RightTerm[SRightTerm.IndexOf(SRightTerm.Substring(i, 5)) + 3].TokenValue);
                        Second.IsOval = true;
                        Op.Children.Add(Second);
                        Nodes.Add("@ab" + Index, Op);
                        SRightTerm = SRightTerm.Remove(i, 5);
                        SRightTerm = SRightTerm.Insert(i, "@");
                        RightTerm.RemoveRange(i, 5);
                        Scanner.Token s = new Scanner.Token();
                        s.TokenValue = "@ab" + Index.ToString();
                        RightTerm.Insert(i, s);
                        i = -1;
                        Index++;
                    }
                    else
                    {
                        try
                        {
                            if (SRightTerm.IndexOf(")", i) == -1) throw new Exception("Syntax Error - You didnt close all the open brackets");
                            if (SRightTerm[i] == '(' && !CheckForbidden(SRightTerm.Substring(i + 1, SRightTerm.IndexOf(")", i) - (i + 1))) && SRightTerm.Substring(i, (SRightTerm.IndexOf(")", i) - i) + 1).Length != SRightTerm.Length)
                            {
                                TranslateEq(SRightTerm.Substring(i + 1, SRightTerm.IndexOf(")", i) - (i + 1)), ref Index, RightTerm.GetRange(i + 1, (SRightTerm.IndexOf(")", i) - (i + 1))), ref Nodes);
                                Index--;
                                RightTerm.RemoveRange(i, (SRightTerm.IndexOf(")", i) - i) + 1);
                                SRightTerm = SRightTerm.Remove(i, (SRightTerm.IndexOf(")", i) - i) + 1);
                                SRightTerm = SRightTerm.Insert(i, "@");
                                Scanner.Token s = new Scanner.Token();
                                s.TokenValue = "@ab" + Index.ToString();
                                RightTerm.Insert(i, s);
                                i = SRightTerm.Length; //hreere
                                Index++;
                            }
                        }
                        catch { }
                    }
                }
                SRightTerm = SRightTerm.Replace("(", "").Replace(")", "");
                for (int i = 0; i < RightTerm.Count; i++)
                {
                    if (RightTerm[i].TokenValue == "(" || RightTerm[i].TokenValue == ")")
                    {
                        RightTerm.RemoveAt(i);
                        i = -1;
                    }
                }//here

                for (int i = 0; SRightTerm.Length != 1; i += 3)
                {
                    CheckBracketsSyntax(SRightTerm, RightTerm);
                    Node Op = new Node("OP " + RightTerm[SRightTerm.LastIndexOf(SRightTerm.Substring(i, 3)) + 1].TokenValue);
                    Op.IsOval = true;
                    Node First = new Node(RightTerm[SRightTerm.LastIndexOf(SRightTerm.Substring(i, 3))].TokenValue);
                    First.IsOval = true;
                    Op.Children.Add(First);
                    Node Second = new Node(RightTerm[SRightTerm.LastIndexOf(SRightTerm.Substring(i, 3)) + 2].TokenValue);
                    Second.IsOval = true;
                    Op.Children.Add(Second);
                    Nodes.Add("@ab" + Index, Op);
                    SRightTerm = SRightTerm.Remove(i, 3);
                    SRightTerm = SRightTerm.Insert(i, "@");
                    RightTerm.RemoveRange(i, 3);
                    Scanner.Token s = new Scanner.Token();
                    s.TokenValue = "@ab" + Index.ToString();
                    RightTerm.Insert(i, s);
                    i = -3;
                    Index++;
                }
                if (Nodes.Count == 0 && RightTerm.Count == 1)
                {
                    RightRoot = new Node(RightTerm[0].TokenValue);
                    RightRoot.IsOval = true;
                }
                else if (Nodes.Count > 0)
                {
                    AssignNodes(Nodes.Last().Value, ref Nodes);
                    RightRoot = Nodes.Last().Value;

                }
                Count = 0;
                #endregion
            }
            else return LeftRoot;
            Root.Children.Add(LeftRoot);
            Root.Children.Add(RightRoot);
            return Root;
            //treeStructure1.AddRootNode(Root);
            //treeStructure1.NodeInBox = true;
            //treeStructure1.DrawNodes();

        }
        static public void CheckBracketsSyntax(string STerm, List<Scanner.Token> Term)
        {
            if (STerm.Length == 0) return;
            if (STerm.Length != 0 && Term.Count == 0) throw new Exception("Syntax Error - You cannot perform an operation with null");
            if (STerm.Length < 3 || Term.Count < 3) throw new Exception("Syntax Error - You cannot perform an operation with null");
        }
        static public void TranslateEq(string STerm, ref int Index, List<Scanner.Token> Term, ref Dictionary<string, Node> Nodes)
        {

            for (int i = 0; i >= 0; i += 3) //here
            {
                CheckBracketsSyntax(STerm, Term);
                Node Op = new Node("OP " + Term[STerm.LastIndexOf(STerm.Substring(i, 3)) + 1].TokenValue);
                Op.IsOval = true;
                Node First = new Node(Term[STerm.LastIndexOf(STerm.Substring(i, 3))].TokenValue);
                First.IsOval = true;
                Op.Children.Add(First);
                Node Second = new Node(Term[STerm.LastIndexOf(STerm.Substring(i, 3)) + 2].TokenValue);
                Second.IsOval = true;
                Op.Children.Add(Second);
                Nodes.Add("@ab" + Index, Op);
                STerm = STerm.Remove(i, 3);
                STerm = STerm.Insert(i, "@"); //here
                Term.RemoveRange(i, 3);
                Scanner.Token s = new Scanner.Token();
                s.TokenValue = "@ab" + Index.ToString();
                Term.Insert(i, s);
                i = -3; //hreere
                Index++;
                if (STerm == "@") break;
            }
        }
        static public bool CheckForbidden(string Text)
        {
            string Forbidden = "(,),<,>,=";
            foreach (char i in Text)
            {
                if (Forbidden.Contains(i)) return true;
            }
            return false;
        }
        static public void AssignNodes(Node CurrentNode, ref Dictionary<string, Node> Nodes)
        {
            if (CurrentNode.Children.First().Text.Contains("@ab"))
            {
                Node temp;
                Nodes.TryGetValue(CurrentNode.Children.First().Text, out temp);
                ReplaceNode(CurrentNode, temp, CurrentNode.Children.First().Text);
                AssignNodes(CurrentNode.Children.First(), ref Nodes);
            }
            if (CurrentNode.Children.Last().Text.Contains("@ab"))
            {
                Node temp;
                Nodes.TryGetValue(CurrentNode.Children.Last().Text, out temp);
                ReplaceNode(CurrentNode, temp, CurrentNode.Children.Last().Text);
                AssignNodes(CurrentNode.Children.Last(), ref Nodes);
            }
        }
        static public void ReplaceNode(Node Root, Node Child, string Key)
        {
            if (Root.Children.First().Text == Key)
            {
                Root.Children.RemoveAt(0);
                Root.Children.Insert(0, Child);
            }
            else if (Root.Children.Last().Text == Key)
            {
                Root.Children.RemoveAt(Root.Children.Count - 1);
                Root.Children.Add(Child);
            }

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////

        static void IsNested(ref Node node)
        {
            if (NewNestedBlocks == 0)
            {
                node.IsInnerRoot = false;
                node.IsInitialInnerRoot = false;
            }
            else
            {
                if (NewNestedBlocks > 0 && Done == false)
                {
                    node.IsInnerRoot = true;
                    node.IsInitialInnerRoot = true;
                }
                else if (NewNestedBlocks > 0 && Done == true)
                {
                    node.IsInnerRoot = true;
                    node.IsInitialInnerRoot = false;
                }
            }
        }

        public static void DrawTree(TreeStructure TreeStruct)
        {
            TreeStruct.NodeInBox = true;
            foreach (Node Found in RootNode.Children)
            {
                TreeStruct.AddRootNode(Found);
            }
            TreeStruct.DrawNodes();
        }

        public static void GetNextToken()
        {
            if (Counter != Scanner.Result.Count - 1)
            {
                Counter++;
                Current = Scanner.Result.ElementAt(Counter);
            }
        }

        static void Check_Grammer(string fn)
        {
            switch (fn)
            {
                case "Statement":
                    if (Current.TokenType == "IF" || Current.TokenType == "REPEAT" || Current.TokenType == "ID" || Current.TokenType == "READ" || Current.TokenType == "WRITE")
                        break;
                    else
                        throw new Exception("Using wrong token as a start of a Statement : '" + Current.TokenValue + "'");

                case "SEMI":
                    throw new Exception("Can not use a semicolon at the end of Statement sequence!");
                case "WrongToken":
                    throw new Exception("Wrong token passed to Factor Function: '" + Current.TokenValue + "'");

                case "Two Consecutive Operations":
                    throw new Exception("can not use two consecutive operations: '" + Current.TokenValue + "'");
                case "else&end":
                    throw new Exception("'else' or 'end' expected in if Statement at: '" + Current.TokenValue + "'");
                case "end":
                    throw new Exception("'end' expected in if Statement at: '" + Current.TokenValue + "'");
                case "then":
                    throw new Exception("'then' expected in if Statement at: '" + Current.TokenValue + "'");
                case "Missing Semi":
                    throw new Exception("'semicolon' expected after Statement at: '" + Current.TokenValue + "'");
                case "readID":
                    throw new Exception("There should be an identifier after 'read' Statement at: '" + Current.TokenValue + "'");
                case "NoRepeatBody":
                    throw new Exception("Repeat Statement must have a body before 'until'");
                case "Empty Parentheses":
                    throw new Exception("You can't use empty parentheses at: '" + Current.TokenValue + "'");

            }

        }

        static Node Factor(ref Node Parent)
        {
            Node x = new Node("Empty");

            if (Current.TokenType == "LPAREN")
            {
                x = GetNodesFromBrackets();
                if (x.Text == "")
                    Check_Grammer("Empty Parentheses");
                Adjust(x);
            }
            else if (Current.TokenType == "NUM")
            {
                //draw
                x = new Node(Current.TokenType + " (" + Current.TokenValue + ") ");

            }
            else if (Current.TokenType == "ID")
            {
                //draw
                x = new Node(Current.TokenType + " (" + Current.TokenValue + ") ");
            }
            else
            {
                Check_Grammer("WrongToken");
            }

            x.IsOval = true;
            return x;
        }

        static Node Term(ref Node Parent)
        {
            Node first = Factor(ref Parent);

            if (Counter != Scanner.Result.Count - 1)
            {
                Scanner.Token temp = Scanner.Result.ElementAt(Counter + 1);

                while (temp.TokenType == "TIMES" || temp.TokenType == "OVER")
                {
                    GetNextToken();
                    //draw
                    Node op_Node = new Node("OP " + Current.TokenValue);
                    op_Node.IsOval = true;
                    op_Node.Children.Add(first);
                    GetNextToken();
                    if (Current.TokenType == "PLUS" || Current.TokenType == "MINUS" || Current.TokenType == "TIMES" || Current.TokenType == "OVER" || Current.TokenType == "LT" || Current.TokenType == "EQ")
                        Check_Grammer("Two Consecutive Operations");
                    else
                    {
                        Node second = Factor(ref Parent);
                        op_Node.Children.Add(second);

                        //save the samll tree in first var
                        first = op_Node;
                        if (Counter != Scanner.Result.Count - 1)
                            temp = Scanner.Result.ElementAt(Counter + 1);
                        else
                            temp = Current;
                    }
                }
            }
            return first;
        }

        static Node Simple_Exp(ref Node Parent)
        {
            Node first = Term(ref Parent);

            if (Counter != Scanner.Result.Count - 1)
            {
                Scanner.Token temp = Scanner.Result.ElementAt(Counter + 1);

                while (temp.TokenType == "PLUS" || temp.TokenType == "MINUS")
                {
                    GetNextToken();
                    //draw
                    Node op_Node = new Node("OP " + Current.TokenValue);
                    op_Node.IsOval = true;
                    op_Node.Children.Add(first);

                    GetNextToken();
                    if (Current.TokenType == "PLUS" || Current.TokenType == "MINUS" || Current.TokenType == "TIMES" || Current.TokenType == "OVER" || Current.TokenType == "LT" || Current.TokenType == "EQ")
                        Check_Grammer("Two Consecutive Operations");
                    else
                    {
                        Node second = Term(ref Parent);
                        op_Node.Children.Add(second);

                        //save the samll tree in first var
                        first = op_Node;
                        if (Counter != Scanner.Result.Count - 1)
                            temp = Scanner.Result.ElementAt(Counter + 1);
                        else
                            temp = Current;
                    }
                }
            }

            return first;
        }



        static void Exp(ref Node Parent)
        {
            Node first = Simple_Exp(ref Parent);

            if (Counter != Scanner.Result.Count - 1)
            {

                Scanner.Token temp = Scanner.Result.ElementAt(Counter + 1);

                if (temp.TokenType == "LT" || temp.TokenType == "EQ")
                {
                    GetNextToken();
                    //draw
                    Node op_Node = new Node("OP " + Current.TokenValue);
                    op_Node.IsOval = true;
                    op_Node.Children.Add(first);

                    GetNextToken();
                    if (Current.TokenType == "PLUS" || Current.TokenType == "MINUS" || Current.TokenType == "TIMES" || Current.TokenType == "OVER" || Current.TokenType == "LT" || Current.TokenType == "EQ")
                        Check_Grammer("Two Consecutive Operations");
                    else
                    {
                        Node second = Simple_Exp(ref Parent);
                        op_Node.Children.Add(second);
                        Parent.Children.Add(op_Node);
                    }
                }
                else
                {
                    Parent.Children.Add(first);
                }
            }
            else
            {
                Parent.Children.Add(first);
            }


            GetNextToken();

        }


        static void Write_Stmt(ref Node Parent)
        {
            //draw "write"
            Node write_Node = new Node("write");
            IsNested(ref write_Node);
            Parent.Children.Add(write_Node);
            GetNextToken();

            Exp(ref write_Node);
            Done = true;
        }

        static void Read_Stmt(ref Node Parent)
        {
            //draw "read"
            GetNextToken();
            //draw identifier with the read in the same node
            if (Current.TokenType != "ID")
                Check_Grammer("readID");
            else
            {
                Node read_Node = new Node("read (" + Current.TokenValue + ")");
                IsNested(ref read_Node);
                Parent.Children.Add(read_Node);
                GetNextToken();
                Done = true;
            }
        }

        static void Assign_Stmt(ref Node Parent)
        {
            //draw "Assign" word with the id in the same node
            Node assign_Node = new Node("assign (" + Current.TokenValue + ")");
            IsNested(ref assign_Node);
            Parent.Children.Add(assign_Node);
            GetNextToken();

            //again tp skip ":="
            GetNextToken();
            Exp(ref assign_Node);
            Done = true;
        }

        static void Repeat_Stmt(ref Node Parent)
        {
            //draw "repeat"
            Node repeat_Node = new Node("repeat");
            IsNested(ref repeat_Node);
            Parent.Children.Add(repeat_Node);
            NewNestedBlocks++;

            GetNextToken();
            if (Current.TokenType == "UNTIL")
            {
                Check_Grammer("NoRepeatBody");
            }

            while (Current.TokenType != "UNTIL")
            {
                Done = false;
                Stmt_Sequence(ref repeat_Node);
            }

            if (Current.TokenType == "UNTIL")
            {
                //draw until
                GetNextToken();
                Exp(ref repeat_Node);
            }

            NewNestedBlocks--;
            Done = true;

        }

        static void If_Stmt(ref Node Parent)
        {
            //draw "if" 
            Node if_Node = new Node("if");
            IsNested(ref if_Node);
            Parent.Children.Add(if_Node);
            NewNestedBlocks++;

            GetNextToken();
            Exp(ref if_Node);

            if (Current.TokenType == "THEN")
            {
                //draw "then"
                GetNextToken();
                Done = false;
                Stmt_Sequence(ref if_Node);

                if (Current.TokenType != "END")
                {
                    if (Current.TokenType == "ELSE")
                    {
                        //draw "else"
                        GetNextToken();
                        Done = false;
                        Stmt_Sequence(ref if_Node);

                        if(Current.TokenType != "END")
                            Check_Grammer("end");
                        GetNextToken();
                    }
                    else
                        Check_Grammer("else&end");

                }
                else
                    GetNextToken();

            }
            else
                Check_Grammer("then");

            NewNestedBlocks--;
            Done = true;
        }

        static void Statement(ref Node Parent)
        {
            Check_Grammer("Statement");

            switch (Current.TokenType)
            {
                case "IF":
                    If_Stmt(ref Parent);
                    break;
                case "REPEAT":
                    Repeat_Stmt(ref Parent);
                    break;
                case "ID":
                    Assign_Stmt(ref Parent);
                    break;
                case "READ":
                    Read_Stmt(ref Parent);
                    break;
                case "WRITE":
                    Write_Stmt(ref Parent);
                    break;

            }
        }

        static void Stmt_Sequence(ref Node Parent)
        {
            Statement(ref Parent);
            if (Current.TokenType != "SEMI" && Counter != Scanner.Result.Count - 1 && Current.TokenType != "ELSE" && Current.TokenType != "END" && Current.TokenType != "UNTIL")
                Check_Grammer("Missing Semi");

            while (Current.TokenType == "SEMI")
            {
                if (Current.TokenType == "SEMI" && Counter == Scanner.Result.Count - 1)
                {
                    Check_Grammer("SEMI");
                }
                else
                {
                    GetNextToken();
                    Statement(ref Parent);
                }
            }
        }

        public static void Program()
        {
            RootNode = new Node("Start");
            Stmt_Sequence(ref RootNode);
        }

    }
}