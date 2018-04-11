using System;
using System.Linq;
using System.Text;
using DotVVM.TypeScript.Compiler.Translators.Operations;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsFormattingVisitor : ITsNodeVisitor
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

        public void VisitAssignmentStatement(TsAssignmentSyntax assignment)
        {
            Indent();
            if (assignment.Reference is TsPropertyReferenceSyntax propertyReference)
            {
                propertyReference.Identifier.AcceptVisitor(this);
                AppendOperator("(", false ,false);
            }
            else
            {
                assignment.Reference.AcceptVisitor(this);
                AppendOperator("=");
            }
            assignment.Expression.AcceptVisitor(this);
            if (assignment.Reference is TsPropertyReferenceSyntax)
            {
                AppendOperator(")", false , false);
            }
            EndStatement();
        }

        public void VisitBinaryOperation(TsBinaryOperationSyntax binaryOperation)
        {
            binaryOperation.LeftExpression.AcceptVisitor(this);
            AppendOperator(binaryOperation.Operator.ToDisplayString());
            binaryOperation.RightExpression.AcceptVisitor(this);
        }

        public void VisitBlockStatement(TsBlockSyntax block)
        {
            Indent();
            Append("{");
            AppendNewline();
            foreach (var statement in block.Statements)
            {
                statement.AcceptVisitor(this);
            }
            Indent();
            Append("}");
            AppendNewline();
        }

        public void VisitClassDeclaration(TsClassDeclarationSyntax classDeclaration)
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

        public void VisitConditionalExpression(TsConditionalExpressionSyntax conditionalExpression)
        {
            conditionalExpression.Condition.AcceptVisitor(this);
            AppendOperator("?");
            conditionalExpression.WhenTrue.AcceptVisitor(this);
            AppendOperator(":");
            conditionalExpression.WhenFalse.AcceptVisitor(this);
        }

        public void VisitDoWhileStatement(TsDoWhileStatementSyntax doWhileStatement)
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
            EndStatement();
        }

        public void VisitForStatement(TsForStatementSyntax forStatement)
        {
            Indent();
            var indent = currentIndent;
            currentIndent = 0;
            appendNewLines = false;
            Append("for(");
            forStatement.BeforeStatement.AcceptVisitor(this);
            forStatement.Condition.AcceptVisitor(this);
            EndStatement();
            forStatement.AfterExpression.AcceptVisitor(this);
            Append(")");
            appendNewLines = true;
            currentIndent = indent;
            IncreaseIndent();
            forStatement.Body.AcceptVisitor(this);
            DecreaseIndent();
        }

        public void VisitIdentifierReference(TsIdentifierReferenceSyntax reference)
        {
            reference.Identifier.AcceptVisitor(this);
        }

        public void VisitIdentifier(TsIdentifierSyntax identifier)
        {
            Append(identifier.Value);
        }

        public void VisitIfStatement(TsIfStatementSyntax ifStatement)
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

        public void VisitIncrementOrDecrementOperation(TsIncrementOrDecrementSyntax incrementOrDecrement)
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

        public void VisitLiteral(TsLiteralExpressionSyntax literal)
        {
            Append(literal.Value);
        }

        public void VisitLocalVariableDeclaration(TsLocalVariableDeclarationSyntax declaration)
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
            EndStatement();
        }

        public void VisitMethodDeclaration(TsMethodDeclarationSyntax methodDeclaration)
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

        public void VisitNamespaceDeclaration(TsNamespaceDeclarationSyntax namespaceDeclaration)
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

        public void VisitParameter(TsParameterSyntax parameter)
        {
            parameter.Identifier.AcceptVisitor(this);
            AppendOperator(":");
            parameter.Type.AcceptVisitor(this);
        }

        public void VisitParenthesizedExpression(TsParenthesizedExpressionSyntax expression)
        {
            Append("(");
            expression.Expression.AcceptVisitor(this);
            Append(")");
        }

        public void VisitPropertyDeclaration(TsPropertyDeclarationSyntax propertyDeclaration)
        {
            Indent();
            Append(propertyDeclaration.Modifier.ToDisplayString());
            AppendSpace();
            propertyDeclaration.Identifier.AcceptVisitor(this);
            Append(":");
            AppendSpace();
            Append("KnockoutObservable<");
            propertyDeclaration.Type.AcceptVisitor(this);
            Append(">");
            EndStatement();
        }

        public void VisitReturnStatement(TsReturnStatementSyntax returnStatement)
        {
            Indent();
            Append("return");
            returnStatement.Expression?.AcceptVisitor(this);
            EndStatement();
        }

        public void VisitType(TsTypeSyntax typeSyntax)
        {
            if(typeSyntax.EquivalentSymbol.IsValueType)
                Append("number");
            else
                Append(typeSyntax.EquivalentSymbol.Name);
        }

        public void VisitUnaryOperation(TsUnaryOperationSyntax unaryOperation)
        {
            AppendOperator(unaryOperation.Operator.ToDisplayString(), true, false);
            unaryOperation.Operand.AcceptVisitor(this);
        }

        public void VisitVariableDeclarator(TsVariableDeclaratorSyntax variableDeclarator)
        {
            variableDeclarator.Identifier.AcceptVisitor(this);
            if (variableDeclarator.Expression != null)
            {
                AppendOperator("=");
                variableDeclarator.Expression.AcceptVisitor(this);
            }
        }

        public void VisitWhileStatement(TsWhileStatementSyntax whileStatement)
        {
            Indent();
            Append("while (");
            whileStatement.Condition.AcceptVisitor(this);
            Append(") ");
            IncreaseIndent();
            whileStatement.Body.AcceptVisitor(this);
            DecreaseIndent();
        }

        public void VisitPropertyReference(TsPropertyReferenceSyntax tsPropertyReferenceSyntax)
        {
            tsPropertyReferenceSyntax.Identifier.AcceptVisitor(this);
            Append("(");
            Append(")");
        }

        public void AcceptMethodCall(TsMethodCallSyntax methodCall)
        {
            methodCall.Name.AcceptVisitor(this);
            Append("(");
            foreach (var parameter in methodCall.Parameters)
            {
                parameter.AcceptVisitor(this);
            }
            Append(")");
        }
    }
}
