using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Impl;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.AspectLookupItems.BaseInfrastructure;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.AspectLookupItems.Behaviors;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.AspectLookupItems.Info;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.AspectLookupItems.Matchers;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.AspectLookupItems.Presentations;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Features.Intellisense.CodeCompletion.CSharp.Rules;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Conversions;
using JetBrains.ReSharper.Psi.CSharp.ExpectedTypes;
using JetBrains.ReSharper.Psi.CSharp.Resources;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharperPlugin.ArgumentsLineComplete
{
    [Language(typeof(CSharpLanguage))]
    public class CSharpMultipleArgumentsProvider : CSharpItemsProviderBase<CSharpCodeCompletionContext>
    {
        private readonly LocalSymbolFilter myLocalSymbolFilter = new();
        protected override bool IsAvailable(CSharpCodeCompletionContext context)
        {
            return context.BasicContext.CodeCompletionType == CodeCompletionType.BasicCompletion;
        }

        protected override bool AddLookupItems(CSharpCodeCompletionContext context, IItemsCollector collector)
        {
            var referenceExpression = context.UnterminatedContext.Reference?.GetTreeNode() as IReferenceExpression;
            var argument = CSharpArgumentNavigator.GetByValue(referenceExpression);
            if (argument is not { Mode: null, NameIdentifier: null })
            {
                return false;
            }

            var argumentList = ArgumentListNavigator.GetByArgument(argument);
            if (argumentList == null) return false;

            var symbolTable = referenceExpression.Reference.GetCompletionSymbolTable().Filter(myLocalSymbolFilter);
            var infos = symbolTable.GetAllSymbolInfos();
            
            var argumentIndex = argumentList.Arguments.IndexOf(argument);
            var overloads = GetAllSuitableParameters(argumentList, argumentIndex);
            if (overloads == null) return false;

            var typeConversionRule = argumentList.GetTypeConversionRule();

            var separator = GetSeparator(context);

            var items = CreateTextItems(infos, overloads, argumentIndex, typeConversionRule, separator);
            if (items.Count == 0) return false;
            
            var textLookupRanges = GetTextRanges(context, argumentList, referenceExpression);
            if (textLookupRanges == null) return false;

            var lookupItems = CreateLookupItems(items, textLookupRanges);
            foreach (var lookupItem in lookupItems)
            {
                collector.Add(lookupItem);
            }
            
            return true;
        }

        private static IEnumerable<LookupItem<TextualInfo>> CreateLookupItems([NotNull] IReadOnlyList<string> items, [NotNull] TextLookupRanges textLookupRanges)
        {
            for (var i = 0; i < items.Count; i++)
            {
                yield return CreateLookupItem(items[i], i);
            }

            LookupItem<TextualInfo> CreateLookupItem(string text, int index)
            {
                var info = new TextualInfo(text, text) { Ranges = textLookupRanges };
                info.Placement.Relevance |=
                    (ulong) (CLRLookupItemRelevance.NonStatic | CLRLookupItemRelevance.LocalVariablesAndParameters | CLRLookupItemRelevance.ExpectedTypeMatch)
                    | (ulong) (LookupItemRelevance.NameCorrelation | LookupItemRelevance.HighSelectionPriority);
                info.Placement.OrderString = $"_{index:D5}{info.Placement.OrderString}";

                var item = LookupItemFactory.CreateLookupItem(info)
                    .WithPresentation(_ =>
                        new TextPresentation<TextualInfo>(_.Info, PsiCSharpThemedIcons.AllParameters.Id,
                            emphasize: false))
                    .WithBehavior(_ => new MultipleArgumentBehaviour(_.Info))
                    .WithMatcher(_ => new TextualMatcher<TextualInfo>(_.Info.Text.Replace(",", "·").Replace(" ", "·"), _.Info));
                return item;
            }
        }
        
        [CanBeNull]
        private static TextLookupRanges GetTextRanges(
            [NotNull] CSharpCodeCompletionContext context, [NotNull] ITreeNode argumentList, [NotNull] IExpression referenceExpression)
        {
            var startOffset = context.TerminatedContext.ToOriginalTreeRange(new TreeTextRange(argumentList.GetTreeStartOffset()));
            var originalReferenceRange = context.TerminatedContext.ToDocumentRange(new TreeTextRange(referenceExpression.GetTreeStartOffset()));
            var containingNode = context.BasicContext.File.GetContainingNodeAt<IArgumentList>(startOffset.StartOffset);
            if (containingNode == null) return null;

            var elementRange = originalReferenceRange.SetEndTo(DocumentOffset.Max(containingNode.GetDocumentEndOffset(), context.BasicContext.SelectedRange.EndOffset));
            var textLookupRanges = CodeCompletionContextProviderBase.GetTextLookupRanges(context.BasicContext, elementRange);
            return textLookupRanges;
        }

        [NotNull]
        private static string GetSeparator([NotNull] CSharpCodeCompletionContext context)
        {
            const string space = " ";
            var separator = ",";

            if (context.SpaceBeforeComma)
            {
                separator = space + separator;
            }

            if (context.SpaceAfterComma)
            {
                separator += space;
            }

            return separator;
        }

        private IReadOnlyList<string> CreateTextItems([NotNull] IList<ISymbolInfo> argumentList,
            [NotNull] IReadOnlyList<DeclaredElementInstance<IParametersOwner>> overloads, int argumentIndex,
            [NotNull] ICSharpTypeConversionRule typeConversionRule, string separator)
        {
            var result = new List<IReadOnlyList<string>>();
            var argumentNameToDeclaredElementMap = argumentList.ToDictionary(x => x.ShortName, x => x);
            foreach (var overload in overloads)
            {
                var elementParameters = overload.Element.Parameters;
                if (elementParameters.Count - argumentIndex <= 1) continue;

                var elements = TryGetTextElements(elementParameters);
                if (elements == null) continue;
                result.Add(elements);
            }

            result.Sort((e1, e2) => e2.Count.CompareTo(e1.Count));
            return result.Select(list => list.Join(separator)).ToIReadOnlyList();

            [CanBeNull, Pure] 
            IReadOnlyList<string> TryGetTextElements([NotNull] IList<IParameter> elementParameters)
            {
                var elements = new List<string>();
                for (var index = 0; index + argumentIndex < elementParameters.Count;index++)
                {
                    var parameter = elementParameters[index + argumentIndex];
                    if (!(argumentNameToDeclaredElementMap.TryGetValue(parameter.ShortName, out var element)
                          && element.GetDeclaredElement().Type() is { } declaredElementType
                        && IsParameterTypeSuitable(parameter.Type, declaredElementType, element.GetSubstitution())))
                        return null;
                    
                    elements.Add(GetItemText(parameter, element));
                }

                return elements;

                bool IsParameterTypeSuitable([NotNull] IType contextParameter, [NotNull] IType candidateParameter, [NotNull] ISubstitution substitution)
                {
                    var candidateParameterType = substitution.Apply(candidateParameter);
                    var conversion = typeConversionRule.ClassifyImplicitConversion(contextParameter, candidateParameterType);
                    var conversionKind = conversion.Kind;
                    return conversionKind is ConversionKind.Identity or ConversionKind.ImplicitReference or ConversionKind.Boxing;
                }

                string GetItemText(IParameter parameter, [NotNull] ISymbolInfo argumentInfo)
                {
                    var keywordPrefix = parameter.Kind switch
                    {
                        ParameterKind.REFERENCE => "ref ",
                        ParameterKind.OUTPUT => "out ",
                        _ => string.Empty
                    };

                    return keywordPrefix + argumentInfo.GetDeclaredElement().ShortName;
                }
            }
        }
        

        [CanBeNull]
        private IReadOnlyList<DeclaredElementInstance<IParametersOwner>> GetAllSuitableParameters(
            [NotNull] IArgumentList argumentList, int argumentIndex)
        {
            var parameters = TryGetParametersFromExpression(argumentList, argumentIndex)
                             ?? TryGetParametersFromCtor(argumentList, argumentIndex)
                             ?? TryGetParametersFromIndexer(argumentList, argumentIndex);
            return parameters;
            
            static IReadOnlyList<DeclaredElementInstance<IParametersOwner>> TryGetParametersFromExpression([NotNull] IArgumentList argumentList, int argumentIndex)
            {
                var invocationExpression = InvocationExpressionNavigator.GetByArgumentList(argumentList);
                var invokedExpression = invocationExpression?.InvokedExpression.GetOperandThroughParenthesis() as IReferenceExpression;
                if (invokedExpression?.ConditionalQualifier is IBaseExpression) return null;
                return invocationExpression != null ? FindNotMatchedParameters(invocationExpression, argumentIndex) : null;
            }
            
            static IReadOnlyList<DeclaredElementInstance<IParametersOwner>> TryGetParametersFromCtor([NotNull] IArgumentList argumentList, int argumentIndex)
            {
                var objectCreationExpression = ObjectCreationExpressionNavigator.GetByArgumentList(argumentList);
                return objectCreationExpression == null ? null : FindNotMatchedParameters(objectCreationExpression, argumentIndex);
            }
            
            static IReadOnlyList<DeclaredElementInstance<IParametersOwner>> TryGetParametersFromIndexer([NotNull] IArgumentList argumentList, int argumentIndex)
            {
                var elementAccessExpression = ElementAccessExpressionNavigator.GetByArgumentList(argumentList);
                var qualifierExpression = elementAccessExpression?.ConditionalQualifier.GetOperandThroughParenthesis() as IBaseExpression;
                if (qualifierExpression != null) return null;
                return elementAccessExpression != null ? FindNotMatchedParameters(elementAccessExpression, argumentIndex) : null;
            }
        }
        

        [NotNull]
        private static IReadOnlyList<DeclaredElementInstance<IParametersOwner>> FindNotMatchedParameters([NotNull] ICSharpArgumentsOwner argumentsOwner, int argumentIndex)
        {
            var candidatesStrategy = new InvocationCandidatesStrategy(
                ProcessInvocationArgs.UP_TO, ProcessInvocationArgs.UP_TO, strictArgumentNumber: false);
            var invocationCandidatesEngine = new InvocationCandidatesEngine(argumentsOwner, argumentIndex, candidatesStrategy);

            var result = new LocalList<DeclaredElementInstance<IParametersOwner>>();
            foreach (var applicableInvocationCandidate in invocationCandidatesEngine.ApplicableCandidates)
            {
                if (applicableInvocationCandidate.MatchResultEndsWithInOrderArgument(argumentsOwner))
                {
                    result.Add(new DeclaredElementInstance<IParametersOwner>(applicableInvocationCandidate.Element, applicableInvocationCandidate.Substitution));
                }
            }

            return result.ReadOnlyList();
        }

        private sealed class LocalSymbolFilter : SimpleSymbolFilter
        {
            public override ResolveErrorType ErrorType => ResolveErrorType.OK;

            public override bool Accepts(IDeclaredElement declaredElement, ISubstitution substitution)
            {
                return declaredElement is ILocalVariable or IParameter;
            }
        }

        // uses to complete only first argument if user completes with comma
        private sealed class MultipleArgumentBehaviour : TextualBehavior<TextualInfo>
        {
            private bool myCompletedByComma; 

            public MultipleArgumentBehaviour([NotNull] TextualInfo info) : base(info)
            {
            }

            public override void Accept(ITextControl textControl, DocumentRange nameRange, LookupItemInsertType insertType, Suffix suffix, ISolution solution, bool keepCaretStill)
            {
                if (suffix.HasPresentation && suffix.Presentation == ',')
                {
                    myCompletedByComma = true;
                }

                base.Accept(textControl, nameRange, insertType, suffix, solution, keepCaretStill);
            }

            protected override DocumentRange DoReplaceText(ITextControl textControl, DocumentRange replaceRange, string typeInName, ref Suffix suffix)
            {
                if (myCompletedByComma)
                {
                    typeInName = typeInName.Split(',').First();
                }

                return base.DoReplaceText(textControl, replaceRange, typeInName, ref suffix);
            }
        }
    }
}