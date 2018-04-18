using System.Linq;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using DotVVM.TypeScript.Compiler.Symbols;

namespace DotVVM.TypeScript.Compiler.Ast.Visitors
{
    public class TsFormattingVisitor : INodeVisitor
    {
        public char IndentationCharacter { get; }
        public int IndentStep { get; }

        public TsFormattingVisitor() : this('\t', 1)
        {
        }

        public TsFormattingVisitor(char indentationCharacter, int indentStep)
        {
            IndentationCharacter = indentationCharacter;
            IndentStep = indentStep;
        }

        private StringBuilder output = new StringBuilder();
        private int currentIndent = 0;
        private bool appendNewLines = true;
        private void Append(string text)
        {
            output.Append(text);
        }

        private void AppendOperator(string @operator, bool addSpaceBefore = true, bool addSpaceAfter = true)
        {
            if (addSpaceBefore) AppendSpace();
            Append(@operator);
            if (addSpaceAfter) AppendSpace();
        }

        private void AppendSpace()
        {
            Append(" ");
        }

        private void AppendNewline()
        {
            Append("\n");
        }

        private void EndStatement(bool addNewLine = true)
        {
            output.Append(";");
            if (appendNewLines) AppendNewline();
        }

        private void Indent()
        {
            output.Append(new string(IndentationCharacter, currentIndent));
        }

        private void IncreaseIndent()
        {
            currentIndent += IndentStep;
        }

        private void DecreaseIndent()
        {
            currentIndent -= IndentStep;
        }

        public string GetOutput()
        {
            return output.ToString();
        }

        public void VisitAssignmentStatement(IAssignmentSyntax assignment)
        {
            Indent();
            if (assignment.Reference is IPropertyReferenceSyntax propertyReference)
            {
                propertyReference.Instance.AcceptVisitor(this);
                Append(".");
                propertyReference.Identifier.AcceptVisitor(this);
                AppendOperator("(", false ,false);
            }
            else if (assignment.Reference is IArrayElementReferenceSyntax arrayElementReferenceSyntax)
            {
                arrayElementReferenceSyntax.ArrayReference.AcceptVisitor(this);
                if (arrayElementReferenceSyntax.ArrayReference is IPropertyReferenceSyntax)
                {
                    Append("()");
                }
                Append("[");
                arrayElementReferenceSyntax.ItemExpression.AcceptVisitor(this);
                Append("]");
                Append("(");
            }
            else
            {
                assignment.Reference.AcceptVisitor(this);
                AppendOperator("=");
            }
            assignment.Expression.AcceptVisitor(this);
            if (assignment.Reference is IPropertyReferenceSyntax)
            {
                AppendOperator(")", false , false);
            }
        }

        public void VisitBinaryOperation(IBinaryOperationSyntax binaryOperation)
        {
            binaryOperation.LeftExpression.AcceptVisitor(this);
            AppendOperator(binaryOperation.Operator.ToDisplayString());
            binaryOperation.RightExpression.AcceptVisitor(this);
        }

        public void VisitBlockStatement(IBlockSyntax block)
        {
            Indent();
            Append("{");
            AppendNewline();
            foreach (var statement in block.Statements)
            {
                statement.AcceptVisitor(this);
                EndStatement();
            }
            Indent();
            Append("}");
            AppendNewline();
        }

        public void VisitClassDeclaration(IClassDeclarationSyntax classDeclaration)
        {
            Indent();
            Append("class");
            AppendSpace();
            classDeclaration.Identifier.AcceptVisitor(this);
            AppendSpace();
            Append("{");
            AppendNewline();
            IncreaseIndent();
            foreach (var member in classDeclaration.Members)
            {
                member.AcceptVisitor(this);
            }
            DecreaseIndent();
            Indent();
            Append("}");
            AppendNewline();
        }

        public void VisitConditionalExpression(IConditionalExpressionSyntax conditionalExpression)
        {
            conditionalExpression.Condition.AcceptVisitor(this);
            AppendOperator("?");
            conditionalExpression.WhenTrue.AcceptVisitor(this);
            AppendOperator(":");
            conditionalExpression.WhenFalse.AcceptVisitor(this);
        }

        public void VisitDoWhileStatement(IDoWhileStatementSyntax doWhileStatement)
        {
            Indent();
            Append("do");
            AppendNewline();
            IncreaseIndent();
            doWhileStatement.Body.AcceptVisitor(this);
            DecreaseIndent();
            Indent();
            Append("while(");
            doWhileStatement.Condition.AcceptVisitor(this);
            Append(")");
        }

        public void VisitForStatement(IForStatementSyntax forStatement)
        {
            Indent();
            var indent = currentIndent;
            currentIndent = 0;
            appendNewLines = false;
            Append("for(");
            forStatement.BeforeStatement.AcceptVisitor(this);
            Append(";");
            forStatement.Condition.AcceptVisitor(this);
            Append(";");
            forStatement.AfterExpression.AcceptVisitor(this);
            Append(")");
            appendNewLines = true;
            currentIndent = indent;
            IncreaseIndent();
            forStatement.Body.AcceptVisitor(this);
            DecreaseIndent();
        }

        public void VisitIdentifierReference(ILocalVariableReferenceSyntax reference)
        {
            reference.Identifier.AcceptVisitor(this);
        }

        public void VisitIdentifier(IIdentifierSyntax identifier)
        {
            Append(identifier.Value);
        }

        public void VisitIfStatement(IIfStatementSyntax ifStatement)
        {
            Indent();
            Append("if (");
            ifStatement.ConditionalExpression.AcceptVisitor(this);
            Append(")");
            AppendNewline();
            IncreaseIndent();
            ifStatement.TrueStatement.AcceptVisitor(this);
            DecreaseIndent();
            if (ifStatement.FalseStatement != null)
            {
                Indent();
                Append("else");
                AppendNewline();
                IncreaseIndent();
                ifStatement.FalseStatement.AcceptVisitor(this);
                DecreaseIndent();
            }
        }

        public void VisitIncrementOrDecrementOperation(IIncrementOrDecrementSyntax incrementOrDecrement)
        {
            var @operator = incrementOrDecrement.IsIncrement ? "++" : "--";
            if (incrementOrDecrement.IsPostfix)
            {
                incrementOrDecrement.Target.AcceptVisitor(this);
                AppendOperator(@operator, false, false);
            }
            else
            {
                AppendOperator(@operator, false, false);
                incrementOrDecrement.Target.AcceptVisitor(this);
            }
        }

        public void VisitLiteral(ILiteralExpressionSyntax literal)
        {
            Append(literal.Value);
        }

        public void VisitLocalVariableDeclaration(ILocalVariableDeclarationSyntax declaration)
        {
            Indent();
            Append("let");
            AppendSpace();
            foreach (var declarator in declaration.Declarators)
            {
                declarator.AcceptVisitor(this);
                if(declarator != declaration.Declarators.Last())
                    AppendOperator(",");
            }
        }

        public void VisitMethodDeclaration(IMethodDeclarationSyntax methodDeclaration)
        {
            Indent();
            Append(methodDeclaration.Modifier.ToDisplayString());
            AppendSpace();
            methodDeclaration.Identifier.AcceptVisitor(this);
            Append("(");
            foreach (var parameter in methodDeclaration.Parameters)
            {
                parameter.AcceptVisitor(this);
                if (parameter != methodDeclaration.Parameters.Last())
                    Append(",");
            }
            Append(")");
            AppendNewline();
            IncreaseIndent();
            methodDeclaration.Body.AcceptVisitor(this);
            DecreaseIndent();
        }

        public void VisitNamespaceDeclaration(INamespaceDeclarationSyntax namespaceDeclaration)
        {
            Append("namespace");
            AppendSpace();
            namespaceDeclaration.Identifier.AcceptVisitor(this);
            AppendOperator("{");
            AppendNewline();
            foreach (var tsClassDeclarationSyntax in namespaceDeclaration.Types)
            {
                tsClassDeclarationSyntax.AcceptVisitor(this);
            }
            AppendNewline();
            AppendOperator("}");
        }

        public void VisitParameter(IParameterSyntax parameter)
        {
            parameter.Identifier.AcceptVisitor(this);
            AppendOperator(":");
            parameter.Type.AcceptVisitor(this);
        }

        public void VisitParenthesizedExpression(IParenthesizedExpressionSyntax expression)
        {
            Append("(");
            expression.Expression.AcceptVisitor(this);
            Append(")");
        }

        public void VisitPropertyDeclaration(IPropertyDeclarationSyntax propertyDeclaration)
        {
            Indent();
            Append(propertyDeclaration.Modifier.ToDisplayString());
            AppendSpace();
            propertyDeclaration.Identifier.AcceptVisitor(this);
            Append(":");
            AppendSpace();
            if (propertyDeclaration.Type.EquivalentSymbol.IsArrayType() && !propertyDeclaration.Type.EquivalentSymbol.IsStringType())
            {
                Append("KnockoutObservableArray<");
            }
            else
            {
                Append("KnockoutObservable<");
            }
            propertyDeclaration.Type.AcceptVisitor(this);
            Append(">");
            EndStatement();
        }

        public void VisitReturnStatement(IReturnStatementSyntax returnStatement)
        {
            Indent();
            Append("return");
            returnStatement.Expression?.AcceptVisitor(this);
        }

        public void VisitType(ITypeSyntax typeSyntax)
        {
            Append(typeSyntax.EquivalentSymbol.GetTypescriptEquivalent());
        }

        public void VisitUnaryOperation(IUnaryOperationSyntax unaryOperation)
        {
            AppendOperator(unaryOperation.Operator.ToDisplayString(), true, false);
            unaryOperation.Operand.AcceptVisitor(this);
        }

        public void VisitVariableDeclarator(IVariableDeclaratorSyntax variableDeclarator)
        {
            variableDeclarator.Identifier.AcceptVisitor(this);
            if (variableDeclarator.Expression != null)
            {
                AppendOperator("=");
                variableDeclarator.Expression.AcceptVisitor(this);
            }
        }

        public void VisitWhileStatement(IWhileStatementSyntax whileStatement)
        {
            Indent();
            Append("while (");
            whileStatement.Condition.AcceptVisitor(this);
            Append(") ");
            IncreaseIndent();
            whileStatement.Body.AcceptVisitor(this);
            DecreaseIndent();
        }

        public void VisitPropertyReference(IPropertyReferenceSyntax propertyReference)
        {
            propertyReference.Instance.AcceptVisitor(this);
            Append(".");
            propertyReference.Identifier.AcceptVisitor(this);
            if (propertyReference.Type.IsArrayType() == false)
            {
                Append("(");
                Append(")");
            }
        }

        public void VisitMethodCall(IMethodCallSyntax methodCall)
        {
            if (methodCall.Object != null)
            {
                methodCall.Object?.AcceptVisitor(this);
                Append(".");
            }
            methodCall.Name.AcceptVisitor(this);
            Append("(");
            foreach (var parameter in methodCall.Arguments) 
            {
                parameter.AcceptVisitor(this);
            }
            Append(")");
        }

        public void VisitInstanceReference(IInstanceReferenceSyntax instanceReference)
        {
            instanceReference.Identifier.AcceptVisitor(this);
        }

        public void VisitParametrizedSyntaxNode(IRawSyntaxNode rawSyntaxNode)
        {
            Append(rawSyntaxNode.Value);
        }

        public void VisitArrayElementReference(IArrayElementReferenceSyntax arrayElementReferenceSyntax)
        {
            
            arrayElementReferenceSyntax.ArrayReference.AcceptVisitor(this);
            if (arrayElementReferenceSyntax.ArrayReference is IPropertyReferenceSyntax)
            {
                Append("()");
            }
            Append("[");
            arrayElementReferenceSyntax.ItemExpression.AcceptVisitor(this);
            Append("]");
            if (arrayElementReferenceSyntax.ArrayReference is IPropertyReferenceSyntax)
            {
                Append("()");
            }
        }

        public void VisitObjectCreationExpresion(IObjectCreationExpressionSyntax objectCreation)
        {
            Append("new ");
            objectCreation.ObjectType.AcceptVisitor(this);
            Append("(");
            foreach (var argument in objectCreation.Arguments)
            {
                argument.AcceptVisitor(this);
                if (argument != objectCreation.Arguments.Last())
                {
                    Append(",");
                }
            }
            Append(")");
        }
    }
}
